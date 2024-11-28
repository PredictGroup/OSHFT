
using System;

namespace OSHFT_Q_R
{
    class Position
    {
        // **********************************************************************

        IDataReceiver dataReceiver;
        TermManager tmgr;

        double pricesum;

        long byOrders;
        bool byOrdersUpdated;

        ulong stopOrderId;
        double stopOrderPrice;

        TermManager.Transaction takeProfit;

        // **********************************************************************

        public long Quantity { get; protected set; }
        public double Price { get; protected set; }

        // **********************************************************************

        public Position(TermManager tmgr, IDataReceiver dataReceiver)
        {
            this.tmgr = tmgr;
            this.dataReceiver = dataReceiver;

            byOrdersUpdated = true;
        }

        // **********************************************************************

        public void PutOwnTrade(OwnTrade trade)
        {

            // ------------------------------------------------------------

            long nq = this.Quantity + trade.Quantity;

            if (Math.Sign(nq) != Math.Sign(this.Quantity))
            {


                this.pricesum = trade.Price * nq;
            }
            else
                this.pricesum += trade.Price * trade.Quantity;

            this.Quantity = nq;

            // ------------------------------------------------------------

            if (stopOrderId != 0)
            {
                tmgr.KillStopOrder(stopOrderId);
                stopOrderId = 0;
            }

            if (takeProfit != null)
            {
                tmgr.CancelAction(takeProfit);
                takeProfit = null;
            }

            // ------------------------------------------------------------

            if (Quantity == 0)
                Price = 0;
            else
            {
                Price = pricesum / Quantity;

                if (Quantity > 0)
                {
                    if (cfg.u.StopOffset != 0)
                    {
                        stopOrderPrice = OSHFT_Q_R.Price.Ceil(Price - cfg.u.StopOffset);

                        stopOrderId = tmgr.CreateStopOrder(
                          stopOrderPrice,
                          stopOrderPrice - cfg.u.StopSlippage,
                          -Quantity);
                    }

                    if (cfg.u.TakeOffset != 0)
                    {
                        takeProfit = tmgr.ExecAction(new OwnAction(
                          TradeOp.Sell,
                          BaseQuote.Absolute,
                          OSHFT_Q_R.Price.Ceil(Price + cfg.u.TakeOffset),
                          Quantity));
                    }
                }
                else
                {
                    if (cfg.u.StopOffset != 0)
                    {
                        stopOrderPrice = OSHFT_Q_R.Price.Floor(Price + cfg.u.StopOffset);

                        stopOrderId = tmgr.CreateStopOrder(
                          stopOrderPrice,
                          stopOrderPrice + cfg.u.StopSlippage,
                          -Quantity);
                    }

                    if (cfg.u.TakeOffset != 0)
                    {
                        takeProfit = tmgr.ExecAction(new OwnAction(
                          TradeOp.Buy,
                          BaseQuote.Absolute,
                          OSHFT_Q_R.Price.Floor(Price - cfg.u.TakeOffset),
                          -Quantity));
                    }
                }

            }

            dataReceiver.PutPosition(Quantity, Price);
        }

        // **********************************************************************

        public bool ByOrdersUpdated
        {
            get { return byOrdersUpdated && !(byOrdersUpdated = false); }
        }

        // **********************************************************************

        public long ByOrders
        {
            get { return byOrders; }
            set { byOrders = value; byOrdersUpdated = true; }
        }

        // **********************************************************************

        public void Clear()
        {
            pricesum = 0;

            Quantity = 0;
            Price = 0;

            ByOrders = 0;

            dataReceiver.PutPosition(Quantity, Price);
        }

        // **********************************************************************
    }
}
