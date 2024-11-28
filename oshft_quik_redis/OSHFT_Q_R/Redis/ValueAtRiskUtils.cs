using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSHFT_Q_R
{

    //int begIdx = 0;
    //int NBElement = 0;
    //double[] real = null;

    //stdDev((close.Count() - 35), (close.Count() - 1), close, 10, 1, out begIdx, out NBElement, out real);

    //double ValueSum = 10000; // определяем риски на сумму в 10000

    //int time = 1; // 1 день

    //double W = real[34] / close[close.Count() - 1];
    //double VaR = getStockVaR(W, ValueSum, time);

    public static class ValueAtRiskUtils
    {
        /**
         * Сумма под риском
         * 
         * 
         * **/
        public static double getStockVaR(double W, double Volume, int time = 1, double K = 2.32634)
        {
            double VaR1 = (W * K); // for 1 day in 1/x

            double TimePow = Math.Sqrt(time);
            double VaR = TimePow * VaR1 * Volume;

            return VaR;
        }

        public static void stdDev(int startIdx,
                                int endIdx,
                                double[] inReal,
                                int optInTimePeriod,
                                double optInNbDev,
                                out int outBegIdx,
                                out int outNBElement,
                                out double[] outReal)
        {
            outBegIdx = 0;
            outNBElement = 0;
            outReal = new double[100];
            int i;

            double tempReal;
            if (startIdx < 0)
                return;
            if ((endIdx < 0) || (endIdx < startIdx))
                return;
            if ((int)optInTimePeriod == (Int32.MinValue))
                optInTimePeriod = 5;
            else if (((int)optInTimePeriod < 2) || ((int)optInTimePeriod > 100000))
                return;
            if (optInNbDev == (-4e+37))
                optInNbDev = 1.000000e+0;
            else if ((optInNbDev < -3.000000e+37) || (optInNbDev > 3.000000e+37))
                return;
            TA_INT_VAR(startIdx, endIdx,
               inReal, optInTimePeriod,
               out outBegIdx, out outNBElement, out outReal);
            //if( retCode != RetCode.Success )
            //   return retCode;
            if (optInNbDev != 1.0)
            {
                for (i = 0; i < (int)outNBElement; i++)
                {
                    tempReal = outReal[i];
                    if (!(tempReal < (0.00000000000001)))
                        outReal[i] = Math.Sqrt(tempReal) * optInNbDev;
                    else
                        outReal[i] = (double)0.0;
                }
            }
            else
            {
                for (i = 0; i < (int)outNBElement; i++)
                {
                    tempReal = outReal[i];
                    if (!(tempReal < (0.00000000000001)))
                        outReal[i] = Math.Sqrt(tempReal);
                    else
                        outReal[i] = (double)0.0;
                }
            }
            return;
        }

        private static void TA_INT_VAR(int startIdx,
                                   int endIdx,
                                   double[] inReal,
                                   int optInTimePeriod,
                                   out int outBegIdx,
                                   out int outNBElement,
                                   out double[] outReal)
        {

            outReal = new double[100];
            double tempReal, periodTotal1, periodTotal2, meanValue1, meanValue2;
            int i, outIdx, trailingIdx, nbInitialElementNeeded;
            nbInitialElementNeeded = (optInTimePeriod - 1);
            if (startIdx < nbInitialElementNeeded)
                startIdx = nbInitialElementNeeded;
            if (startIdx > endIdx)
            {
                outBegIdx = 0;
                outNBElement = 0;
                outReal = null;
                return;
            }
            periodTotal1 = 0;
            periodTotal2 = 0;
            trailingIdx = startIdx - nbInitialElementNeeded;
            i = trailingIdx;
            if (optInTimePeriod > 1)
            {
                while (i < startIdx)
                {
                    tempReal = inReal[i++];
                    periodTotal1 += tempReal;
                    tempReal *= tempReal;
                    periodTotal2 += tempReal;
                }
            }
            outIdx = 0;
            do
            {
                tempReal = inReal[i++];
                periodTotal1 += tempReal;
                tempReal *= tempReal;
                periodTotal2 += tempReal;
                meanValue1 = periodTotal1 / optInTimePeriod;
                meanValue2 = periodTotal2 / optInTimePeriod;
                tempReal = inReal[trailingIdx++];
                periodTotal1 -= tempReal;
                tempReal *= tempReal;
                periodTotal2 -= tempReal;
                outReal[outIdx++] = meanValue2 - meanValue1 * meanValue1;
            } while (i <= endIdx);
            outNBElement = outIdx;
            outBegIdx = startIdx;
            return;
        }


    }
}
