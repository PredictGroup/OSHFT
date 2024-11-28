using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace OSHFT_Q_R
{
    static class ExchangeManager
    {
        public static ObservableCollection<BaseExchange> baseExchanges = new ObservableCollection<BaseExchange>();

        // **************************************************************************

        static RedisExchange toRedis;
        //static PandasExchange toPandas;
        static ShortTraderExchange toShortTrader;

        // **************************************************************************

        public static DataManager dm;
        public static TermManager tm;

        // **************************************************************************

        static MinStrategy _1minStrg;
        static MinStrategy _3minStrg;
        static MinStrategy _5minStrg;
        static MinStrategy _10minStrg;
        static MinStrategy _15minStrg;
        static MinStrategy _20minStrg;
        static MinStrategy _30minStrg;
        static MinStrategy _60minStrg;

        // **************************************************************************

        static ExchangeManager()
        {
            // **********************************************************************

            toRedis = new RedisExchange("RedisExchange");
            //toPandas = new PandasExchange("PandasExchange");
            toShortTrader = new ShortTraderExchange();

            baseExchanges.Add(toRedis);
            //baseExchanges.Add(toPandas);
            baseExchanges.Add(toShortTrader);

            _1minStrg = new MinStrategy(cfg.u._1minTimeFrameSize, 1, "_1minStrategy");
            //_2minStrg = new MinStrategy(cfg.u._2minTimeFrameSize, 2, "_2minStrategy");
            _3minStrg = new MinStrategy(cfg.u._3minTimeFrameSize, 3, "_3minStrategy");
            //_4minStrg = new MinStrategy(cfg.u._4minTimeFrameSize, 4, "_4minStrategy");
            _5minStrg = new MinStrategy(cfg.u._5minTimeFrameSize, 5, "_5minStrategy");
            _10minStrg = new MinStrategy(cfg.u._10minTimeFrameSize, 6, "_10minStrategy");
            _15minStrg = new MinStrategy(cfg.u._15minTimeFrameSize, 7, "_15minStrategy");
            _20minStrg = new MinStrategy(cfg.u._20minTimeFrameSize, 8, "_20minStrategy");
            _30minStrg = new MinStrategy(cfg.u._30minTimeFrameSize, 9, "_30minStrategy");
            _60minStrg = new MinStrategy(cfg.u._60minTimeFrameSize, 10, "_60minStrategy");

            baseExchanges.Add(_1minStrg);
            //baseExchanges.Add(_2minStrg);
            baseExchanges.Add(_3minStrg);
            //baseExchanges.Add(_4minStrg);
            baseExchanges.Add(_5minStrg);
            baseExchanges.Add(_10minStrg);
            baseExchanges.Add(_15minStrg);
            baseExchanges.Add(_20minStrg);
            baseExchanges.Add(_30minStrg);
            baseExchanges.Add(_60minStrg);
        }

        public static void SetDataManager(DataManager _dm)
        {
            dm = _dm;
        }

        public static void SetTermManager(TermManager _tm)
        {
            tm = _tm;
        }

        public static void SetMainForm(OSHFT_Q_RMain _mainForm)
        {
            RedisExchange.SetMainForm(_mainForm);
            //PandasExchange.SetMainForm(_mainForm);
            MinStrategy.SetMainForm(_mainForm);
            ShortTraderExchange.SetMainForm(_mainForm);
        }

        public static void Activate()
        {
            int i = 0;
            foreach (BaseExchange strategy in baseExchanges)
            {
                Activate(i);
                i++;
            }
        }

        public static void Deactivate()
        {
            int i = 0;
            foreach (BaseExchange strategy in baseExchanges)
            {
                Deactivate(i);
                i++;
            }
        }

        public static void Activate(int idx)
        {
            if (!baseExchanges[idx].Active)
            {
                baseExchanges[idx].Start();
                dm.QuotesQueue.RegisterHandler(baseExchanges[idx].ProcessQuotes);
                dm.SpreadsQueue.RegisterHandler(baseExchanges[idx].ProcessSpread);
                dm.TicksQueue.RegisterHandler(baseExchanges[idx].ProcessTick);
                dm.SettingsQueue.RegisterHandler(baseExchanges[idx].ProcessSetting);
                dm.PutOrdersQueue.RegisterHandler(baseExchanges[idx].ProcessPutOrder);
                dm.TradesQueue.RegisterHandler(baseExchanges[idx].ProcessTrade);
            }
        }

        public static void Deactivate(int idx)
        {
            if (baseExchanges[idx].Active)
            {
                baseExchanges[idx].Stop();
                dm.QuotesQueue.UnregisterHandler(baseExchanges[idx].ProcessQuotes);
                dm.SpreadsQueue.UnregisterHandler(baseExchanges[idx].ProcessSpread);
                dm.TicksQueue.UnregisterHandler(baseExchanges[idx].ProcessTick);
                dm.SettingsQueue.UnregisterHandler(baseExchanges[idx].ProcessSetting);
                dm.PutOrdersQueue.UnregisterHandler(baseExchanges[idx].ProcessPutOrder);
                dm.TradesQueue.UnregisterHandler(baseExchanges[idx].ProcessTrade);
            }
        }
    }
}
