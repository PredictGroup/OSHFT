using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Runtime.CompilerServices; // MethodImpl
using System.Collections;
using System.Runtime.Serialization;
using StackExchange.Redis;
using SimpleMsgPack;
using ServiceStack;
using ServiceStack.Redis;

namespace OSHFT_Q_R
{
    public class RedisExchange : BaseExchange
    {
        const bool miniRedisExchange = true; // обмен данными с Redis в минимальном режиме

        // **********************************************************************
        static OSHFT_Q_RMain mainForm;

        RedisManagerPool redisManager = new RedisManagerPool(cfg.u.RedisPassword + "@" + cfg.u.RedisServerIP + ":" + cfg.u.RedisServerPort);

        DateTime firstTickDt = DateTime.Today.AddHours(7).AddMinutes(49).AddSeconds(59);
        DateTime lastTickDt = DateTime.Today.AddHours(23).AddMinutes(49).AddSeconds(59);

        TimeSpan lostConnectionInterval = new TimeSpan(0, 0, 10, 0, 000);

        DispatcherTimer refreshing;
        static bool working;

        double lastPrice = 0;

        // **********************************************************************

        private const string TickChannel = "ticks";

        // **********************************************************************

        IDatabase db;
        const string orders_key = "orders_to_market";
        const string orders_cancel_key = "orders_to_cancel";
        const string orders_consumer_group = "orders_cg";
        const string orders_consumer = "orders_consumer";

        // **********************************************************************

        // minutes - Id в таблице тайм фреймов
        public RedisExchange(string Name = "RedisExchange")
            : base(Name)
        {
            // + Redis
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                    { cfg.u.RedisServerIP, cfg.u.RedisServerPort }
                },
                KeepAlive = 180,
                User = cfg.u.RedisUser,
                Password = cfg.u.RedisPassword,
                DefaultVersion = new Version("2.8.5"),
                // Needed for cache clear
                AllowAdmin = true
            };
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
            db = redis.GetDatabase();

            try
            {
                db.StreamCreateConsumerGroup(orders_key, orders_consumer_group, StreamPosition.NewMessages);
                //db.StreamAdd(orders_key, "empty", "empty");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка. " + ex.Message);
            }


            // - Redis
            working = false;
            refreshing = new DispatcherTimer();
            refreshing.Interval = cfg.SbUpdateInterval;
            refreshing.Tick += new EventHandler(ordersFromRedis);

            refreshing.Start();
        }

        public static void SetMainForm(OSHFT_Q_RMain _mainForm)
        {
            mainForm = _mainForm;
            mainForm.LogToScreeen("Redis exchange client connected.");
        }

        // **********************************************************************

        public override void ProcessSpread(Spread spread)
        {
            if (miniRedisExchange)
                return;

            DateTime now = DateTime.Now;
            if (now < firstTickDt)
                return;

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = cfg.u.SecCode;
            msgpack.ForcePathObject("Ask").AsFloat = spread.Ask;
            msgpack.ForcePathObject("Bid").AsFloat = spread.Bid;
            //msgpack.ForcePathObject("DateTime").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "spreads", "*", "spread", packData);
            }
        }

        // **********************************************************************

        public override void ProcessQuotes(Quote[] quotes)
        {
            if (miniRedisExchange)
                return;

            DateTime now = DateTime.Now;
            if (now < firstTickDt)
                return;

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = cfg.u.SecCode;
            foreach (Quote qt in quotes)
            {
                msgpack.ForcePathObject("Quote.Price").AsArray.Add(qt.Price);
                msgpack.ForcePathObject("Quote.Volume").AsArray.Add(qt.Volume);
                msgpack.ForcePathObject("Quote.Type").AsArray.Add(qt.Type.ToString());
            }
            //msgpack.ForcePathObject("DateTime").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "quotes", "*", "quote", packData);
            }
        }

        // **********************************************************************

        public override void ProcessTick(Tick tick)
        {
            if (miniRedisExchange)
                return;

            if (tick.DateTime < firstTickDt)
                return;

            if(tick.SecCode == cfg.u.SecCode)
            {
                lastPrice = tick.RawPrice;
            }

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = tick.SecCode;
            msgpack.ForcePathObject("TradeNum").AsString = tick.TradeNum;
            msgpack.ForcePathObject("Price").AsFloat = tick.RawPrice;
            msgpack.ForcePathObject("Volume").AsFloat = tick.Volume;
            msgpack.ForcePathObject("Operation").AsString = tick.Op.ToString();
            msgpack.ForcePathObject("DateTime").AsString = tick.DateTime.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "ticks", "*", "tick", packData);
            }
        }

        // **********************************************************************

        public override void ProcessSetting(Setting setting)
        {
            if (miniRedisExchange)
                return;

            if (setting.DateTime < firstTickDt)
                return;

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = setting.SecCode;
            msgpack.ForcePathObject("ClassCode").AsString = setting.ClassCode;
            //msgpack.ForcePathObject("ClassName").AsString = setting.ClassName;
            msgpack.ForcePathObject("OptionBase").AsString = setting.OptionBase;
            msgpack.ForcePathObject("MatDate").AsString = setting.MatDate.ToString("o");
            msgpack.ForcePathObject("DaysToMatDate").AsInteger = setting.DaysToMatDate;
            msgpack.ForcePathObject("Numbids").AsInteger = setting.Numbids;
            msgpack.ForcePathObject("Numoffers").AsInteger = setting.Numoffers;
            msgpack.ForcePathObject("Biddeptht").AsInteger = setting.Biddeptht;
            msgpack.ForcePathObject("Offerdeptht").AsInteger = setting.Offerdeptht;
            msgpack.ForcePathObject("Voltoday").AsInteger = setting.Voltoday;
            msgpack.ForcePathObject("Valtoday").AsFloat = setting.Valtoday;
            msgpack.ForcePathObject("Numtrades").AsInteger = setting.Numtrades;
            msgpack.ForcePathObject("Numcontracts").AsInteger = setting.Numcontracts;
            msgpack.ForcePathObject("Selldepo").AsFloat = setting.Selldepo;
            msgpack.ForcePathObject("Buydepo").AsFloat = setting.Buydepo;
            msgpack.ForcePathObject("Strike").AsFloat = setting.Strike;
            msgpack.ForcePathObject("OptionType").AsString = setting.OptionType;
            msgpack.ForcePathObject("Volatility").AsFloat = setting.Volatility;
            msgpack.ForcePathObject("DateTime").AsString = setting.DateTime.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis 
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "settings", "*", "setting", packData);
            }
        }

        // **********************************************************************

        public override void ProcessTrade(Trade trade)
        {
            DateTime now = DateTime.Now;

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = trade.SecCode;
            msgpack.ForcePathObject("TradeNum").AsString = trade.TradeNum;
            msgpack.ForcePathObject("OrderNum").AsString = trade.OrderNum;
            msgpack.ForcePathObject("Price").AsFloat = trade.RawPrice;
            msgpack.ForcePathObject("Volume").AsInteger = trade.Quantity;
            msgpack.ForcePathObject("Operation").AsString = trade.Op.ToString();
            msgpack.ForcePathObject("DateTime").AsString = trade.DateTime.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis 
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "trades", "*", "trade", packData);
            }
            mainForm.LogToScreeen("Redis exchange: trade sent.");
        }

        // **********************************************************************

        public override void ProcessPutOrder(PutOrder putOrder)
        {
            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = putOrder.SecCode;
            msgpack.ForcePathObject("OrderNum").AsString = putOrder.OrderNum;
            msgpack.ForcePathObject("Price").AsFloat = putOrder.RawPrice;
            msgpack.ForcePathObject("Volume").AsInteger = putOrder.Quantity;
            msgpack.ForcePathObject("Balance").AsInteger = putOrder.Balance;
            msgpack.ForcePathObject("Value").AsFloat = putOrder.Value;
            msgpack.ForcePathObject("Operation").AsString = putOrder.Op.ToString();
            msgpack.ForcePathObject("Status").AsString = putOrder.Status.ToString();
            msgpack.ForcePathObject("DateTime").AsString = putOrder.PutDateTime.ToString("o");
            msgpack.ForcePathObject("WithdrawDateTime").AsString = putOrder.WithdrawDateTime.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis 
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "orders", "*", "order", packData);
            }
            mainForm.LogToScreeen("Redis exchange: order put.");
        }

        // **********************************************************************


        void ordersFromRedis(object sender, EventArgs e)
        {
            if (working)
                return;

            working = true;

            if (ExchangeManager.tm.Connected == TermConnection.None)
            {
                working = false;
                mainForm.LogToScreeen("Redis exchange: Quik doesnt connected.");
                //return;
            }

            // Orders Buy/Sell:

            StreamEntry[] vOwnOrder = db.StreamReadGroup(orders_key, orders_consumer_group, orders_consumer, StreamPosition.NewMessages, 1);

            if (vOwnOrder.Length == 0)
            {
                working = false;
                return; // Заявок на исполнение нет.
            }

            StreamEntry OwnOrderMessage = vOwnOrder[0];

            int order_id = 0;
            string symbol = "";
            string operation = "";
            DateTime datetime = DateTime.MinValue;
            double price = 0.0;
            int volume = 0;
            string version = "";

            foreach (NameValueEntry val in OwnOrderMessage.Values)
            {
                switch (val.Name.ToString())
                {
                    case "order_id":
                        order_id = (int)val.Value;
                        break;
                    case "symbol":
                        symbol = val.Value.ToString();
                        break;
                    case "operation":
                        operation = val.Value.ToString();
                        break;
                    case "datetime":
                        datetime = DateTime.Parse(val.Value.ToString());
                        break;
                    case "price":
                        price = (double)val.Value;
                        break;
                    case "volume":
                        volume = (int)val.Value;
                        break;
                    case "version":
                        version = val.Value.ToString();
                        break;
                }
            }

            if (symbol != cfg.u.SecCode)
            {
                working = false;
                mainForm.LogToScreeen("Order ticker " + OwnOrderMessage.Values.GetValue(1) + " wait for " + cfg.u.SecCode + ". Please, change configuration.");
                return;
            }

            DateTime localDateTime = datetime.AddHours(-2);
            if (!isDateTimeValid(localDateTime)) // если данные не актуальные, возврат
            {
                working = false;
                mainForm.LogToScreeen("Order locl datetime " + datetime.ToString() + " too older.");
                return;
            }

            double priceValue = 0;
            if (price == 0)
            {
                if (lastPrice == 0)
                {
                    mainForm.LogToScreeen("We have no price and lastPrice = 0.");
                    working = false;
                    return;
                }
                else
                {
                    priceValue = lastPrice;
                    if (operation == "Buy")
                        priceValue = priceValue + cfg.u.PriceStep * 1;
                    else
                        if (operation == "Sell")
                        priceValue = priceValue - cfg.u.PriceStep * 1;
                }
            }
            else
            {
                priceValue = price;
            }

            int qty = 0;
            if (volume == 0)
                qty = 1;
            else
                qty = volume;


            ulong OrderID = 0;
            switch (operation)
            {
                case "Buy": // buy

                    TermManager.Transaction tr = ExchangeManager.tm.ExecAction(new OwnAction(
                                                                                              TradeOp.Buy,
                                                                                              BaseQuote.Absolute,
                                                                                              priceValue,
                                                                                              qty));
                    while (tr.OId == 0)
                    {
                        mainForm.LogToScreeen("Transaction: continued.");
                        continue;
                    }
                    OrderID = tr.OId;
                    mainForm.LogToScreeen("Transaction: received OrderID"+ OrderID.ToString());
                    break;

                case "Sell": // sell

                    TermManager.Transaction tr1 = ExchangeManager.tm.ExecAction(new OwnAction(
                                                                                              TradeOp.Sell,
                                                                                              BaseQuote.Absolute,
                                                                                              priceValue,
                                                                                              qty));
                    while (tr1.OId == 0)
                    {
                        mainForm.LogToScreeen("Transaction: continued.");
                        continue;
                    }
                    OrderID = tr1.OId;
                    mainForm.LogToScreeen("Transaction: received OrderID" + OrderID.ToString());
                    break;
            }
            // Redis 
            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = symbol;
            msgpack.ForcePathObject("OrderNum").AsString = OrderID.ToString();
            msgpack.ForcePathObject("Order_ID").AsString = order_id.ToString();
            msgpack.ForcePathObject("TimeStamp").AsString = DateTime.Now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis 
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "order_nums_from_market", "*", "ordernum", packData);
            }

            var oAck = db.StreamAcknowledge(orders_key, orders_consumer_group, OwnOrderMessage.Id);
            mainForm.LogToScreeen("Order " + OrderID.ToString() + " is set to market.");


            // Cancel Orders:
            return; // пока не реализовано

            StreamEntry[] vCancelOrder = db.StreamReadGroup(orders_cancel_key, orders_consumer_group, orders_consumer, StreamPosition.NewMessages, 1); //StreamPosition.Beginning NewMessages

            if (vCancelOrder.Length == 0)
            {
                working = false;
                return; // Заявок на отмену нет.
            }

            StreamEntry CancelOrderMessage = vCancelOrder[0];

            ulong order_id_cancel = 0;
            ulong ordernum_cancel = 0;
            string symbol_cancel = "";
            DateTime datetime_cancel = DateTime.MinValue;
            string version_cancel = "";

            foreach (NameValueEntry val in CancelOrderMessage.Values)
            {
                switch (val.Name.ToString())
                {
                    case "order_id":
                        order_id_cancel = (ulong)val.Value;
                        break;
                    case "symbol":
                        symbol_cancel = val.Value.ToString();
                        break;
                    case " ordernum":
                        ordernum_cancel = (ulong)val.Value;
                        break;
                    case "datetime":
                        datetime_cancel = DateTime.Parse(val.Value.ToString());
                        break;
                    case "version":
                        version_cancel = val.Value.ToString();
                        break;
                }
            }

            if (symbol != cfg.u.SecCode)
            {
                working = false;
                mainForm.LogToScreeen("Order ticker " + OwnOrderMessage.Values.GetValue(1) + " wait for " + cfg.u.SecCode + ". Please, change configuration."); // Symbol
                return;
            }

            if (!isDateTimeValid(datetime)) // если данные не актуальные, возврат
            {
                working = false;
                mainForm.LogToScreeen("Order datetime " + datetime.ToString() + " too older.");
                return;
            }

            if (order_id_cancel != 0)    //cancel specific order by id
                ExchangeManager.tm.CancelAction(order_id_cancel);
            else // cancel all orders
                ExchangeManager.tm.ExecAction(new OwnAction(
                                                              TradeOp.Cancel,
                                                              BaseQuote.None,
                                                              0,
                                                              0)); // отменяем ордера

            var oAckCancel = db.StreamAcknowledge(orders_cancel_key, orders_consumer_group, CancelOrderMessage.Id);
            mainForm.LogToScreeen("Order " + order_id_cancel + " canceled.");

            working = false;
        }


        // ------------------------------------------------------------

        bool isDateTimeValid(DateTime dt)
        {
            bool ret = false;

            // переданное время должно быть в диапазоне от -7 минут назад до текущего времени
            if (dt.Ticks >= DateTime.Now.Ticks - lostConnectionInterval.Ticks)
                ret = true; // валидно

            return ret;
        }
    }
}
