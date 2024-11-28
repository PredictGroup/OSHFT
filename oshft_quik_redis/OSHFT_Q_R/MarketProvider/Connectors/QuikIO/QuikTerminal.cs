
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using OSHFT_Q_R;
using Trans2QuikAPI;

namespace OSHFT_Q_R.QuikIO
{
    class QuikTerminal : ITerminal
    {
        // **********************************************************************

        const string NotConnectedStr = "Нет соединения.";

        TermManager mgr;

        static UInt32 transId;

        Int32 error = 0;
        //StringBuilder msg;
        UInt32 err_msg_size = 256;
        Byte[] msg;

        bool working;
        bool connected;

        Timer connecting;

        // **********************************************************************

        public string Name { get { return "QUIK"; } }

        // **********************************************************************

        public QuikTerminal(TermManager mgr)
        {
            this.mgr = mgr;

            msg = new Byte[err_msg_size];
            connecting = new Timer(TryConnect);
        }

        // **********************************************************************
        // *                             Соединение                             *
        // **********************************************************************

        void TryConnect(Object state)
        {
            if (working)
            {
                try
                {
                    if (
                      Trans2Quik.SET_CONNECTION_STATUS_CALLBACK(StatusCallback, ref error, msg, err_msg_size) != 0
                      ||
                      Trans2Quik.SET_TRANSACTIONS_REPLY_CALLBACK(TransactionReplyCallback, ref error, msg, err_msg_size) != 0
                      )
                    {
                        mgr.ConnectionUpdate(TermConnection.None, msg.ToString());
                        return;
                    }

                    int result = Trans2Quik.connect(cfg.u.QuikFolder, ref error, msg, err_msg_size);
                    if (result != 0 && result != 4)
                    {
                        mgr.ConnectionUpdate(TermConnection.None, msg.ToString());
                        return;
                    }
                }
                catch (Exception e)
                {
                    mgr.ConnectionUpdate(TermConnection.None, e.Message);
                    return;
                }

                connected = true;

                if (Trans2Quik.ubsubscribe_orders() != 0
                  || Trans2Quik.ubsubscribe_trades() != 0
                  || Trans2Quik.subscribe_orders(cfg.u.ClassCode, cfg.u.SecCode) != 0
                  || Trans2Quik.START_ORDERS(OrderStatusCallback) != 0
                  || Trans2Quik.subscribe_trades(cfg.u.ClassCode, cfg.u.SecCode) != 0
                  || Trans2Quik.START_TRADES(TradeStatusCallback) != 0)
                {
                    mgr.ConnectionUpdate(TermConnection.Partial, "Соединение установлено неполностью");
                    return;
                }

                mgr.ConnectionUpdate(TermConnection.Full, "Соединение с сервером QUIK установлено");
            }

            connecting.Change(Timeout.Infinite, Timeout.Infinite);
        }

        // **********************************************************************

        public void Connect()
        {
            working = true;
            connecting.Change(0, cfg.QuikTryConnectInterval);
        }

        // **********************************************************************

        public void Disconnect()
        {
            working = false;
            connecting.Change(Timeout.Infinite, Timeout.Infinite);

            if (connected)
                Trans2Quik.disconnect(ref error, msg, err_msg_size);
        }

        // **********************************************************************
        void StatusCallback(Int32 nConnectionEvent, UInt32 nExtendedErrorCode, byte[] lpstrInfoMessage)
        {
            switch (nConnectionEvent)
            {
                case 9:
                    connecting.Change(0, cfg.QuikTryConnectInterval);
                    break;

                case 11:
                    connected = false;
                    connecting.Change(0, cfg.QuikTryConnectInterval);
                    break;
            }
        }

        // **********************************************************************
        // *                               Сделки                               *
        // **********************************************************************

        void TradeStatusCallback(
                Int32 nMode
                , UInt64 nNumber
                , UInt64 nOrderNumber
                , string ClassCode
                , string SecCode
                , Double dPrice
                , Int64 nQty
                , Double dValue
                , Int32 nIsSell
                , IntPtr pTradeDescriptor)
        {
            if (nMode == 0)
            {
                if (nIsSell != 0)
                    nQty = -nQty;

                string comment = Trans2Quik.TRANS2QUIK_TRADE_BROKERREF(pTradeDescriptor);

                if ((comment.EndsWith(cfg.FullProgName) || cfg.u.AcceptAllTrades))
                    mgr.PutOwnTrade(new OwnTrade(Price.GetInt(dPrice), nQty));
            }
        }

        // **********************************************************************
        // *                               Заявки                               *
        // **********************************************************************
        // transaction_reply_callback_impl 
        void TransactionReplyCallback(
          int r,
          Int32 err,
          Int32 rc,
          UInt32 tid,
          UInt64 order_id,
        [MarshalAs(UnmanagedType.LPStr)] string msg,
        IntPtr pTransactionReplyDescriptor)
        {
            if (r == 0 && rc == 3)
                mgr.ActionReply(tid, (UInt64)order_id, null);
            else
                mgr.ActionReply(tid, (UInt64)order_id, msg.Length == 0 ? r + ", " + err : msg.ToString());
        }

        // **********************************************************************

        void OrderStatusCallback(
                                Int32 nMode,
                                Int32 dwTransID,
                                UInt64 nOrderNum,
                                string ClassCode,
                                string SecCode,
                                Double dPrice,
                                Int64 nBalance,
                                Double dValue,
                                Int32 nIsSell,
                                Int32 nStatus,
                                IntPtr nOrderDescriptor)
        {
            if (nMode == 0)
            {
                long filled;

                if (nIsSell == 0)
                    filled = Trans2Quik.TRANS2QUIK_ORDER_QTY(nOrderDescriptor) - nBalance;
                else
                {
                    filled = nBalance - Trans2Quik.TRANS2QUIK_ORDER_QTY(nOrderDescriptor);
                    nBalance = -nBalance;
                }

                if (nStatus == 1)
                    mgr.OrderUpdate((UInt64)nOrderNum, nBalance, filled);
                else
                    mgr.OrderUpdate((UInt64)nOrderNum, 0, filled);
            }
        }

        // **********************************************************************
        // *                        Управление заявками                         *
        // **********************************************************************

        string SendOrder(char op, double price, long quantity, out UInt32 tid)
        {
            if (connected)
            {
                tid = ++transId;

                int r = Trans2Quik.send_async_transaction(
                  "TRANS_ID=" + tid +
                  "; ACCOUNT=" + cfg.u.QuikAccount +
                  "; CLIENT_CODE=" + cfg.u.QuikClientCode + "/" + cfg.FullProgName +
                  "; SECCODE=" + cfg.u.SecCode +
                  "; CLASSCODE=" + cfg.u.ClassCode +
                  "; ACTION=NEW_ORDER; OPERATION=" + op +
                  "; PRICE=" + Price.GetRaw(price) +
                  "; QUANTITY=" + quantity +
                  ";",
                  ref error, msg, err_msg_size);

                if (r == 0)
                    return null;
                else
                {
                    tid = 0;
                    return msg.Length == 0 ? r + ", " + error : msg.ToString();
                }
            }
            else
            {
                tid = 0;
                return NotConnectedStr;
            }
        }

        // **********************************************************************

        public string SendBuyOrder(double price, long quantity, out UInt32 tid)
        {
            return SendOrder('B', price, quantity, out tid);
        }

        // **********************************************************************

        public string SendSellOrder(double price, long quantity, out UInt32 tid)
        {
            return SendOrder('S', price, quantity, out tid);
        }

        // **********************************************************************

        public string KillOrder(UInt64 oid)
        {
            if (connected)
            {
                transId++;

                int r = Trans2Quik.send_async_transaction(
                  "TRANS_ID=" + transId +
                  "; SECCODE=" + cfg.u.SecCode +
                  "; CLASSCODE=" + cfg.u.ClassCode +
                  "; ACTION=KILL_ORDER; ORDER_KEY=" + oid +
                  ";",
                  ref error, msg, err_msg_size);

                if (r == 0)
                    return null;
                else
                    return msg.Length == 0 ? r + ", " + error : msg.ToString();
            }
            else
                return NotConnectedStr;
        }

        // **********************************************************************
    }
}
