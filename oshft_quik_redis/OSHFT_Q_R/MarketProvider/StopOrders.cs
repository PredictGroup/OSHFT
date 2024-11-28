
using System;
using System.Collections.Generic;

namespace OSHFT_Q_R
{
    class StopOrders
    {
        // **********************************************************************

        struct StopOrder
        {
            public ulong Id;
            public double StopPrice;
            public double ExecPrice;
            public long Quantity;

            public StopOrder(ulong id, double stopPrice, double execPrice, long quantity)
            {
                this.Id = id;
                this.StopPrice = stopPrice;
                this.ExecPrice = execPrice;
                this.Quantity = quantity;
            }
        }

        // **********************************************************************

        IDataReceiver dataReceiver;
        TermManager tmgr;

        static ulong lastId;

        List<StopOrder> orders;

        // **********************************************************************

        public StopOrders(TermManager tmgr, IDataReceiver dataReceiver)
        {
            this.tmgr = tmgr;
            this.dataReceiver = dataReceiver;

            orders = new List<StopOrder>();
        }

        // **********************************************************************

        public ulong CreateOrder(double stopPrice, double execPrice, long quantity)
        {
            StopOrder order = new StopOrder(--lastId, stopPrice, execPrice, quantity);

            lock (orders)
            {
                orders.Add(order);

                dataReceiver.PutOwnOrder(new OwnOrder(
                  order.Id,
                  order.StopPrice,
                  order.Quantity,
                  0));
            }

            return order.Id;
        }

        // **********************************************************************

        public void KillOrder(ulong id, double price)
        {
            lock (orders)
            {
                int i = 0;

                while (i < orders.Count)
                {
                    StopOrder order = orders[i];

                    if (order.Id == id || order.StopPrice == price)
                    {
                        orders.RemoveAt(i);
                        dataReceiver.PutOwnOrder(new OwnOrder(order.Id, order.StopPrice, 0, 0));
                    }
                    else
                        i++;
                }
            }
        }

        // **********************************************************************

        public void Clear()
        {
            if (orders.Count > 0)
                lock (orders)
                {
                    for (int i = 0; i < orders.Count; i++)
                    {
                        StopOrder order = orders[i];
                        dataReceiver.PutOwnOrder(new OwnOrder(order.Id, order.StopPrice, 0, 0));
                    }

                    orders.Clear();
                }
        }

        // **********************************************************************
    }
}
