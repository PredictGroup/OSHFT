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
    class ShortTraderExchange : BaseExchange
    {

        // **********************************************************************
        static OSHFT_Q_RMain mainForm;

        RedisManagerPool redisManager = new RedisManagerPool(cfg.u.RedisUser + ":" + cfg.u.RedisPassword + "@" + cfg.u.RedisServerIP + ":" + cfg.u.RedisServerPort);

        public ShortTraderExchange(string Name = "ShortTraderExchange")
            : base(Name)
        {
        }

        public static void SetMainForm(OSHFT_Q_RMain _mainForm)
        {
            mainForm = _mainForm;
            mainForm.LogToScreeen("Short Trader client connected.");
        }

        // **********************************************************************

        public override void ProcessSpread(Spread spread)
        {
            DateTime now = DateTime.Now;

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
                var ret = redisClient.Custom("XADD", "spreads_shorttrader", "*", "spread", packData);
            }
        }

        // **********************************************************************

        public override void ProcessQuotes(Quote[] quotes)
        {
        }

        // **********************************************************************

        public override void ProcessTick(Tick tick)
        {
        }

        // **********************************************************************

        public override void ProcessSetting(Setting setting)
        {
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
