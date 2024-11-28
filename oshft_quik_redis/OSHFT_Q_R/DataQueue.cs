using System;
using System.Collections.Generic;

namespace OSHFT_Q_R
{
    // ************************************************************************
    // *                              DataQueue                               *
    // ************************************************************************

    sealed class DataQueue<T>
    {
        // --------------------------------------------------------------

        Queue<T> queue = new Queue<T>();

        // --------------------------------------------------------------

        Action<T> handlers;

        // --------------------------------------------------------------

        public void RegisterHandler(Action<T> handler)
        {
            handlers += handler;
        }

        // --------------------------------------------------------------

        public void UnregisterHandler(Action<T> handler)
        {
            handlers -= handler;
        }

        // --------------------------------------------------------------

        public void Enqueue(T data)
        {
            lock (queue)
                queue.Enqueue(data);
        }

        // --------------------------------------------------------------

        public void Process()
        {
            lock (queue)
            {
                if (handlers != null)
                    while (queue.Count > 0)
                        handlers.Invoke(queue.Dequeue());
                else
                    queue.Clear();
            }
        }

        // --------------------------------------------------------------

        public int Length { get { return queue.Count; } }

        // --------------------------------------------------------------
    }


    // ************************************************************************
    // *                              TicksQueue                              *
    // ************************************************************************

    sealed class TicksQueue
    {
        // --------------------------------------------------------------

        Queue<Tick> queue = new Queue<Tick>();

        // --------------------------------------------------------------

        Action<Tick> handlers;

        // --------------------------------------------------------------

        public void RegisterHandler(Action<Tick> handler)
        {
            handlers += handler;
        }

        // --------------------------------------------------------------

        public void UnregisterHandler(Action<Tick> handler)
        {
            handlers -= handler;
        }

        // --------------------------------------------------------------

        public void Enqueue(Tick trade)
        {
            lock (queue)
                queue.Enqueue(trade);
        }

        // --------------------------------------------------------------

        public void Process()
        {
            lock (queue)
            {
                if (handlers != null)
                    while (queue.Count > 0)
                        handlers.Invoke(queue.Dequeue());
                else
                    queue.Clear();
            }
        }

        // --------------------------------------------------------------

        public int Length { get { return queue.Count; } }

        // --------------------------------------------------------------
    }


    // ************************************************************************
    // *                             TradesQueue                              *
    // ************************************************************************

    interface ITradesHandler { void PutTrade(string skey, Trade trade); }


    // ************************************************************************
    // *                             PutOrdersQueue                           *
    // ************************************************************************

    interface IPutOrdersHandler { void PutOrder(string skey, PutOrder putOrder); }


    // ************************************************************************
    // *                              OrdersList                              *
    // ************************************************************************

    interface IOrdersHandler { void OrdersUpdated(double price); }

    // ************************************************************************

    class OrdersList
    {
        // --------------------------------------------------------------

        Dictionary<double, List<OwnOrder>> list;

        Queue<OwnOrder> queue;
        List<IOrdersHandler> handlers;

        // --------------------------------------------------------------

        public OrdersList()
        {
            list = new Dictionary<double, List<OwnOrder>>();

            queue = new Queue<OwnOrder>();
            handlers = new List<IOrdersHandler>();
        }

        // --------------------------------------------------------------

        public void RegisterHandler(IOrdersHandler handler)
        {
            handlers.Add(handler);
        }

        // --------------------------------------------------------------

        public void Enqueue(OwnOrder order)
        {
            lock (queue)
                queue.Enqueue(order);
        }

        // --------------------------------------------------------------

        public void UpdateHandlers()
        {
            lock (queue)
            {
                while (queue.Count > 0)
                {
                    OwnOrder order = queue.Dequeue();
                    List<OwnOrder> orders;

                    if (list.TryGetValue(order.Price, out orders))
                    {
                        int i = 0;

                        while (i < orders.Count && orders[i].Id != order.Id) i++;

                        if (i < orders.Count)
                        {
                            if (order.Active == 0)
                            {
                                if (orders.Count == 1)
                                    list.Remove(order.Price);
                                else
                                    orders.RemoveAt(i);
                            }
                            else
                                orders[i] = order;
                        }
                        else if (order.Active != 0)
                            orders.Add(order);
                    }
                    else if (order.Active != 0)
                    {
                        orders = new List<OwnOrder>();
                        orders.Add(order);
                        list.Add(order.Price, orders);
                    }

                    for (int i = 0; i < handlers.Count; i++)
                        handlers[i].OrdersUpdated(order.Price);
                }
            }
        }

        // --------------------------------------------------------------

        public int QueueLength { get { return queue.Count; } }
        public bool Contains(int price) { return list.ContainsKey(price); }

        // --------------------------------------------------------------

        public IList<OwnOrder> this[int price]
        {
            get
            {
                List<OwnOrder> orders;

                if (list.TryGetValue(price, out orders))
                    return orders.AsReadOnly();
                else
                    return null;
            }
        }

        // --------------------------------------------------------------

        public void Clear()
        {
            lock (queue)
            {
                double[] keys = new double[list.Keys.Count];
                list.Keys.CopyTo(keys, 0);

                queue.Clear();
                list.Clear();

                foreach (int price in keys)
                    for (int i = 0; i < handlers.Count; i++)
                        handlers[i].OrdersUpdated(price);
            }
        }

        // --------------------------------------------------------------
    }

}
