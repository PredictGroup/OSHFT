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
    public class PandasExchange : BaseExchange
    {
        // **********************************************************************
        static OSHFT_Q_RMain mainForm;

        RedisManagerPool redisManager = new RedisManagerPool(cfg.u.RedisUser + ":" + cfg.u.RedisPassword + "@" + cfg.u.RedisServerIP + ":" + cfg.u.RedisServerPort);

        DateTime firstTickDt = DateTime.Today.AddHours(8); // UTC time zone
        DateTime lastTickDt = DateTime.Today.AddHours(20).AddMinutes(49).AddSeconds(59);

        TimeSpan lostConnectionInterval = new TimeSpan(0, 0, 10, 0, 000);

        static bool working;

        double lastPrice = 0;

        DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0);
        static int lastSpreadID = 0;
        static long lastSpreadTimeSpan;
        static int lastQuoteID = 0;
        static long lastQuoteTimeSpan;
        static int lastTickID = 0;
        static long lastTickTimeSpan;
        static int lastSettingID = 0;
        static long lastSettingTimeSpan;

        // **********************************************************************

        // minutes - Id в таблице тайм фреймов
        public PandasExchange(string Name = "PandasExchange")
            : base(Name)
        {
            working = false;
        }

        public static void SetMainForm(OSHFT_Q_RMain _mainForm)
        {
            mainForm = _mainForm;
            mainForm.LogToScreeen("Pandas exchange client connected.");
        }

        // **********************************************************************

        public override void ProcessSpread(Spread spread)
        {
            DateTime now = DateTime.UtcNow;

            long timeSpan = (long)now.Subtract(dt1970).TotalMilliseconds;
            if (lastSpreadTimeSpan == timeSpan)
                lastSpreadID++;
            else
            {
                lastSpreadTimeSpan = timeSpan;
                lastSpreadID = 0;
            }

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
                var ret = redisClient.Custom("XADD", "spreads_pandas", timeSpan.ToString() + "-" + lastSpreadID.ToString(), "spread", packData);
            }
        }

        // **********************************************************************

        public override void ProcessQuotes(Quote[] quotes)
        {
            DateTime now = DateTime.UtcNow;

            long timeSpan = (long)now.Subtract(dt1970).TotalMilliseconds;
            if (lastQuoteTimeSpan == timeSpan)
                lastQuoteID++;
            else
            {
                lastQuoteTimeSpan = timeSpan;
                lastQuoteID = 0;
            }

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

            // Redis
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "quotes_pandas", timeSpan.ToString() + "-" + lastQuoteID.ToString(), "quote", packData);
            }
        }

        // **********************************************************************

        public override void ProcessTick(Tick tick)
        {
            if (tick.DateTime < firstTickDt)
                return;

            DateTime now = DateTime.UtcNow;
            long timeSpan = (long)now.Subtract(dt1970).TotalMilliseconds;
            if (lastTickTimeSpan == timeSpan)
                lastTickID++;
            else
            {
                lastTickTimeSpan = timeSpan;
                lastTickID = 0;
            }

            lastPrice = tick.RawPrice;

            SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
            msgpack.ForcePathObject("Symbol").AsString = tick.SecCode;
            msgpack.ForcePathObject("TradeNum").AsString = tick.TradeNum;
            msgpack.ForcePathObject("Price").AsFloat = tick.RawPrice;
            msgpack.ForcePathObject("Volume").AsFloat = tick.Volume;
            msgpack.ForcePathObject("Operation").AsString = tick.Op.ToString();
            msgpack.ForcePathObject("DateTime").AsString = tick.DateTime.ToString("o");
            msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "ticks_pandas", timeSpan.ToString() + "-" + lastTickID.ToString(), "tick", packData);
            }
        }

        // **********************************************************************

        public override void ProcessSetting(Setting setting)
        {
            if (setting.DateTime < firstTickDt)
                return;

            DateTime now = DateTime.UtcNow;
            long timeSpan = (long)now.Subtract(dt1970).TotalMilliseconds;
            if (lastSettingTimeSpan == timeSpan)
                lastSettingID++;
            else
            {
                lastSettingTimeSpan = timeSpan;
                lastSettingID = 0;
            }

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
            msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
            msgpack.ForcePathObject("Version").AsString = "1.0";

            byte[] packData = msgpack.Encode2Bytes();

            // Redis
            using (var redisClient = redisManager.GetClient())
            {
                var ret = redisClient.Custom("XADD", "settings_pandas", timeSpan.ToString() + "-" + lastSettingID.ToString(), "setting", packData);
            }
        }

        // **********************************************************************

        public override void ProcessTrade(Trade trade)
        {
        }

        // **********************************************************************

        public override void ProcessPutOrder(PutOrder putOrder)
        {
        }

        // **********************************************************************

    }
}
