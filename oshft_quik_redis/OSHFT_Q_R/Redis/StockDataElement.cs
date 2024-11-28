using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSHFT_Q_R
{
    [Serializable()]
    public struct StockDataElement
    {
        public string ticker;

        public DateTime date;

        public double high;
        public double low;

        public double open;
        public double close;

        public double middle; // Middle price

        public double Volume;
        public double VolumeBuy;
        public double VolumeSell;

        public double VolumeBuyQty;
        public double VolumeSellQty;

        public double PriceVolumeSum; // для формулы средней цены: сумма(цена * объем) / объем  - значение цены где рисуется блин

        // ------------------------------------------------------------

        public double VolumeDiffHigh;
        public double VolumeDiffLow;
        public double VolumeDiffClose;

        public double VolumeDiffClose3;
        public double VolumeDiffClose6;
        public double VolumeDiffClose9;
        public double VolumeDiffClose21;

        public double VolumeAvg; // среднее значение накопленное за торговую сессию
        public double VolumeMidOpen;
        public double VolumeMidHigh;
        public double VolumeMidLow;
        public double VolumeMidClose; // среднее значение от разницы объема

        public double VolumeMidClose3;
        public double VolumeMidClose6;
        public double VolumeMidClose9;
        public double VolumeMidClose21;



        // A-B-C
        public int QtyBuy_A;
        public int QtyBuy_B;
        public int QtyBuy_C;
        public int QtyBuy_D;

        public int QtySell_A;
        public int QtySell_B;
        public int QtySell_C;
        public int QtySell_D;

        public int VolBuy_A; // объем для вычисления среднего значения объема в группе
        public int VolBuy_B;
        public int VolBuy_C;
        public int VolBuy_D;

        public int VolSell_A;
        public int VolSell_B;
        public int VolSell_C;
        public int VolSell_D;

        // Delta Open Close
        public double DeltaOC_High;
        public double DeltaOC_Low;
        public double DeltaOC_Close;

        public double DeltaOC_Close3;
        public double DeltaOC_Close6;
        public double DeltaOC_Close9;
        public double DeltaOC_Close21;

        // Delta High Low
        public double DeltaHL;
        // Delta Open Close
        public double DeltaOC_2;
        // Delta High Low
        public double DeltaHL_2;

        public double DeltaOH;
        public double DeltaOL;

        public double DeltaCH;
        public double DeltaCL;

        public double DeltaOHOLDiff;
        public double DeltaCHCLDiff;

        public double DeltaOH_2;
        public double DeltaOL_2;

        public double DeltaCH_2;
        public double DeltaCL_2;

        public double DeltaOHOLDiff_2;
        public double DeltaCHCLDiff_2;

        //BUY VS SELL
        public double DeltaOH_Buy;
        public double DeltaOL_Sell;

        public double DeltaCH_Sell;
        public double DeltaCL_Buy;

        public double DeltaOHOLDiff_Buy_Sell;
        public double DeltaCHCLDiff_Buy_Sell;

        public double DeltaOH_Buy_2;
        public double DeltaOL_Sell_2;

        public double DeltaCH_Sell_2;
        public double DeltaCL_Buy_2;

        public double DeltaOHOLDiff_Buy_Sell_2;
        public double DeltaCHCLDiff_Buy_Sell_2;


        // Delta OC AVG & MID
        public double DeltaOCAvg;
        public double DeltaOCMidOpen;
        public double DeltaOCMidHigh;
        public double DeltaOCMidLow;
        public double DeltaOCMidClose;

        public double DeltaOCMidClose3;
        public double DeltaOCMidClose6;
        public double DeltaOCMidClose9;
        public double DeltaOCMidClose21;

        public double DeltaOC_2Mid;
        public double DeltaOC_2Avg;

        // Cluster. Кластер это несколько подряд сделок в одном направлении, покупка или продажа.
        public double ClusterQtyBuy;
        public double ClusterQtySell;

        public int ClusterVolBuy;
        public int ClusterVolSell;

        // Delta cluster. Величина и сила движения в кластере.
        public double ClusterDeltaBuy;
        public double ClusterDeltaSell;
        public double ClusterDeltaBuy_2;
        public double ClusterDeltaSell_2;


        // Cluster A-B-C
        public int ClusterQtyBuy_A;
        public int ClusterQtyBuy_B;
        public int ClusterQtyBuy_C;
        public int ClusterQtyBuy_D;

        public int ClusterQtySell_A;
        public int ClusterQtySell_B;
        public int ClusterQtySell_C;
        public int ClusterQtySell_D;

        public int ClusterVolBuy_A; // объем для вычисления среднего значения объема в группе
        public int ClusterVolBuy_B;
        public int ClusterVolBuy_C;
        public int ClusterVolBuy_D;

        public int ClusterVolSell_A;
        public int ClusterVolSell_B;
        public int ClusterVolSell_C;
        public int ClusterVolSell_D;

        public double ClusterMid_A;
        public double ClusterMid_B;
        public double ClusterMid_C;
        public double ClusterMid_D;

        public double ClusterAvg_A;
        public double ClusterAvg_B;
        public double ClusterAvg_C;
        public double ClusterAvg_D;

        public double ClusterQtyAvg;

        public double ClusterVolMid;
        public double ClusterVolAvg;

        public double countBuyVol;
        public double countSellVol;

        public double BoundBuyVol;
        public double BoundSellVol;
        public double BreakBuyVol;
        public double BreakSellVol;

        public double countBuyQty;
        public double countSellQty;

        public double BoundBuyQty;
        public double BoundSellQty;
        public double BreakBuyQty;
        public double BreakSellQty;

        // ------------------------------------------------------------


        public StockDataElement(string Ticker, DateTime dt)
        {
            this.ticker = Ticker;
            this.date = dt;

            this.high = 0;
            this.low = double.MaxValue;

            this.open = 0;
            this.close = 0;

            this.middle = 0;

            this.Volume = 0;
            this.VolumeBuy = 0;
            this.VolumeSell = 0;

            this.VolumeBuyQty = 0;
            this.VolumeSellQty = 0;

            this.PriceVolumeSum = 0;

            this.VolumeDiffHigh = 0;
            this.VolumeDiffLow = 0;
            this.VolumeDiffClose = 0;

            this.VolumeDiffClose3 = 0;
            this.VolumeDiffClose6 = 0;
            this.VolumeDiffClose9 = 0;
            this.VolumeDiffClose21 = 0;

            this.VolumeAvg = 0;
            this.VolumeMidOpen = 0;
            this.VolumeMidHigh = 0;
            this.VolumeMidLow = 0;
            this.VolumeMidClose = 0;

            this.VolumeMidClose3 = 0;
            this.VolumeMidClose6 = 0;
            this.VolumeMidClose9 = 0;
            this.VolumeMidClose21 = 0;

            this.DeltaOC_High = 0;
            this.DeltaOC_Low = 0;
            this.DeltaOC_Close = 0;

            this.DeltaOC_Close3 = 0;
            this.DeltaOC_Close6 = 0;
            this.DeltaOC_Close9 = 0;
            this.DeltaOC_Close21 = 0;

            this.DeltaHL = 0;
            this.DeltaOC_2 = 0;
            this.DeltaHL_2 = 0;
            this.DeltaOH = 0;
            this.DeltaOL = 0;
            this.DeltaCH = 0;
            this.DeltaCL = 0;
            this.DeltaOH_2 = 0;
            this.DeltaOL_2 = 0;
            this.DeltaCH_2 = 0;
            this.DeltaCL_2 = 0;
            this.DeltaOH_Buy = 0;
            this.DeltaOL_Sell = 0;
            this.DeltaCH_Sell = 0;
            this.DeltaCL_Buy = 0;
            this.DeltaOH_Buy_2 = 0;
            this.DeltaOL_Sell_2 = 0;
            this.DeltaCH_Sell_2 = 0;
            this.DeltaCL_Buy_2 = 0;

            this.DeltaOCAvg = 0;
            this.DeltaOCMidOpen = 0;
            this.DeltaOCMidHigh = 0;
            this.DeltaOCMidLow = 0;
            this.DeltaOCMidClose = 0;

            this.DeltaOCMidClose3 = 0;
            this.DeltaOCMidClose6 = 0;
            this.DeltaOCMidClose9 = 0;
            this.DeltaOCMidClose21 = 0;

            this.DeltaOC_2Mid = 0;
            this.DeltaOC_2Avg = 0;

            DeltaOHOLDiff = 0;
            DeltaCHCLDiff = 0;
            DeltaOHOLDiff_2 = 0;
            DeltaCHCLDiff_2 = 0;
            DeltaOHOLDiff_Buy_Sell = 0;
            DeltaCHCLDiff_Buy_Sell = 0;
            DeltaOHOLDiff_Buy_Sell_2 = 0;
            DeltaCHCLDiff_Buy_Sell_2 = 0;


            // Cluster

            this.ClusterQtyBuy = 0;
            this.ClusterQtySell = 0;

            this.ClusterVolBuy = 0;
            this.ClusterVolSell = 0;

            this.ClusterDeltaBuy = 0;
            this.ClusterDeltaSell = 0;
            this.ClusterDeltaBuy_2 = 0;
            this.ClusterDeltaSell_2 = 0;

            // A-B-C
            this.QtyBuy_A = 0;
            this.QtyBuy_B = 0;
            this.QtyBuy_C = 0;
            this.QtyBuy_D = 0;

            this.QtySell_A = 0;
            this.QtySell_B = 0;
            this.QtySell_C = 0;
            this.QtySell_D = 0;

            this.VolBuy_A = 0;
            this.VolBuy_B = 0;
            this.VolBuy_C = 0;
            this.VolBuy_D = 0;

            this.VolSell_A = 0;
            this.VolSell_B = 0;
            this.VolSell_C = 0;
            this.VolSell_D = 0;

            // Cluster A-B-C
            this.ClusterQtyBuy_A = 0;
            this.ClusterQtyBuy_B = 0;
            this.ClusterQtyBuy_C = 0;
            this.ClusterQtyBuy_D = 0;

            this.ClusterQtySell_A = 0;
            this.ClusterQtySell_B = 0;
            this.ClusterQtySell_C = 0;
            this.ClusterQtySell_D = 0;

            this.ClusterVolBuy_A = 0;
            this.ClusterVolBuy_B = 0;
            this.ClusterVolBuy_C = 0;
            this.ClusterVolBuy_D = 0;

            this.ClusterVolSell_A = 0;
            this.ClusterVolSell_B = 0;
            this.ClusterVolSell_C = 0;
            this.ClusterVolSell_D = 0;

            this.ClusterMid_A = 0;
            this.ClusterMid_B = 0;
            this.ClusterMid_C = 0;
            this.ClusterMid_D = 0;

            this.ClusterAvg_A = 0;
            this.ClusterAvg_B = 0;
            this.ClusterAvg_C = 0;
            this.ClusterAvg_D = 0;

            this.ClusterQtyAvg = 0;

            this.ClusterVolMid = 0;
            this.ClusterVolAvg = 0;

            this.countBuyVol = 0;
            this.countSellVol = 0;

            this.BoundBuyVol = 0;
            this.BoundSellVol = 0;
            this.BreakBuyVol = 0;
            this.BreakSellVol = 0;

            this.countBuyQty = 0;
            this.countSellQty = 0;

            this.BoundBuyQty = 0;
            this.BoundSellQty = 0;
            this.BreakBuyQty = 0;
            this.BreakSellQty = 0;
        }

    }
}
