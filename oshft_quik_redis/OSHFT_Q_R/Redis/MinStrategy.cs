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
    public class MinStrategy : BaseExchange // minute strategy
    {
        // **********************************************************************
        static OSHFT_Q_RMain mainForm;

        int currentTimeFrame = cfg.u._3minTimeFrameSize;
        int currentTimeFrameId = 3;

        // **********************************************************************

        protected Dictionary<string, Dictionary<DateTime, StockDataElement>> volumeElements; // данные в потоке ticks
        protected Dictionary<string, Dictionary<DateTime, SettingDataElement>> settingElements;

        // **********************************************************************

        protected Dictionary<string, DateTime> lastDateTimeVolume;
        protected Dictionary<string, DateTime> lastDateTimeSetting;

        protected Dictionary<string, Dictionary<DateTime, StockDataElement>> VolumeVector { get { return volumeElements; } }
        protected Dictionary<string, Dictionary<DateTime, SettingDataElement>> SettingVector { get { return settingElements; } }

        // **********************************************************************

        bool firstRound = true;

        bool firstLoopInCandle = true;

        bool lastCandleSend = false;

        // **********************************************************************

        DateTime lastTickDt;
        DateTime firstTickDt;

        // **********************************************************************
        protected DateTime lastProcessedDateTime = DateTime.MinValue;
        // **********************************************************************
        double LastVolumeMid = 0;
        double LastVolumeAvg = 0;

        double LastDeltaOCMid = 0;
        double LastDeltaOCAvg = 0;

        double LastDeltaOC_2Mid = 0;
        double LastDeltaOC_2Avg = 0;

        double initClusterPrice = 0;
        TradeOp initClusterOp;

        double countClusterQtyBuy = 0;
        double countClusterQtySell = 0;


        double ClusterAvg_A_last;
        double ClusterAvg_B_last;
        double ClusterAvg_C_last;
        double ClusterAvg_D_last;

        double ClusterQtyAvg_last;
        double ClusterVolAvg_last;

        int lastClusterVol = 0;

        // **********************************************************************
        //+ GAS 13042015
        double initCountPrice = 0;
        //- GAS 13042015
        // **********************************************************************

        // ********************************************************************** REDIS
        RedisManagerPool redisManager = new RedisManagerPool(cfg.u.RedisUser + ":" + cfg.u.RedisPassword + "@" + cfg.u.RedisServerIP + ":" + cfg.u.RedisServerPort);

        // **********************************************************************

        // minutes - Id в таблице тайм фреймов
        public MinStrategy(int TimeFrameMinutes, int TimeFrameId, string StrategyName = "_3minStrategy")
            : base(StrategyName)
        {
            volumeElements = new Dictionary<string, Dictionary<DateTime, StockDataElement>>();
            settingElements = new Dictionary<string, Dictionary<DateTime, SettingDataElement>>();

            lastDateTimeVolume = new Dictionary<string, DateTime>();
            lastDateTimeSetting = new Dictionary<string, DateTime>();


            currentTimeFrame = TimeFrameMinutes;
            currentTimeFrameId = TimeFrameId;

            firstTickDt = DateTime.Today.AddHours(7).AddMinutes(49).AddSeconds(59);
            lastTickDt = DateTime.Today.AddHours(23).AddMinutes(49).AddSeconds(59);

            // **********************************************************************

            // cfg.u.SecCode

            // **********************************************************************

            ClusterAvg_A_last = 0;
            ClusterAvg_B_last = 0;
            ClusterAvg_C_last = 0;
            ClusterAvg_D_last = 0;

            ClusterQtyAvg_last = 0;
            ClusterVolAvg_last = 0;

        }

        public static void SetMainForm(OSHFT_Q_RMain _mainForm)
        {
            mainForm = _mainForm;
            mainForm.LogToScreeen("MinStrategy exchange client connected.");
        }

        // **********************************************************************

        // **********************************************************************

        private async void createNewVolume(string SecCode, DateTime lastDT)
        {
            bool found = false;

            DateTime lastCandleTime = DateTime.MinValue;
            if (lastDateTimeVolume.TryGetValue(SecCode, out lastCandleTime) == false)
            {
                // начальная установка даты и времени отсчета
                lastCandleTime = lastDT;
                lastDT = firstTickDt; // ставим на начало сессии

                //lastDateTimeVolume.Remove(SecCode);
                //lastDateTimeVolume.Add(SecCode, lastDT);
                lastDateTimeVolume[SecCode] = lastDT; // сохраняем
            }

            StockDataElement lastEl1 = new StockDataElement(SecCode, lastDT);

            lock (VolumeVector)
            {
                Dictionary<DateTime, StockDataElement> volumes = new Dictionary<DateTime, StockDataElement>();
                lock (volumes)
                {
                    if (VolumeVector.TryGetValue(SecCode, out volumes) == false)
                    {
                        volumes = new Dictionary<DateTime, StockDataElement>();
                        StockDataElement vds = new StockDataElement(SecCode, lastDT);
                        //volumes.Add(lastDT, vds);
                        //VolumeVector.Add(SecCode, volumes);
                        volumes[lastDT] = vds;
                        VolumeVector[SecCode] = volumes;
                    }
                    else
                    {
                        if (volumes.TryGetValue(lastCandleTime, out lastEl1) == true)
                            found = true;

                        StockDataElement vds = new StockDataElement(SecCode, lastDT);

                        //ErrLog ne = new ErrLog();
                        //ne.ErrText = "SecCode " + SecCode + "; lastCandleTime " + lastCandleTime.ToString() + "; lastDT " + lastDT.ToString();
                        //ne.TimeStamp = DateTime.Now;
                        //cont1.ErrLog.InsertOnSubmit(ne);
                        //cont1.SubmitChanges();

                        //volumes.Add(lastDT, vds);
                        volumes[lastDT] = vds;

                        //VolumeVector.Remove(SecCode);
                        //VolumeVector.Add(SecCode, volumes);
                        VolumeVector[SecCode] = volumes;
                    }
                }
            }

            //lastDateTimeVolume.Remove(SecCode);
            //lastDateTimeVolume.Add(SecCode, lastDT);
            lastDateTimeVolume[SecCode] = lastDT;


            // запуск в отдельном процессе сохранения последнего элемента в базу
            if (found)
            {
                lastEl1.middle = lastEl1.PriceVolumeSum / lastEl1.Volume;
                lastEl1.date = lastCandleTime; // будет одной временной точкой
                if (lastEl1.close != 0)
                {
                    await saveStockElementToDB(lastEl1);
                }
            }
        }

        private async void createNewSetting(string SecCode, DateTime lastDT)
        {
            bool found = false;

            DateTime lastCandleTime = DateTime.MinValue;
            if (lastDateTimeSetting.TryGetValue(SecCode, out lastCandleTime) == false)
            {
                // начальная установка даты и времени отсчета
                lastCandleTime = lastDT;
                lastDT = firstTickDt; // ставим на начало сессии

                //lastDateTimeSetting.Remove(SecCode);
                //lastDateTimeSetting.Add(SecCode, lastDT);

                lastDateTimeSetting[SecCode] = lastDT; // сохраняем!
            }

            SettingDataElement lastEl2 = new SettingDataElement(SecCode, lastDT);

            lock (SettingVector)
            {
                Dictionary<DateTime, SettingDataElement> settings = new Dictionary<DateTime, SettingDataElement>();
                lock (settings)
                {
                    if (SettingVector.TryGetValue(SecCode, out settings) == false) // ищем по текущему Коду SecCode есть ли уже значение settings оно одно для каждого элемента
                    {
                        settings = new Dictionary<DateTime, SettingDataElement>();
                        SettingDataElement sde = new SettingDataElement(SecCode, lastDT);
                        settings[lastDT] = sde;
                        SettingVector[SecCode] = settings;
                        //settings.Add(lastDT, sde);
                        //SettingVector.Add(SecCode, settings);
                    }
                    else
                    {
                        if (settings.TryGetValue(lastCandleTime, out lastEl2) == true)
                            found = true;

                        SettingDataElement sde = new SettingDataElement(SecCode, lastDT);
                        settings[lastDT] = sde;
                        SettingVector[SecCode] = settings;
                        //settings.Add(lastDT, sde);
                        //SettingVector.Remove(SecCode);
                        //SettingVector.Add(SecCode, settings);
                    }
                }
            }

            //lastDateTimeSetting.Remove(SecCode);
            //lastDateTimeSetting.Add(SecCode, lastDT);

            lastDateTimeSetting[SecCode] = lastDT; // сохраняем!

            if (found)
            {
                lastEl2.DateTime = lastCandleTime; // будет одной временной точкой
                await saveSettingElementToDB(lastEl2);
            }

        }

        // **********************************************************************

        public override void ProcessTick(Tick tick)
        {
            if (tick.DateTime < firstTickDt)
                return;

            lock (VolumeVector)
            {
                DateTime lastDT = DateTime.MinValue;
                if (lastDateTimeVolume.TryGetValue(tick.SecCode, out lastDT) == false)
                {
                    // начальная установка даты и времени отсчета
                    lastDT = tick.DateTime;
                }

                Dictionary<DateTime, StockDataElement> volumes = new Dictionary<DateTime, StockDataElement>();
                if (VolumeVector.TryGetValue(tick.SecCode, out volumes) == false)
                {
                    createNewVolume(tick.SecCode, lastDT);

                    VolumeVector.TryGetValue(tick.SecCode, out volumes);
                }

                StockDataElement vds = new StockDataElement(tick.SecCode, lastDT);
                volumes.TryGetValue(lastDT, out vds);


                // если время больше XXX мин, делаем новую группу объема
                long csTicks = currentTimeFrame * TimeSpan.TicksPerSecond;
                if (tick.DateTime.Ticks - lastDT.Ticks >= csTicks)
                { // новая группа объема

                    //--------------------

                    LastVolumeAvg = vds.VolumeAvg;
                    LastVolumeMid = vds.VolumeMidClose;

                    LastDeltaOCAvg = vds.DeltaOCAvg;
                    LastDeltaOCMid = vds.DeltaOCMidClose;

                    LastDeltaOC_2Avg = vds.DeltaOC_2Avg;
                    LastDeltaOC_2Mid = vds.DeltaOC_2Mid;

                    //// обнуляем текущий кластер
                    //initClusterPrice = tick.IntPrice;
                    //initClusterOp = tick.Op;


                    //+ 3-6-9-21 
                    var elementss3 = (from d in volumes
                                      orderby d.Key descending
                                      select d.Value.VolumeDiffClose).Take(3).Average();

                    var elementss6 = (from d in volumes
                                      orderby d.Key descending
                                      select d.Value.VolumeDiffClose).Take(6).Average();

                    var elementss9 = (from d in volumes
                                      orderby d.Key descending
                                      select d.Value.VolumeDiffClose).Take(9).Average();

                    var elementss21 = (from d in volumes
                                       orderby d.Key descending
                                       select d.Value.VolumeDiffClose).Take(21).Average();
                    //---
                    var elementssM3 = (from d in volumes
                                       orderby d.Key descending
                                       select d.Value.VolumeMidClose).Take(3).Average();

                    var elementssM6 = (from d in volumes
                                       orderby d.Key descending
                                       select d.Value.VolumeMidClose).Take(6).Average();

                    var elementssM9 = (from d in volumes
                                       orderby d.Key descending
                                       select d.Value.VolumeMidClose).Take(9).Average();

                    var elementssM21 = (from d in volumes
                                        orderby d.Key descending
                                        select d.Value.VolumeMidClose).Take(21).Average();
                    //---
                    var elementssOC3 = (from d in volumes
                                        orderby d.Key descending
                                        select d.Value.DeltaOC_Close).Take(3).Average();

                    var elementssOC6 = (from d in volumes
                                        orderby d.Key descending
                                        select d.Value.DeltaOC_Close).Take(6).Average();

                    var elementssOC9 = (from d in volumes
                                        orderby d.Key descending
                                        select d.Value.DeltaOC_Close).Take(9).Average();

                    var elementssOC21 = (from d in volumes
                                         orderby d.Key descending
                                         select d.Value.DeltaOC_Close).Take(21).Average();
                    //---
                    var elementssOCM3 = (from d in volumes
                                         orderby d.Key descending
                                         select d.Value.DeltaOCMidClose).Take(3).Average();

                    var elementssOCM6 = (from d in volumes
                                         orderby d.Key descending
                                         select d.Value.DeltaOCMidClose).Take(6).Average();

                    var elementssOCM9 = (from d in volumes
                                         orderby d.Key descending
                                         select d.Value.DeltaOCMidClose).Take(9).Average();

                    var elementssOCM21 = (from d in volumes
                                          orderby d.Key descending
                                          select d.Value.DeltaOCMidClose).Take(21).Average();
                    //- 3-6-9-21


                    //+ old
                    lastDT = tick.DateTime;
                    createNewVolume(tick.SecCode, lastDT);
                    volumes.TryGetValue(lastDT, out vds);
                    //- old

                    vds.VolumeDiffClose3 = elementss3;
                    vds.VolumeDiffClose6 = elementss6;
                    vds.VolumeDiffClose9 = elementss9;
                    vds.VolumeDiffClose21 = elementss21;
                    //---
                    vds.VolumeMidClose3 = elementssM3;
                    vds.VolumeMidClose6 = elementssM6;
                    vds.VolumeMidClose9 = elementssM9;
                    vds.VolumeMidClose21 = elementssM21;
                    //---
                    vds.DeltaOC_Close3 = elementssOC3;
                    vds.DeltaOC_Close6 = elementssOC6;
                    vds.DeltaOC_Close9 = elementssOC9;
                    vds.DeltaOC_Close21 = elementssOC21;
                    //---
                    vds.DeltaOCMidClose3 = elementssOCM3;
                    vds.DeltaOCMidClose6 = elementssOCM6;
                    vds.DeltaOCMidClose9 = elementssOCM9;
                    vds.DeltaOCMidClose21 = elementssOCM21;

                    //---

                    vds.high = tick.IntPrice;
                    vds.low = tick.IntPrice;

                    vds.open = tick.IntPrice;
                    vds.close = tick.IntPrice;

                    firstLoopInCandle = true;
                }

                if (firstRound)
                {
                    vds.high = tick.IntPrice;
                    vds.low = tick.IntPrice;

                    vds.open = tick.IntPrice;
                    vds.close = tick.IntPrice;

                    initClusterPrice = tick.IntPrice;
                    initClusterOp = tick.Op;
                }
                //+ 02082015 Bug fix
                if (vds.open == 0)
                    vds.open = tick.IntPrice;
                //+ 02082015 Bug fix

                if (tick.IntPrice > vds.high)
                    vds.high = tick.IntPrice;

                if (tick.IntPrice < vds.low)
                    vds.low = tick.IntPrice;

                vds.close = tick.IntPrice;

                vds.Volume += tick.Volume;
                vds.PriceVolumeSum += (tick.IntPrice * tick.Volume);

                if (tick.Op == TradeOp.Sell)
                {
                    vds.VolumeSell += tick.Volume;
                    vds.VolumeSellQty++;
                }
                else
                {
                    vds.VolumeBuy += tick.Volume;
                    vds.VolumeBuyQty++;
                }

                //+ 26072015 добавляем функциона 2.0
                vds.VolumeDiffClose = (vds.VolumeBuy - vds.VolumeSell);

                if (firstLoopInCandle)
                {
                    vds.VolumeDiffHigh = vds.VolumeDiffClose;
                    vds.VolumeDiffLow = vds.VolumeDiffClose;
                }
                else
                {
                    if (vds.VolumeDiffClose > vds.VolumeDiffHigh)
                        vds.VolumeDiffHigh = vds.VolumeDiffClose;
                    if (vds.VolumeDiffClose < vds.VolumeDiffLow)
                        vds.VolumeDiffLow = vds.VolumeDiffClose;
                }

                vds.VolumeAvg = LastVolumeAvg + vds.VolumeDiffClose;

                if (firstRound)
                    vds.VolumeMidClose = vds.VolumeDiffClose;
                else
                    vds.VolumeMidClose = (LastVolumeMid + vds.VolumeDiffClose) / 2;


                if (firstLoopInCandle)
                {
                    vds.VolumeMidOpen = vds.VolumeMidClose;
                    vds.VolumeMidHigh = vds.VolumeMidClose;
                    vds.VolumeMidLow = vds.VolumeMidClose;
                }
                else
                {
                    if (vds.VolumeMidClose > vds.VolumeMidHigh)
                        vds.VolumeMidHigh = vds.VolumeMidClose;
                    if (vds.VolumeMidClose < vds.VolumeMidLow)
                        vds.VolumeMidLow = vds.VolumeMidClose;
                }

                // Delta - величина движения и объема
                if (vds.open == vds.close)
                    vds.DeltaOC_Close = 0;
                else
                    vds.DeltaOC_Close = (vds.close - vds.open) / cfg.u.PriceStep * vds.Volume / 1000;

                if (firstLoopInCandle)
                {
                    vds.DeltaOC_High = vds.DeltaOC_Close;
                    vds.DeltaOC_Low = vds.DeltaOC_Close;
                }
                else
                {
                    if (vds.DeltaOC_Close > vds.DeltaOC_High)
                        vds.DeltaOC_High = vds.DeltaOC_Close;
                    if (vds.DeltaOC_Close < vds.DeltaOC_Low)
                        vds.DeltaOC_Low = vds.DeltaOC_Close;
                }

                if (vds.high == vds.low)
                    vds.DeltaHL = 0;
                else
                    vds.DeltaHL = (vds.high - vds.low) / cfg.u.PriceStep * vds.Volume / 1000;

                // 2
                if (vds.open == vds.close)
                    vds.DeltaOC_2 = 0;
                else
                    vds.DeltaOC_2 = (vds.close - vds.open) / cfg.u.PriceStep / vds.Volume * 1000;

                if (vds.high == vds.low)
                    vds.DeltaHL_2 = 0;
                else
                    vds.DeltaHL_2 = (vds.high - vds.low) / cfg.u.PriceStep / vds.Volume * 1000;



                // Delta OH OL (Open High - Open Low)
                if (vds.open == vds.high)
                    vds.DeltaOH = 0;
                else
                    vds.DeltaOH = (vds.high - vds.open) / cfg.u.PriceStep * vds.Volume / 100;
                //--
                if (vds.open == vds.low)
                    vds.DeltaOL = 0;
                else
                    vds.DeltaOL = (vds.open - vds.low) / cfg.u.PriceStep * vds.Volume / 100;

                // Delta CH CL (Close High - Close Low)
                if (vds.close == vds.high)
                    vds.DeltaCH = 0;
                else
                    vds.DeltaCH = (vds.high - vds.close) / cfg.u.PriceStep * vds.Volume / 100;
                //--
                if (vds.close == vds.low)
                    vds.DeltaCL = 0;
                else
                    vds.DeltaCL = (vds.close - vds.low) / cfg.u.PriceStep * vds.Volume / 100;

                vds.DeltaOHOLDiff = vds.DeltaOH - vds.DeltaOL;
                vds.DeltaCHCLDiff = vds.DeltaCL - vds.DeltaCH;

                // 222222
                // Delta OH OL (Open High - Open Low)
                if (vds.open == vds.high)
                    vds.DeltaOH_2 = 0;
                else
                    vds.DeltaOH_2 = (vds.high - vds.open) / cfg.u.PriceStep / vds.Volume * 1000;
                //--
                if (vds.open == vds.low)
                    vds.DeltaOL_2 = 0;
                else
                    vds.DeltaOL_2 = (vds.open - vds.low) / cfg.u.PriceStep / vds.Volume * 1000;

                // Delta CH CL (Close High - Close Low)
                if (vds.close == vds.high)
                    vds.DeltaCH_2 = 0;
                else
                    vds.DeltaCH_2 = (vds.high - vds.close) / cfg.u.PriceStep / vds.Volume * 1000;
                //--
                if (vds.close == vds.low)
                    vds.DeltaCL_2 = 0;
                else
                    vds.DeltaCL_2 = (vds.close - vds.low) / cfg.u.PriceStep / vds.Volume * 1000;

                vds.DeltaOHOLDiff_2 = vds.DeltaOH_2 - vds.DeltaOL_2;
                vds.DeltaCHCLDiff_2 = vds.DeltaCL_2 - vds.DeltaCH_2;


                // BUY VS SELL Delta OH OL CH CL

                // Delta OH OL (Open High - Open Low)
                if (vds.open == vds.high || vds.VolumeBuy == 0)
                    vds.DeltaOH_Buy = 0;
                else
                    vds.DeltaOH_Buy = (vds.high - vds.open) / cfg.u.PriceStep * vds.VolumeBuy / 100;
                //--
                if (vds.open == vds.low || vds.VolumeSell == 0)
                    vds.DeltaOL_Sell = 0;
                else
                    vds.DeltaOL_Sell = (vds.open - vds.low) / cfg.u.PriceStep * vds.VolumeSell / 100;

                // Delta CH CL (Close High - Close Low)
                if (vds.close == vds.high || vds.VolumeSell == 0)
                    vds.DeltaCH_Sell = 0;
                else
                    vds.DeltaCH_Sell = (vds.high - vds.close) / cfg.u.PriceStep * vds.VolumeSell / 100;
                //--
                if (vds.close == vds.low || vds.VolumeBuy == 0)
                    vds.DeltaCL_Buy = 0;
                else
                    vds.DeltaCL_Buy = (vds.close - vds.low) / cfg.u.PriceStep * vds.VolumeBuy / 100;

                vds.DeltaOHOLDiff_Buy_Sell = vds.DeltaOH_Buy - vds.DeltaOL_Sell;
                vds.DeltaCHCLDiff_Buy_Sell = vds.DeltaCL_Buy - vds.DeltaCH_Sell;

                // 2 2 2 2 2 2
                // Delta OH OL (Open High - Open Low)
                if (vds.open == vds.high || vds.VolumeBuy == 0)
                    vds.DeltaOH_Buy_2 = 0;
                else
                    vds.DeltaOH_Buy_2 = (vds.high - vds.open) / cfg.u.PriceStep / vds.VolumeBuy * 1000;
                //--
                if (vds.open == vds.low || vds.VolumeSell == 0)
                    vds.DeltaOL_Sell_2 = 0;
                else
                    vds.DeltaOL_Sell_2 = (vds.open - vds.low) / cfg.u.PriceStep / vds.VolumeSell * 1000;

                // Delta CH CL (Close High - Close Low)
                if (vds.close == vds.high || vds.VolumeSell == 0)
                    vds.DeltaCH_Sell_2 = 0;
                else
                    vds.DeltaCH_Sell_2 = (vds.high - vds.close) / cfg.u.PriceStep / vds.VolumeSell * 1000;
                //--
                if (vds.close == vds.low || vds.VolumeBuy == 0)
                    vds.DeltaCL_Buy_2 = 0;
                else
                    vds.DeltaCL_Buy_2 = (vds.close - vds.low) / cfg.u.PriceStep / vds.VolumeBuy * 1000;

                vds.DeltaOHOLDiff_Buy_Sell_2 = vds.DeltaOH_Buy_2 - vds.DeltaOL_Sell_2;
                vds.DeltaCHCLDiff_Buy_Sell_2 = vds.DeltaCL_Buy_2 - vds.DeltaCH_Sell_2;


                // - BUY VS SELL Delta OH OL CH CL

                // Delta OC AVG & MID
                vds.DeltaOCAvg = LastDeltaOCAvg + vds.DeltaOC_Close;
                if (firstRound)
                    vds.DeltaOCMidClose = vds.DeltaOC_Close;
                else
                    vds.DeltaOCMidClose = (LastDeltaOCMid + vds.DeltaOC_Close) / 2;


                vds.DeltaOC_2Avg = LastDeltaOC_2Avg + vds.DeltaOC_2;
                if (firstRound)
                    vds.DeltaOC_2Mid = vds.DeltaOC_2;
                else
                    vds.DeltaOC_2Mid = (LastDeltaOC_2Mid + vds.DeltaOC_2) / 2;

                if (firstLoopInCandle)
                {
                    vds.DeltaOCMidOpen = vds.DeltaOCMidClose;
                    vds.DeltaOCMidHigh = vds.DeltaOCMidClose;
                    vds.DeltaOCMidLow = vds.DeltaOCMidClose;
                }
                else
                {
                    if (vds.DeltaOCMidClose > vds.DeltaOCMidHigh)
                        vds.DeltaOCMidHigh = vds.DeltaOCMidClose;
                    if (vds.DeltaOCMidClose < vds.DeltaOCMidLow)
                        vds.DeltaOCMidLow = vds.DeltaOCMidClose;
                }

                //+ Clusters
                if (tick.Op != initClusterOp)
                { // если операция сменилась, т.е. предыдущий кластер сменяется на противоположный
                    //+ A-B-C Cluster
                    if (initClusterOp == TradeOp.Buy)
                    {
                        if (lastClusterVol <= VolumeCfg.volCluster_D)
                        {
                            vds.ClusterQtyBuy_D++;
                            vds.ClusterVolBuy_D += lastClusterVol;
                        }
                        else
                            if (lastClusterVol <= VolumeCfg.volCluster_C)
                        {
                            vds.ClusterQtyBuy_C++;
                            vds.ClusterVolBuy_C += lastClusterVol;
                        }
                        else
                                if (lastClusterVol <= VolumeCfg.volCluster_B)
                        {
                            vds.ClusterQtyBuy_B++;
                            vds.ClusterVolBuy_B += lastClusterVol;
                        }
                        else
                        {
                            vds.ClusterQtyBuy_A++;
                            vds.ClusterVolBuy_A += lastClusterVol;
                        }
                    }
                    else
                        if (initClusterOp == TradeOp.Sell)
                    {
                        if (lastClusterVol <= VolumeCfg.volCluster_D)
                        {
                            vds.ClusterQtySell_D++;
                            vds.ClusterVolSell_D += lastClusterVol;
                        }
                        else
                            if (lastClusterVol <= VolumeCfg.volCluster_C)
                        {
                            vds.ClusterQtySell_C++;
                            vds.ClusterVolSell_C += lastClusterVol;
                        }
                        else
                                if (lastClusterVol <= VolumeCfg.volCluster_B)
                        {
                            vds.ClusterQtySell_B++;
                            vds.ClusterVolSell_B += lastClusterVol;
                        }
                        else
                        {
                            vds.ClusterQtySell_A++;
                            vds.ClusterVolSell_A += lastClusterVol;
                        }
                    }
                    lastClusterVol = 0; // обнуляем

                    initClusterPrice = tick.IntPrice;
                    initClusterOp = tick.Op;

                    // MID
                    if (firstRound)
                    {
                        vds.ClusterMid_A = vds.ClusterVolBuy_A - vds.ClusterVolSell_A;
                        vds.ClusterMid_B = vds.ClusterVolBuy_B - vds.ClusterVolSell_B;
                        vds.ClusterMid_C = vds.ClusterVolBuy_C - vds.ClusterVolSell_C;
                        vds.ClusterMid_D = vds.ClusterVolBuy_D - vds.ClusterVolSell_D;
                    }
                    else
                    {
                        vds.ClusterMid_A = (vds.ClusterVolBuy_A - vds.ClusterVolSell_A) / 2;
                        vds.ClusterMid_B = (vds.ClusterVolBuy_B - vds.ClusterVolSell_B) / 2;
                        vds.ClusterMid_C = (vds.ClusterVolBuy_C - vds.ClusterVolSell_C) / 2;
                        vds.ClusterMid_D = (vds.ClusterVolBuy_D - vds.ClusterVolSell_D) / 2;
                    }

                    // AVG
                    vds.ClusterAvg_A = ClusterAvg_A_last + vds.ClusterVolBuy_A - vds.ClusterVolSell_A;
                    ClusterAvg_A_last = vds.ClusterVolBuy_A - vds.ClusterVolSell_A;

                    vds.ClusterAvg_B = ClusterAvg_B_last + vds.ClusterVolBuy_B - vds.ClusterVolSell_B;
                    ClusterAvg_B_last = vds.ClusterVolBuy_B - vds.ClusterVolSell_B;

                    vds.ClusterAvg_C = ClusterAvg_C_last + vds.ClusterVolBuy_C - vds.ClusterVolSell_C;
                    ClusterAvg_C_last = vds.ClusterVolBuy_C - vds.ClusterVolSell_C;

                    vds.ClusterAvg_D = ClusterAvg_D_last + vds.ClusterVolBuy_D - vds.ClusterVolSell_D;
                    ClusterAvg_D_last = vds.ClusterVolBuy_D - vds.ClusterVolSell_D;

                    // QTY AVG
                    vds.ClusterQtyAvg = ClusterQtyAvg_last + (vds.ClusterQtyBuy_A + vds.ClusterQtyBuy_B + vds.ClusterQtyBuy_C + vds.ClusterQtyBuy_D) -
                        (vds.ClusterQtySell_A + vds.ClusterQtySell_B + vds.ClusterQtySell_C + vds.ClusterQtySell_D);
                    ClusterQtyAvg_last = (vds.ClusterQtyBuy_A + vds.ClusterQtyBuy_B + vds.ClusterQtyBuy_C + vds.ClusterQtyBuy_D) -
                                            (vds.ClusterQtySell_A + vds.ClusterQtySell_B + vds.ClusterQtySell_C + vds.ClusterQtySell_D);

                    //+ VOL
                    if (firstRound)
                    {
                        vds.ClusterVolMid = (vds.ClusterVolBuy_A + vds.ClusterVolBuy_B + vds.ClusterVolBuy_C + vds.ClusterVolBuy_D) -
                                                (vds.ClusterVolSell_A + vds.ClusterVolSell_B + vds.ClusterVolSell_C + vds.ClusterVolSell_D);
                    }
                    else
                    {
                        vds.ClusterVolMid = ((vds.ClusterVolBuy_A + vds.ClusterVolBuy_B + vds.ClusterVolBuy_C + vds.ClusterVolBuy_D) -
                                                (vds.ClusterVolSell_A + vds.ClusterVolSell_B + vds.ClusterVolSell_C + vds.ClusterVolSell_D)) / 2;
                    }


                    vds.ClusterVolAvg = ClusterVolAvg_last + (vds.ClusterVolBuy_A + vds.ClusterVolBuy_B + vds.ClusterVolBuy_C + vds.ClusterVolBuy_D) -
                                            (vds.ClusterVolSell_A + vds.ClusterVolSell_B + vds.ClusterVolSell_C + vds.ClusterVolSell_D);
                    ClusterVolAvg_last = (vds.ClusterVolBuy_A + vds.ClusterVolBuy_B + vds.ClusterVolBuy_C + vds.ClusterVolBuy_D) -
                                            (vds.ClusterVolSell_A + vds.ClusterVolSell_B + vds.ClusterVolSell_C + vds.ClusterVolSell_D);
                    //- VOL

                    //+ A-B-C Cluster
                } // else // if (tick.Op == initClusterOp)


                // + подсчет кластера 
                if (tick.Op == TradeOp.Buy)
                {
                    vds.ClusterVolBuy += tick.Volume;
                    countClusterQtyBuy = (tick.IntPrice - initClusterPrice) / cfg.u.PriceStep;
                    if (countClusterQtyBuy == 0)
                        countClusterQtyBuy = 1;

                    vds.ClusterQtyBuy += countClusterQtyBuy;


                    // for cluster A-B-C count
                    lastClusterVol += tick.Volume;
                }
                else if (tick.Op == TradeOp.Sell)
                {
                    vds.ClusterVolSell += tick.Volume;
                    countClusterQtySell = (initClusterPrice - tick.IntPrice) / cfg.u.PriceStep;
                    if (countClusterQtySell == 0)
                        countClusterQtySell = 1;

                    vds.ClusterQtySell += countClusterQtySell;

                    // for cluster A-B-C count
                    lastClusterVol += tick.Volume;
                }
                else
                {
                    mainForm.LogToScreeen("MinStrategy exchange: tick operation error in minStrategy.");
                    throw new Exception("tick operation error in minStrategy.");
                }
                   

                // - подсчет кластера

                // cluster delta:
                vds.ClusterDeltaBuy = vds.ClusterQtyBuy * vds.ClusterVolBuy;
                vds.ClusterDeltaSell = vds.ClusterQtySell * vds.ClusterVolSell;
                if (vds.ClusterQtyBuy != 0 && vds.ClusterVolBuy != 0)
                    vds.ClusterDeltaBuy_2 = vds.ClusterQtyBuy / vds.ClusterVolBuy;
                else
                    vds.ClusterDeltaBuy_2 = 0;
                if (vds.ClusterQtySell != 0 && vds.ClusterVolSell != 0)
                    vds.ClusterDeltaSell_2 = vds.ClusterQtySell / vds.ClusterVolSell;
                else
                    vds.ClusterDeltaSell_2 = 0;
                //- Clusters

                //+ количество A-B-C накоплением
                if (tick.Op == TradeOp.Sell)
                {
                    if (tick.Volume <= VolumeCfg.volD) // 1-3
                    {
                        vds.QtySell_D++;
                        vds.VolSell_D += tick.Volume;
                    }
                    else
                        if (tick.Volume <= VolumeCfg.volC) // 3-25
                    {
                        vds.QtySell_C++;
                        vds.VolSell_C += tick.Volume;
                    }
                    else
                            if (tick.Volume <= VolumeCfg.volB) // 25-90
                    {
                        vds.QtySell_B++;
                        vds.VolSell_B += tick.Volume;
                    }
                    else // >90
                    {
                        vds.QtySell_A++;
                        vds.VolSell_A += tick.Volume;
                    }

                }
                else
                    if (tick.Op == TradeOp.Buy)
                {
                    if (tick.Volume <= VolumeCfg.volD)
                    {
                        vds.QtyBuy_D++;
                        vds.VolBuy_D += tick.Volume;
                    }
                    else
                        if (tick.Volume <= VolumeCfg.volC)
                    {
                        vds.QtyBuy_C++;
                        vds.VolBuy_C += tick.Volume;
                    }
                    else
                            if (tick.Volume <= VolumeCfg.volB)
                    {
                        vds.QtyBuy_B++;
                        vds.VolBuy_B += tick.Volume;
                    }
                    else
                    {
                        vds.QtyBuy_A++;
                        vds.VolBuy_A += tick.Volume;
                    }
                }
                //- количество A-B-C накоплением

                //+ GAS 13042015
                if (initCountPrice == 0)
                    initCountPrice = tick.IntPrice;

                if (initCountPrice != tick.IntPrice)
                {
                    if (initCountPrice < tick.IntPrice) // растем
                    {
                        vds.BoundBuyVol += 0;
                        vds.BoundSellVol += vds.countSellVol;
                        vds.BreakBuyVol += vds.countBuyVol;
                        vds.BreakSellVol += 0;

                        vds.BoundBuyQty += 0;
                        vds.BoundSellQty += vds.countSellQty;
                        vds.BreakBuyQty += vds.countBuyQty;
                        vds.BreakSellQty += 0;
                    }
                    else
                        if (initCountPrice > tick.IntPrice) // падаем
                    {
                        vds.BoundBuyVol += vds.countBuyVol;
                        vds.BoundSellVol += 0;
                        vds.BreakBuyVol += 0;
                        vds.BreakSellVol += vds.countSellVol;

                        vds.BoundBuyQty += vds.countBuyQty;
                        vds.BoundSellQty += 0;
                        vds.BreakBuyQty += 0;
                        vds.BreakSellQty += vds.countSellQty;
                    }

                    vds.countBuyVol = 0;
                    vds.countSellVol = 0;

                    vds.countBuyQty = 0;
                    vds.countSellQty = 0;

                    initCountPrice = tick.IntPrice;
                }

                if (tick.Op == TradeOp.Sell)
                {
                    vds.countSellQty++;
                    vds.countSellVol += tick.Volume;
                }
                else
                    if (tick.Op == TradeOp.Buy)
                {
                    vds.countBuyQty++;
                    vds.countBuyVol += tick.Volume;
                }
                //- GAS 13042015
                //- 26072015 добавляем функциона 2.0


                // END

                firstRound = false;
                firstLoopInCandle = false;

                //volumes.Remove(lastDT);
                //volumes.Add(lastDT, vds);
                volumes[lastDT] = vds;
                //VolumeVector.Remove(tick.SecCode);
                //VolumeVector.Add(tick.SecCode, volumes);
                VolumeVector[tick.SecCode] = volumes;

                // Save last tick
                if (vds.date >= lastTickDt && !lastCandleSend)
                {
                    vds.middle = vds.PriceVolumeSum / vds.Volume;
                    if (vds.close != 0)
                    {
                        saveStockElementToDB(vds);
                    }


                    lastCandleSend = true;
                }
            }
        }


        // **********************************************************************

        public override void ProcessSetting(Setting setting)
        {
            if (setting.DateTime < firstTickDt)
                return;

            lock (SettingVector)
            {
                string SecCode = setting.SecCode;

                DateTime lastDT = DateTime.MinValue;
                if (lastDateTimeSetting.TryGetValue(SecCode, out lastDT) == false)
                {
                    // начальная установка даты и времени отсчета
                    lastDT = setting.DateTime;
                }

                Dictionary<DateTime, SettingDataElement> settings = new Dictionary<DateTime, SettingDataElement>();
                if (SettingVector.TryGetValue(SecCode, out settings) == false)
                {
                    createNewSetting(SecCode, lastDT);

                    SettingVector.TryGetValue(SecCode, out settings);
                }

                SettingDataElement sde = new SettingDataElement(SecCode, lastDT);
                settings.TryGetValue(lastDT, out sde);

                // если время больше XXX мин, делаем новую группу объема
                long csTicks = currentTimeFrame * TimeSpan.TicksPerSecond;
                if (setting.DateTime.Ticks - lastDT.Ticks >= csTicks)
                { // новая группа объема

                    lastDT = setting.DateTime;
                    createNewSetting(SecCode, lastDT);
                    settings.TryGetValue(lastDT, out sde);
                }

                sde.ClassCode = setting.ClassCode; // код класса
                sde.ClassName = setting.ClassName; // класс бумаги
                sde.OptionBase = setting.OptionBase; // +1 базовый актив опциона
                sde.MatDate = setting.MatDate; // +1 дата погашения
                sde.DaysToMatDate = setting.DaysToMatDate; // дней до погашения

                if (sde.NumbidsOpen == 0)
                    sde.NumbidsOpen = setting.Numbids;
                if (sde.NumbidsHigh == 0)
                    sde.NumbidsHigh = setting.Numbids;
                else
                    if (sde.NumbidsHigh < setting.Numbids)
                    sde.NumbidsHigh = setting.Numbids;
                if (sde.NumbidsLow == 0)
                    sde.NumbidsLow = setting.Numbids;
                else
                    if (sde.NumbidsLow > setting.Numbids)
                    sde.NumbidsLow = setting.Numbids;
                sde.NumbidsClose = setting.Numbids;

                if (sde.NumoffersOpen == 0)
                    sde.NumoffersOpen = setting.Numoffers;
                if (sde.NumoffersHigh == 0)
                    sde.NumoffersHigh = setting.Numoffers;
                else
                    if (sde.NumoffersHigh < setting.Numoffers)
                    sde.NumoffersHigh = setting.Numoffers;
                if (sde.NumoffersLow == 0)
                    sde.NumoffersLow = setting.Numoffers;
                else
                    if (sde.NumoffersLow > setting.Numoffers)
                    sde.NumoffersLow = setting.Numoffers;
                sde.NumoffersClose = setting.Numoffers;

                if (sde.BiddepthtOpen == 0)
                    sde.BiddepthtOpen = setting.Biddeptht;
                if (sde.BiddepthtHigh == 0)
                    sde.BiddepthtHigh = setting.Biddeptht;
                else
                    if (sde.BiddepthtHigh < setting.Biddeptht)
                    sde.BiddepthtHigh = setting.Biddeptht;
                if (sde.BiddepthtLow == 0)
                    sde.BiddepthtLow = setting.Biddeptht;
                else
                    if (sde.BiddepthtLow > setting.Biddeptht)
                    sde.BiddepthtLow = setting.Biddeptht;
                sde.BiddepthtClose = setting.Biddeptht;

                if (sde.OfferdepthtOpen == 0)
                    sde.OfferdepthtOpen = setting.Offerdeptht;
                if (sde.OfferdepthtHigh == 0)
                    sde.OfferdepthtHigh = setting.Offerdeptht;
                else
                    if (sde.OfferdepthtHigh < setting.Offerdeptht)
                    sde.OfferdepthtHigh = setting.Offerdeptht;
                if (sde.OfferdepthtLow == 0)
                    sde.OfferdepthtLow = setting.Offerdeptht;
                else
                    if (sde.OfferdepthtLow > setting.Offerdeptht)
                    sde.OfferdepthtLow = setting.Offerdeptht;
                sde.OfferdepthtClose = setting.Offerdeptht;

                sde.Voltoday = setting.Voltoday; // количество во всех сделках 
                sde.Valtoday = setting.Valtoday; // текущий оборот в деньгах
                sde.Numtrades = setting.Numtrades; // количество сделок за сегодня

                if (sde.NumcontractsOpen == 0)
                    sde.NumcontractsOpen = setting.Numcontracts;
                if (sde.NumcontractsHigh == 0)
                    sde.NumcontractsHigh = setting.Numcontracts;
                else
                    if (sde.NumcontractsHigh < setting.Numcontracts)
                    sde.NumcontractsHigh = setting.Numcontracts;
                if (sde.NumcontractsLow == 0)
                    sde.NumcontractsLow = setting.Numcontracts;
                else
                    if (sde.NumcontractsLow > setting.Numcontracts)
                    sde.NumcontractsLow = setting.Numcontracts;
                sde.NumcontractsClose = setting.Numcontracts;

                sde.Selldepo = setting.Selldepo; // ГО продавца
                sde.Buydepo = setting.Buydepo; // ГО покупателя
                sde.Strike = setting.Strike; // +1 цена страйк
                sde.OptionType = setting.OptionType; // +1 тип опциона CALL PUT
                sde.Volatility = setting.Volatility; // +1 волатильность опциона


                //settings.Remove(lastDT);
                //settings.Add(lastDT, sde);
                settings[lastDT] = sde;
                //SettingVector.Remove(SecCode);
                //SettingVector.Add(SecCode, settings);
                SettingVector[SecCode] = settings;
            }
        }


        // **********************************************************************
        // **********************************************************************

        async void saveAllVolumeDataToDB()
        {
            foreach (var values in VolumeVector.Values)
            {
                foreach (StockDataElement element in values.Values)
                {
                    if(element.close == 0)
                    {
                        continue;
                    }
                    await saveStockElementToDB(element);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private Task saveStockElementToDB(StockDataElement element)
        {
            return Task.Run(() =>
            {
                SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
                msgpack.ForcePathObject("Symbol").AsString = element.ticker;
                msgpack.ForcePathObject("TimeFrame").AsInteger = currentTimeFrameId;
                msgpack.ForcePathObject("DateTime").AsString = element.date.ToString("o");
                msgpack.ForcePathObject("High").AsFloat = element.high;
                msgpack.ForcePathObject("Low").AsFloat = element.low;

                msgpack.ForcePathObject("Open").AsFloat = element.open;
                msgpack.ForcePathObject("Close").AsFloat = element.close;

                msgpack.ForcePathObject("Middle").AsFloat = element.middle;

                msgpack.ForcePathObject("Volume").AsFloat = element.Volume;
                msgpack.ForcePathObject("VolumeBuy").AsFloat = element.VolumeBuy;
                msgpack.ForcePathObject("VolumeSell").AsFloat = element.VolumeSell;

                msgpack.ForcePathObject("VolumeBuyQty").AsFloat = element.VolumeBuyQty;
                msgpack.ForcePathObject("VolumeSellQty").AsFloat = element.VolumeSellQty;

                msgpack.ForcePathObject("VolumeDiffHigh").AsFloat = element.VolumeDiffHigh;
                msgpack.ForcePathObject("VolumeDiffLow").AsFloat = element.VolumeDiffLow;
                msgpack.ForcePathObject("VolumeDiffClose").AsFloat = element.VolumeDiffClose;

                msgpack.ForcePathObject("VolumeDiffClose3").AsFloat = element.VolumeDiffClose3;
                msgpack.ForcePathObject("VolumeDiffClose6").AsFloat = element.VolumeDiffClose6;
                msgpack.ForcePathObject("VolumeDiffClose9").AsFloat = element.VolumeDiffClose9;
                msgpack.ForcePathObject("VolumeDiffClose21").AsFloat = element.VolumeDiffClose21;

                msgpack.ForcePathObject("VolumeAvg").AsFloat = element.VolumeAvg; // среднее значение накопленное за торговую сессию

                msgpack.ForcePathObject("VolumeMidOpen").AsFloat = element.VolumeMidOpen; // среднее значение от разницы объема
                msgpack.ForcePathObject("VolumeMidHigh").AsFloat = element.VolumeMidHigh; // среднее значение от разницы объема
                msgpack.ForcePathObject("VolumeMidLow").AsFloat = element.VolumeMidLow; // среднее значение от разницы объема
                msgpack.ForcePathObject("VolumeMidClose").AsFloat = element.VolumeMidClose; // среднее значение от разницы объема

                msgpack.ForcePathObject("VolumeMidClose3").AsFloat = element.VolumeMidClose3;
                msgpack.ForcePathObject("VolumeMidClose6").AsFloat = element.VolumeMidClose6;
                msgpack.ForcePathObject("VolumeMidClose9").AsFloat = element.VolumeMidClose9;
                msgpack.ForcePathObject("VolumeMidClose21").AsFloat = element.VolumeMidClose21;

                // A-B-C
                msgpack.ForcePathObject("QtyBuy_A").AsFloat = element.QtyBuy_A;
                msgpack.ForcePathObject("QtyBuy_B").AsFloat = element.QtyBuy_B;
                msgpack.ForcePathObject("QtyBuy_C").AsFloat = element.QtyBuy_C;
                msgpack.ForcePathObject("QtyBuy_D").AsFloat = element.QtyBuy_D;

                msgpack.ForcePathObject("QtySell_A").AsFloat = element.QtySell_A;
                msgpack.ForcePathObject("QtySell_B").AsFloat = element.QtySell_B;
                msgpack.ForcePathObject("QtySell_C").AsFloat = element.QtySell_C;
                msgpack.ForcePathObject("QtySell_D").AsFloat = element.QtySell_D;

                // объем для вычисления среднего значения объема в группе
                msgpack.ForcePathObject("VolBuy_A").AsFloat = element.VolBuy_A;
                msgpack.ForcePathObject("VolBuy_B").AsFloat = element.VolBuy_B;
                msgpack.ForcePathObject("VolBuy_C").AsFloat = element.VolBuy_C;
                msgpack.ForcePathObject("VolBuy_D").AsFloat = element.VolBuy_D;

                msgpack.ForcePathObject("VolSell_A").AsFloat = element.VolSell_A;
                msgpack.ForcePathObject("VolSell_B").AsFloat = element.VolSell_B;
                msgpack.ForcePathObject("VolSell_C").AsFloat = element.VolSell_C;
                msgpack.ForcePathObject("VolSell_D").AsFloat = element.VolSell_D;

                // Delta Open Close
                msgpack.ForcePathObject("DeltaOC_High").AsFloat = element.DeltaOC_High;
                msgpack.ForcePathObject("DeltaOC_Low").AsFloat = element.DeltaOC_Low;
                msgpack.ForcePathObject("DeltaOC_Close").AsFloat = element.DeltaOC_Close;

                msgpack.ForcePathObject("DeltaOC_Close3").AsFloat = element.DeltaOC_Close3;
                msgpack.ForcePathObject("DeltaOC_Close6").AsFloat = element.DeltaOC_Close6;
                msgpack.ForcePathObject("DeltaOC_Close9").AsFloat = element.DeltaOC_Close9;
                msgpack.ForcePathObject("DeltaOC_Close21").AsFloat = element.DeltaOC_Close21;

                // Delta High Low
                msgpack.ForcePathObject("DeltaHL").AsFloat = element.DeltaHL;
                // Delta Open Close
                msgpack.ForcePathObject("DeltaOC_2").AsFloat = element.DeltaOC_2;
                // Delta High Low
                msgpack.ForcePathObject("DeltaHL_2").AsFloat = element.DeltaHL_2;

                msgpack.ForcePathObject("DeltaOH").AsFloat = element.DeltaOH;
                msgpack.ForcePathObject("DeltaOL").AsFloat = element.DeltaOL;
                msgpack.ForcePathObject("DeltaCH").AsFloat = element.DeltaCH;
                msgpack.ForcePathObject("DeltaCL").AsFloat = element.DeltaCL;
                msgpack.ForcePathObject("DeltaOHOLDiff").AsFloat = element.DeltaOHOLDiff;
                msgpack.ForcePathObject("DeltaCHCLDiff").AsFloat = element.DeltaCHCLDiff;

                msgpack.ForcePathObject("DeltaOH_2").AsFloat = element.DeltaOH_2;
                msgpack.ForcePathObject("DeltaOL_2").AsFloat = element.DeltaOL_2;
                msgpack.ForcePathObject("DeltaCH_2").AsFloat = element.DeltaCH_2;
                msgpack.ForcePathObject("DeltaCL_2").AsFloat = element.DeltaCL_2;
                msgpack.ForcePathObject("DeltaOHOLDiff_2").AsFloat = element.DeltaOHOLDiff_2;
                msgpack.ForcePathObject("DeltaCHCLDiff_2").AsFloat = element.DeltaCHCLDiff_2;

                msgpack.ForcePathObject("DeltaOH_Buy").AsFloat = element.DeltaOH_Buy;
                msgpack.ForcePathObject("DeltaOL_Sell").AsFloat = element.DeltaOL_Sell;
                msgpack.ForcePathObject("DeltaCH_Sell").AsFloat = element.DeltaCH_Sell;
                msgpack.ForcePathObject("DeltaCL_Buy").AsFloat = element.DeltaCL_Buy;
                msgpack.ForcePathObject("DeltaOH_Buy_2").AsFloat = element.DeltaOH_Buy_2;
                msgpack.ForcePathObject("DeltaOL_Sell_2").AsFloat = element.DeltaOL_Sell_2;
                msgpack.ForcePathObject("DeltaCH_Sell_2").AsFloat = element.DeltaCH_Sell_2;
                msgpack.ForcePathObject("DeltaCL_Buy_2").AsFloat = element.DeltaCL_Buy_2;

                msgpack.ForcePathObject("DeltaOHOLDiff_Buy_Sell").AsFloat = element.DeltaOHOLDiff_Buy_Sell;
                msgpack.ForcePathObject("DeltaCHCLDiff_Buy_Sell").AsFloat = element.DeltaCHCLDiff_Buy_Sell;
                msgpack.ForcePathObject("DeltaOHOLDiff_Buy_Sell_2").AsFloat = element.DeltaOHOLDiff_Buy_Sell_2;
                msgpack.ForcePathObject("DeltaCHCLDiff_Buy_Sell_2").AsFloat = element.DeltaCHCLDiff_Buy_Sell_2;

                // Delta OC AVG & MID
                msgpack.ForcePathObject("DeltaOCAvg").AsFloat = element.DeltaOCAvg;
                msgpack.ForcePathObject("DeltaOCMidOpen").AsFloat = element.DeltaOCMidOpen;
                msgpack.ForcePathObject("DeltaOCMidHigh").AsFloat = element.DeltaOCMidHigh;
                msgpack.ForcePathObject("DeltaOCMidLow").AsFloat = element.DeltaOCMidLow;
                msgpack.ForcePathObject("DeltaOCMidClose").AsFloat = element.DeltaOCMidClose;

                msgpack.ForcePathObject("DeltaOCMidClose3").AsFloat = element.DeltaOCMidClose3;
                msgpack.ForcePathObject("DeltaOCMidClose6").AsFloat = element.DeltaOCMidClose6;
                msgpack.ForcePathObject("DeltaOCMidClose9").AsFloat = element.DeltaOCMidClose9;
                msgpack.ForcePathObject("DeltaOCMidClose21").AsFloat = element.DeltaOCMidClose21;

                msgpack.ForcePathObject("DeltaOC_2Mid").AsFloat = element.DeltaOC_2Mid;
                msgpack.ForcePathObject("DeltaOC_2Avg").AsFloat = element.DeltaOC_2Avg;

                // Cluster. Кластер это несколько подряд сделок в одном направлении, покупка или продажа.
                msgpack.ForcePathObject("ClusterQtyBuy").AsFloat = element.ClusterQtyBuy;
                msgpack.ForcePathObject("ClusterQtySell").AsFloat = element.ClusterQtySell;

                msgpack.ForcePathObject("ClusterVolBuy").AsFloat = element.ClusterVolBuy;
                msgpack.ForcePathObject("ClusterVolSell").AsFloat = element.ClusterVolSell;

                // Delta cluster. Величина и сила движения в кластере.
                msgpack.ForcePathObject("ClusterDeltaBuy").AsFloat = element.ClusterDeltaBuy;
                msgpack.ForcePathObject("ClusterDeltaSell").AsFloat = element.ClusterDeltaSell;
                msgpack.ForcePathObject("ClusterDeltaBuy_2").AsFloat = element.ClusterDeltaBuy_2;
                msgpack.ForcePathObject("ClusterDeltaSell_2").AsFloat = element.ClusterDeltaSell_2;

                // Cluster A-B-C
                msgpack.ForcePathObject("ClusterQtyBuy_A").AsFloat = element.ClusterQtyBuy_A;
                msgpack.ForcePathObject("ClusterQtyBuy_B").AsFloat = element.ClusterQtyBuy_B;
                msgpack.ForcePathObject("ClusterQtyBuy_C").AsFloat = element.ClusterQtyBuy_C;
                msgpack.ForcePathObject("ClusterQtyBuy_D").AsFloat = element.ClusterQtyBuy_D;

                msgpack.ForcePathObject("ClusterQtySell_A").AsFloat = element.ClusterQtySell_A;
                msgpack.ForcePathObject("ClusterQtySell_B").AsFloat = element.ClusterQtySell_B;
                msgpack.ForcePathObject("ClusterQtySell_C").AsFloat = element.ClusterQtySell_C;
                msgpack.ForcePathObject("ClusterQtySell_D").AsFloat = element.ClusterQtySell_D;

                msgpack.ForcePathObject("ClusterVolBuy_A").AsFloat = element.ClusterVolBuy_A; // объем для вычисления среднего значения объема в группе
                msgpack.ForcePathObject("ClusterVolBuy_B").AsFloat = element.ClusterVolBuy_B;
                msgpack.ForcePathObject("ClusterVolBuy_C").AsFloat = element.ClusterVolBuy_C;
                msgpack.ForcePathObject("ClusterVolBuy_D").AsFloat = element.ClusterVolBuy_D;

                msgpack.ForcePathObject("ClusterVolSell_A").AsFloat = element.ClusterVolSell_A;
                msgpack.ForcePathObject("ClusterVolSell_B").AsFloat = element.ClusterVolSell_B;
                msgpack.ForcePathObject("ClusterVolSell_C").AsFloat = element.ClusterVolSell_C;
                msgpack.ForcePathObject("ClusterVolSell_D").AsFloat = element.ClusterVolSell_D;
                // --------------------------------

                msgpack.ForcePathObject("ClusterMid_A").AsFloat = element.ClusterMid_A;
                msgpack.ForcePathObject("ClusterMid_B").AsFloat = element.ClusterMid_B;
                msgpack.ForcePathObject("ClusterMid_C").AsFloat = element.ClusterMid_C;
                msgpack.ForcePathObject("ClusterMid_D").AsFloat = element.ClusterMid_D;

                msgpack.ForcePathObject("ClusterAvg_A").AsFloat = element.ClusterAvg_A;
                msgpack.ForcePathObject("ClusterAvg_B").AsFloat = element.ClusterAvg_B;
                msgpack.ForcePathObject("ClusterAvg_C").AsFloat = element.ClusterAvg_C;
                msgpack.ForcePathObject("ClusterAvg_D").AsFloat = element.ClusterAvg_D;

                msgpack.ForcePathObject("ClusterQtyAvg").AsFloat = element.ClusterQtyAvg;

                msgpack.ForcePathObject("ClusterVolMid").AsFloat = element.ClusterVolMid;
                msgpack.ForcePathObject("ClusterVolAvg").AsFloat = element.ClusterVolAvg;

                msgpack.ForcePathObject("BoundBuyVol").AsFloat = element.BoundBuyVol;
                msgpack.ForcePathObject("BoundSellVol").AsFloat = element.BoundSellVol;
                msgpack.ForcePathObject("BreakBuyVol").AsFloat = element.BreakBuyVol;
                msgpack.ForcePathObject("BreakSellVol").AsFloat = element.BreakSellVol;

                msgpack.ForcePathObject("BoundBuyQty").AsFloat = element.BoundBuyQty;
                msgpack.ForcePathObject("BoundSellQty").AsFloat = element.BoundSellQty;
                msgpack.ForcePathObject("BreakBuyQty").AsFloat = element.BreakBuyQty;
                msgpack.ForcePathObject("BreakSellQty").AsFloat = element.BreakSellQty;

                //+ empty fields
                msgpack.ForcePathObject("m1_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m1_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m1_percent_value").AsFloat = 0;

                msgpack.ForcePathObject("m3_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m3_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m3_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m3_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m3_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m3_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m5_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m5_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m5_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m5_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m5_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m5_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m10_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m10_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m10_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m10_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m10_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m10_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m15_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m15_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m15_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m15_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m15_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m15_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m30_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m30_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m30_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m30_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m30_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m30_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m60_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m60_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m60_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m60_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m60_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m60_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m240_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m240_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m240_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m240_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m240_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m240_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("m1200_max_value").AsFloat = 0;
                msgpack.ForcePathObject("m1200_min_value").AsFloat = 0;
                msgpack.ForcePathObject("m1200_percent_value").AsFloat = 0;
                msgpack.ForcePathObject("m1200_max_avg").AsFloat = 0;
                msgpack.ForcePathObject("m1200_min_avg").AsFloat = 0;
                msgpack.ForcePathObject("m1200_percent_avg").AsFloat = 0;

                msgpack.ForcePathObject("VaR").AsFloat = 0;
                //- empty fields

                //msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
                msgpack.ForcePathObject("Version").AsString = "1.0";

                byte[] packData = msgpack.Encode2Bytes();

                // Redis
                using (var redisClient = redisManager.GetClient())
                {
                    var ret = redisClient.Custom("XADD", "volquotes", "*", "volquote", packData);
                }

            });
        }

        // **********************************************************************

        async void saveAllSettingDataToDB()
        {
            foreach (var values in SettingVector.Values)
            {
                foreach (SettingDataElement element in values.Values)
                {
                    await saveSettingElementToDB(element);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private Task saveSettingElementToDB(SettingDataElement element)
        {
            return Task.Run(() =>
            {
                if (element.MatDate == null)
                    return;

                SimpleMsgPack.MsgPack msgpack = new SimpleMsgPack.MsgPack();
                msgpack.ForcePathObject("Symbol").AsString = element.SecCode;
                msgpack.ForcePathObject("TimeFrame").AsInteger = currentTimeFrameId;
                msgpack.ForcePathObject("DateTime").AsString = element.DateTime.ToString("o");

                msgpack.ForcePathObject("SecCode").AsString = element.SecCode; // код бумаги
                msgpack.ForcePathObject("ClassCode").AsString = element.ClassCode; // код класса
                msgpack.ForcePathObject("ClassName").AsString = element.ClassName; // класс бумаги
                msgpack.ForcePathObject("OptionBase").AsString = element.OptionBase; // +1 базовый актив опциона
                msgpack.ForcePathObject("MatDate").AsString = element.MatDate.Value.ToString("o"); // +1 дата погашения
                msgpack.ForcePathObject("DaysToMatDate").AsInteger = element.DaysToMatDate; // дней до погашения

                msgpack.ForcePathObject("NumbidsOpen").AsInteger = element.NumbidsOpen; // количество заявкок на покупку
                msgpack.ForcePathObject("NumbidsHigh").AsInteger = element.NumbidsHigh; // количество заявкок на покупку
                msgpack.ForcePathObject("NumbidsLow").AsInteger = element.NumbidsLow; // количество заявкок на покупку
                msgpack.ForcePathObject("NumbidsClose").AsInteger = element.NumbidsClose; // количество заявкок на покупку

                msgpack.ForcePathObject("NumoffersOpen").AsInteger = element.NumoffersOpen; // количество заявок на продажу
                msgpack.ForcePathObject("NumoffersHigh").AsInteger = element.NumoffersHigh; // количество заявок на продажу
                msgpack.ForcePathObject("NumoffersLow").AsInteger = element.NumoffersLow; // количество заявок на продажу
                msgpack.ForcePathObject("NumoffersClose").AsInteger = element.NumoffersClose; // количество заявок на продажу

                msgpack.ForcePathObject("BiddepthtOpen").AsInteger = element.BiddepthtOpen; // суммарный спрос контрактов
                msgpack.ForcePathObject("BiddepthtHigh").AsInteger = element.BiddepthtHigh; // суммарный спрос контрактов
                msgpack.ForcePathObject("BiddepthtLow").AsInteger = element.BiddepthtLow; // суммарный спрос контрактов
                msgpack.ForcePathObject("BiddepthtClose").AsInteger = element.BiddepthtClose; // суммарный спрос контрактов

                msgpack.ForcePathObject("OfferdepthtOpen").AsInteger = element.OfferdepthtOpen; // суммарное предложение контрактов
                msgpack.ForcePathObject("OfferdepthtHigh").AsInteger = element.OfferdepthtHigh; // суммарное предложение контрактов
                msgpack.ForcePathObject("OfferdepthtLow").AsInteger = element.OfferdepthtLow; // суммарное предложение контрактов
                msgpack.ForcePathObject("OfferdepthtClose").AsInteger = element.OfferdepthtClose; // суммарное предложение контрактов

                msgpack.ForcePathObject("Voltoday").AsInteger = element.Voltoday; // количество во всех сделках 
                msgpack.ForcePathObject("Valtoday").AsFloat = element.Valtoday; // текущий оборот в деньгах
                msgpack.ForcePathObject("Numtrades").AsInteger = element.Numtrades; // количество сделок за сегодня

                msgpack.ForcePathObject("NumcontractsOpen").AsInteger = element.NumcontractsOpen; // количество открытых позиций (Открытый Интерес)
                msgpack.ForcePathObject("NumcontractsHigh").AsInteger = element.NumcontractsHigh; // количество открытых позиций (Открытый Интерес)
                msgpack.ForcePathObject("NumcontractsLow").AsInteger = element.NumcontractsLow; // количество открытых позиций (Открытый Интерес)
                msgpack.ForcePathObject("NumcontractsClose").AsInteger = element.NumcontractsClose; // количество открытых позиций (Открытый Интерес)

                msgpack.ForcePathObject("Selldepo").AsFloat = element.Selldepo; // ГО продавца
                msgpack.ForcePathObject("Buydepo").AsFloat = element.Buydepo; // ГО покупателя
                msgpack.ForcePathObject("Strike").AsFloat = element.Strike; // +1 цена страйк
                msgpack.ForcePathObject("OptionType").AsString = element.OptionType; // +1 тип опциона CALL PUT
                msgpack.ForcePathObject("Volatility").AsFloat = element.Volatility; // +1 волатильность опциона

                //msgpack.ForcePathObject("TimeStamp").AsString = now.ToString("o");
                msgpack.ForcePathObject("Version").AsString = "1.0";

                byte[] packData = msgpack.Encode2Bytes();

                // Redis
                using (var redisClient = redisManager.GetClient())
                {
                    var ret = redisClient.Custom("XADD", "settingquotes", "*", "settingquote", packData);
                }

            });
        }

        // **********************************************************************

        public override void ProcessTrade(Trade trade) { }

        public override void ProcessSpread(Spread spread) { }

        public override void ProcessQuotes(Quote[] quotes) { }

        public override void ProcessPutOrder(PutOrder putOrder) { }
    }
}
