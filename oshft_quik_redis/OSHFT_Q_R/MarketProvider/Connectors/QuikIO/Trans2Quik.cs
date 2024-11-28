
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Trans2QuikAPI
{
    static class Trans2Quik
    {
#if (X64)
        public const string DLL_NAME = "TRANS2QUIK.DLL";
#else
        public const string DLL_NAME = "TRANS2QUIK.DLL";
#endif

        #region C++ signatures
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SEND_SYNC_TRANSACTION (
        //    LPSTR lpstTransactionString, 
        //    long* pnReplyCode, 
        //    PDWORD pdwTransId, 
        //    double* pdOrderNum, 
        //    LPSTR lpstrResultMessage, 
        //    DWORD dwResultMessageSize, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SEND_ASYNC_TRANSACTION (
        //    LPSTR lpstTransactionString, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_CONNECT (
        //    LPSTR lpstConnectionParamsString, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_DISCONNECT (
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SET_CONNECTION_STATUS_CALLBACK (
        //    TRANS2QUIK_CONNECTION_STATUS_CALLBACK pfConnectionStatusCallback, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SET_TRANSACTIONS_REPLY_CALLBACK (
        //    TRANS2QUIK_TRANSACTION_REPLY_CALLBACK pfTransactionReplyCallback, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_IS_QUIK_CONNECTED (
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_IS_DLL_CONNECTED (
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);
        #endregion

        #region Константы возвращаемых значений
        public const Int32 TRANS2QUIK_SUCCESS = 0;
        public const Int32 TRANS2QUIK_FAILED = 1;
        public const Int32 TRANS2QUIK_QUIK_TERMINAL_NOT_FOUND = 2;
        public const Int32 TRANS2QUIK_DLL_VERSION_NOT_SUPPORTED = 3;
        public const Int32 TRANS2QUIK_ALREADY_CONNECTED_TO_QUIK = 4;
        public const Int32 TRANS2QUIK_WRONG_SYNTAX = 5;
        public const Int32 TRANS2QUIK_QUIK_NOT_CONNECTED = 6;
        public const Int32 TRANS2QUIK_DLL_NOT_CONNECTED = 7;
        public const Int32 TRANS2QUIK_QUIK_CONNECTED = 8;
        public const Int32 TRANS2QUIK_QUIK_DISCONNECTED = 9;
        public const Int32 TRANS2QUIK_DLL_CONNECTED = 10;
        public const Int32 TRANS2QUIK_DLL_DISCONNECTED = 11;
        public const Int32 TRANS2QUIK_MEMORY_ALLOCATION_ERROR = 12;
        public const Int32 TRANS2QUIK_WRONG_CONNECTION_HANDLE = 13;
        public const Int32 TRANS2QUIK_WRONG_INPUT_PARAMS = 14;
        #endregion

        public static string ResultToString(Int32 Result)
        {
            switch (Result)
            {
                case TRANS2QUIK_SUCCESS:                                //0
                    return "TRANS2QUIK_SUCCESS";
                case TRANS2QUIK_FAILED:                                 //1
                    return "TRANS2QUIK_FAILED";
                case TRANS2QUIK_QUIK_TERMINAL_NOT_FOUND:                //2
                    return "TRANS2QUIK_QUIK_TERMINAL_NOT_FOUND";
                case TRANS2QUIK_DLL_VERSION_NOT_SUPPORTED:              //3
                    return "TRANS2QUIK_DLL_VERSION_NOT_SUPPORTED";
                case TRANS2QUIK_ALREADY_CONNECTED_TO_QUIK:              //4
                    return "TRANS2QUIK_ALREADY_CONNECTED_TO_QUIK";
                case TRANS2QUIK_WRONG_SYNTAX:                           //5
                    return "TRANS2QUIK_WRONG_SYNTAX";
                case TRANS2QUIK_QUIK_NOT_CONNECTED:                     //6
                    return "TRANS2QUIK_QUIK_NOT_CONNECTED";
                case TRANS2QUIK_DLL_NOT_CONNECTED:                      //7
                    return "TRANS2QUIK_DLL_NOT_CONNECTED";
                case TRANS2QUIK_QUIK_CONNECTED:                         //8
                    return "TRANS2QUIK_QUIK_CONNECTED";
                case TRANS2QUIK_QUIK_DISCONNECTED:                      //9
                    return "TRANS2QUIK_QUIK_DISCONNECTED";
                case TRANS2QUIK_DLL_CONNECTED:                          //10
                    return "TRANS2QUIK_DLL_CONNECTED";
                case TRANS2QUIK_DLL_DISCONNECTED:                       //11
                    return "TRANS2QUIK_DLL_DISCONNECTED";
                case TRANS2QUIK_MEMORY_ALLOCATION_ERROR:                //12
                    return "TRANS2QUIK_MEMORY_ALLOCATION_ERROR";
                case TRANS2QUIK_WRONG_CONNECTION_HANDLE:                //13
                    return "TRANS2QUIK_WRONG_CONNECTION_HANDLE";
                case TRANS2QUIK_WRONG_INPUT_PARAMS:                     //14
                    return "TRANS2QUIK_WRONG_INPUT_PARAMS";
                default:
                    return "UNKNOWN_VALUE";
            }
        }

        public static string ByteToString(byte[] Str)
        {
            int count = 0;
            for (int i = 0; i < Str.Length; ++i)
            {
                if (0 == Str[i])
                {
                    count = i;
                    break;
                }
            }
            return System.Text.Encoding.Default.GetString(Str, 0, count);
        }

        public static string DateTimeStr(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            long high = filetime.dwHighDateTime;
            long ft = (high << 32) + filetime.dwLowDateTime;

            return DateTime.FromFileTimeUtc(ft).ToString();
        }

        #region connect
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_CONNECT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 connect(
            string lpcstrConnectionParamsString,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        public static void connect_test(bool FinalPause)
        {
            Byte[] EMsg = new Byte[50];
            UInt32 EMsgSz = 50;
            Int32 ExtEC = 0, rez = -1;
            String path = @"C:\Users\Andy\Documents\oshft_quik_redis\OSHFT_Q_R\bin\Debug";

            try
            {
                string[] lines = System.IO.File.ReadAllLines(@"settings.txt", System.Text.Encoding.GetEncoding(1251));
                path = lines[0];
            }
            catch (Exception e)
            {
                path = PATH_2_QUIK;
                Console.WriteLine(e.Message);
            }

            rez = connect(path, ref ExtEC, EMsg, EMsgSz);
            Console.WriteLine("test_q.connect_test>\t\t{0} {1} ", rez & 255, ResultToString(rez & 255));
            //Console.WriteLine(" ExtEC={0}, EMsg={1}, EMsgSz={2}", (ExtEC & 255), EMsg, EMsgSz);
            if (FinalPause) Console.ReadLine();
        }
        #endregion

        #region is_dll_connected
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_IS_DLL_CONNECTED",
            CallingConvention = CallingConvention.StdCall)]
        static extern Int32 is_dll_connected(
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        public static void is_dll_connected_test(bool FinalPause)
        {
            Byte[] EMsg = new Byte[50];
            UInt32 EMsgSz = 50;
            Int32 ExtEC = 0, rez = -1;
            rez = is_dll_connected(ref ExtEC, EMsg, EMsgSz);
            Console.WriteLine("test_q.is_dll_connected_test>\t{0} {1}", (rez & 255), ResultToString(rez & 255));
            if (FinalPause) Console.ReadLine();
        }

        public static bool IsDLLConnected()
        {
            Byte[] EMsg = new Byte[50];
            UInt32 EMsgSz = 50;
            Int32 ExtEC = 0, rez = -1;
            rez = is_dll_connected(ref ExtEC, EMsg, EMsgSz) & 255;
            if (rez == 10)
                return true;
            else
                return false;
        }
        #endregion

        #region disconnect
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_DISCONNECT",
          CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 disconnect(
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);
        public static void disconnect_test(bool FinalPause)
        {
            Console.WriteLine("test_q.disconnect_test>.....\n for 'TRANS2QUIK_SET_CONNECTION_STATUS_CALLBACK' test\n press QUIK.button <Прекратить обработку>");
            Console.WriteLine(" or press <any key> to continue...");
            Console.ReadKey();
            Byte[] dEMsg = new Byte[50];
            UInt32 dEMsgSz = 50;
            Int32 dExtEC = 0, drez = -1;
            drez = disconnect(ref dExtEC, dEMsg, dEMsgSz);
            Console.WriteLine("test_q.disconnect_test>\t\t{0} {1}", (drez & 255), ResultToString(drez & 255));
            //Console.WriteLine(" ExtEC={0}, EMsg={1}, EMsgSz={2}", (ExtEC & 255).ToString(), ByteToString(EMsg), EMsgSz);
            if (FinalPause) Console.ReadLine();
        }
        #endregion

        #region is_quik_connected
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_IS_QUIK_CONNECTED",
            CallingConvention = CallingConvention.StdCall)]
        static extern Int32 is_quik_connected(
             ref Int32 pnExtendedErrorCode,
             byte[] lpstrErrorMessage,
             UInt32 dwErrorMessageSize);
        public static void IsQuikConnected(ref bool ret)
        {
            Byte[] EMsg = new Byte[50];
            Int32 ExtEC = 0, rez = -1;
            UInt32 EMsgSz = 50;
            rez = is_quik_connected(ref ExtEC, EMsg, EMsgSz) & 255;
            if (rez == 8)
                ret = true;
            else
                ret = false;
        }
        public static bool IsQuikConnected()
        {
            Byte[] EMsg = new Byte[50];
            Int32 ExtEC = 0, rez = -1;
            UInt32 EMsgSz = 50;
            rez = is_quik_connected(ref ExtEC, EMsg, EMsgSz) & 255;
            if (rez == 8)
                return true;
            else
                return false;
        }
        public static void is_quik_connected_test(bool FinalPause)
        {
            Byte[] EMsg = new Byte[50];
            Int32 ExtEC = 0, rez = -1;
            UInt32 EMsgSz = 50;
            rez = is_quik_connected(ref ExtEC, EMsg, EMsgSz);
            Console.WriteLine("test_q.is_quik_connected_test>\t{0} {1}", (rez & 255), ResultToString(rez & 255));
            //Console.WriteLine(" ExtEC={0}, EMsg={1}, EMsgSz={2}",(ExtEC & 255).ToString(), ByteToString(EMsg), EMsgSz);
            if (FinalPause) Console.ReadLine();
        }
        #endregion

        #region send_sync_transaction
        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SEND_SYNC_TRANSACTION (
        //    LPSTR lpstTransactionString, 
        //    long* pnReplyCode, 
        //    PDWORD pdwTransId, 
        //    double* pdOrderNum, 
        //    LPSTR lpstrResultMessage, 
        //    DWORD dwResultMessageSize, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstErrorMessage, 
        //    DWORD dwErrorMessageSize);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_SEND_SYNC_TRANSACTION",
            CallingConvention = CallingConvention.StdCall)]
        static extern Int32 send_sync_transaction(
            string lpstTransactionString,
            ref Int32 pnReplyCode,
            ref int pdwTransId,
            ref UInt64 pnOrderNum,
            byte[] lpstrResultMessage,
            UInt32 dwResultMessageSize,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        public static void send_sync_transaction_test(
                                string transactionStr,
                                ref UInt64 OrderNum,
                                ref int TransID)
        {
            Console.Write("\ntest_q.send_sync_transaction_test>\n");

            Byte[] EMsg = new Byte[50];
            Byte[] ResMsg = new Byte[50];
            Int32 ExtEC = -100, rez = -1, ReplyCd = 0;
            UInt32 ResMsgSz = 50, EMsgSz = 50;

            rez = send_sync_transaction(transactionStr, ref ReplyCd, ref TransID, ref OrderNum,
                 ResMsg, ResMsgSz, ref ExtEC, EMsg, EMsgSz);

            Console.WriteLine("{0} {1}", (rez & 255), ResultToString(rez & 255));
            Console.WriteLine(" ExtEC={0}, EMsg={1}, EMsgSz={2}",
                (ExtEC & 255).ToString(), ByteToString(EMsg), EMsgSz);

            String resStr = ByteToString(ResMsg);
            resStr = resStr.Trim();

            Console.WriteLine(" ReplyCode={0} TransID={1}  OrderNum={2} \n ResMsg={3}, ResMsgSz={4}, Transaction={5}",
                ReplyCd, TransID, OrderNum, resStr, ResMsgSz, transactionStr);
        }
        #endregion

        #region send_async_transaction
        public delegate void TRANSACTION_REPLY_CALLBACK(
            Int32 nTransactionResult,
            Int32 nTransactionExtendedErrorCode,
            Int32 nTransactionReplyCode,
            UInt32 dwTransId,
            UInt64 dOrderNum,
            [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage,
            IntPtr pTransReplyDescriptor);

        [DllImport(DLL_NAME,
                    EntryPoint = "TRANS2QUIK_SET_TRANSACTIONS_REPLY_CALLBACK",
                    CallingConvention = CallingConvention.StdCall)]
        static extern Int32 SET_TRANSACTIONS_REPLY_CALLBACK_UNMGR(
            TRANSACTION_REPLY_CALLBACK_UNMGR pfTransactionReplyCallback,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        public static Int32 SET_TRANSACTIONS_REPLY_CALLBACK(
            TRANSACTION_REPLY_CALLBACK pfTransactionReplyCallback,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize)
        {
            transaction_reply_callback = pfTransactionReplyCallback;
            return SET_TRANSACTIONS_REPLY_CALLBACK_UNMGR(
              transaction_reply_callback_unmgr,
              ref pnExtendedErrorCode,
              lpstrErrorMessage,
              dwErrorMessageSize);
        }

        [DllImport(DLL_NAME,
                    EntryPoint = "TRANS2QUIK_SEND_ASYNC_TRANSACTION",
                    CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 send_async_transaction(
            [MarshalAs(UnmanagedType.LPStr)] string transactionString,
            ref Int32 nExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        #region Protocol_1_3

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_CLASS_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_CLASS_CODE_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_CLASS_CODE(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_CLASS_CODE_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_SEC_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_SEC_CODE_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_SEC_CODE(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_SEC_CODE_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_PRICE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRANSACTION_REPLY_PRICE(IntPtr tradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_QUANTITY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int64 TRANS2QUIK_TRANSACTION_REPLY_QUANTITY(IntPtr tradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_BALANCE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int64 TRANS2QUIK_TRANSACTION_REPLY_BALANCE(IntPtr tradeDescriptor);


        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_FIRMID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_FIRMID_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_FIRMID(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_FIRMID_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_ACCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_ACCOUNT_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_ACCOUNT(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_ACCOUNT_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_CLIENT_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_CLIENT_CODE_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_CLIENT_CODE(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_CLIENT_CODE_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_BROKERREF",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_BROKERREF_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_BROKERREF(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_BROKERREF_IMPL(tradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRANSACTION_REPLY_EXCHANGE_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRANSACTION_REPLY_EXCHANGE_CODE_IMPL(IntPtr tradeDescriptor);
        public static string TRANS2QUIK_TRANSACTION_REPLY_EXCHANGE_CODE(IntPtr tradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRANSACTION_REPLY_EXCHANGE_CODE_IMPL(tradeDescriptor));
        }

        #endregion

        public static Int32 send_async_transaction_test(string transactionString)
        {
            UInt32 err_msg_size = 256;
            Byte[] err_msg = new Byte[err_msg_size];
            Int32 nExtendedErrorCode = 0;

            Int32 res = send_async_transaction(
                            transactionString,
                            ref nExtendedErrorCode,
                            err_msg,
                            err_msg_size);

            Console.WriteLine("Send async transaction res={0} {1}",
                (res & 255),
                ResultToString(res & 255));

            Console.WriteLine(" ExtEC={0}, EMsg={1}, EMsgSz={2}",
                (nExtendedErrorCode & 255).ToString(),
                ByteToString(err_msg),
                err_msg_size);

            return res;
        }

        //public static void transaction_reply_callback_impl(
        //    Int32 nTransactionResult,
        //    Int32 nTransactionExtendedErrorCode,
        //    Int32 nTransactionReplyCode,
        //    UInt32 dwTransId,
        //    UInt64 nOrderNum,
        //    [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage,
        //    IntPtr pTransactionReplyDescriptor)
        //{
        //    String strOutput =
        //        "TrRes=" + nTransactionResult
        //        + " TrResStr=" + ResultToString(nTransactionResult & 255)
        //        + " TrExErrCode=" + nTransactionExtendedErrorCode
        //        + " TrReplyCode=" + nTransactionReplyCode
        //        + " TrID=" + dwTransId
        //        + " OrderNum=" + nOrderNum
        //        + " ResMsg=" + TransactionReplyMessage
        //        ;

        //    String strProtocol13 = ""
        //        + " ClassCode=" + TRANS2QUIK_TRANSACTION_REPLY_CLASS_CODE(pTransactionReplyDescriptor)
        //        + " SecCode=" + TRANS2QUIK_TRANSACTION_REPLY_SEC_CODE(pTransactionReplyDescriptor)
        //        + " Price=" + TRANS2QUIK_TRANSACTION_REPLY_PRICE(pTransactionReplyDescriptor)
        //        + " Qty=" + TRANS2QUIK_TRANSACTION_REPLY_QUANTITY(pTransactionReplyDescriptor)
        //        + " Balance=" + TRANS2QUIK_TRANSACTION_REPLY_BALANCE(pTransactionReplyDescriptor)
        //        + " FirmId=" + TRANS2QUIK_TRANSACTION_REPLY_FIRMID(pTransactionReplyDescriptor)
        //        + " Account=" + TRANS2QUIK_TRANSACTION_REPLY_ACCOUNT(pTransactionReplyDescriptor)
        //        + " ClientCode=" + TRANS2QUIK_TRANSACTION_REPLY_CLIENT_CODE(pTransactionReplyDescriptor)
        //        + " BrokerRef=" + TRANS2QUIK_TRANSACTION_REPLY_BROKERREF(pTransactionReplyDescriptor)
        //        + " ExchangeCode=" + TRANS2QUIK_TRANSACTION_REPLY_EXCHANGE_CODE(pTransactionReplyDescriptor)
        //        ;

        //    try
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(
        //                @"async_trans.log",
        //                true,
        //                System.Text.Encoding.GetEncoding(1251)))
        //        {
        //            file.WriteLine(strOutput + strProtocol13);
        //            Console.WriteLine("TRANS_REPLY_CALLBACK: " + strOutput + strProtocol13);
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}

        #endregion

        #region orders

        #region time_const
        const Int32 ORDER_QUIKDATE = 0;
        const Int32 ORDER_QUIKTIME = 1;
        const Int32 ORDER_MICROSEC = 2;
        const Int32 ORDER_WITHDRAW_QUIKDATE = 3;
        const Int32 ORDER_WITHDRAW_QUIKTIME = 4;
        const Int32 ORDER_WITHDRAW_MICROSEC = 5;
        #endregion

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_SUBSCRIBE_ORDERS",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 subscribe_orders(
                [MarshalAs(UnmanagedType.LPStr)] string class_code,
                [MarshalAs(UnmanagedType.LPStr)] string sec_code);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_UNSUBSCRIBE_ORDERS",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ubsubscribe_orders();

        public delegate void ORDER_STATUS_CALLBACK(
                Int32 nMode,
                Int32 dwTransID,
                UInt64 nOrderNum,
                string ClassCode,
                string SecCode,
                double dPrice,
                Int64 nBalance,
                Double dValue,
                Int32 nIsSell,
                Int32 nStatus,
                IntPtr pOrderDescriptor);

        [DllImport(DLL_NAME,
                    EntryPoint = "TRANS2QUIK_START_ORDERS",
                    CallingConvention = CallingConvention.StdCall)]
        static extern Int32 START_ORDERS_UNMGR(
            ORDER_STATUS_CALLBACK_UNMGR pfOrderStatusCallback);

        #region order_descriptor_functions
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_QTY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int64 TRANS2QUIK_ORDER_QTY(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_DATE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_DATE(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_TIME(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_DATE_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_DATE_TIME(
                                        IntPtr nOrderDescriptor,
                                        Int32 nTimeType);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_ACTIVATION_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_ACTIVATION_TIME(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_WITHDRAW_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_WITHDRAW_TIME(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_EXPIRY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_EXPIRY(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_ACCRUED_INT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_ORDER_ACCRUED_INT(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_YIELD",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_ORDER_YIELD(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_UID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_UID(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_VISIBLE_QTY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int64 TRANS2QUIK_ORDER_VISIBLE_QTY(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_PERIOD",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_PERIOD(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_FILETIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern System.Runtime.InteropServices.ComTypes.FILETIME
                            TRANS2QUIK_ORDER_FILETIME(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_WITHDRAW_FILETIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern System.Runtime.InteropServices.ComTypes.FILETIME
                            TRANS2QUIK_ORDER_WITHDRAW_FILETIME(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_USERID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_USERID_IMPL(IntPtr nOrderDescriptor);

        public static string TRANS2QUIK_ORDER_USERID(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_USERID_IMPL(nOrderDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_ACCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_ACCOUNT_IMPL(IntPtr nOrderDescriptor);

        public static string TRANS2QUIK_ORDER_ACCOUNT(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_ACCOUNT_IMPL(nOrderDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_BROKERREF",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_BROKERREF_IMPL(IntPtr nOrderDescriptor);

        public static string TRANS2QUIK_ORDER_BROKERREF(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_BROKERREF_IMPL(nOrderDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_CLIENT_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_CLIENT_CODE_IMPL(IntPtr nOrderDescriptor);

        public static string TRANS2QUIK_ORDER_CLIENT_CODE(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_CLIENT_CODE_IMPL(nOrderDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_FIRMID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_FIRMID_IMPL(IntPtr nOrderDescriptor);

        public static string TRANS2QUIK_ORDER_FIRMID(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_FIRMID_IMPL(nOrderDescriptor));
        }

        #region Protocol_1_3

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_VALUE_ENTRY_TYPE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_VALUE_ENTRY_TYPE(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_EXTENDED_FLAGS",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_EXTENDED_FLAGS(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_MIN_QTY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int64 TRANS2QUIK_ORDER_MIN_QTY(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_EXEC_TYPE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_ORDER_EXEC_TYPE(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_AWG_PRICE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_ORDER_AWG_PRICE(IntPtr nOrderDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_ORDER_REJECT_REASON",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_ORDER_REJECT_REASON_IMPL(IntPtr nOrderDescriptor);
        public static string TRANS2QUIK_ORDER_REJECT_REASON(IntPtr nOrderDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_ORDER_REJECT_REASON_IMPL(nOrderDescriptor));
        }

        #endregion // Protocol_1_3

        #endregion

        //public static void order_status_callback_impl(
        //                        Int32 nMode,
        //                        Int32 dwTransID,
        //                        UInt64 nOrderNum,
        //                        string ClassCode,
        //                        string SecCode,
        //                        Double dPrice,
        //                        Int64 nBalance,
        //                        Double dValue,
        //                        Int32 nIsSell,
        //                        Int32 nStatus,
        //                        IntPtr nOrderDescriptor)
        //{
        //    String mainString = "Mode=" + nMode +
        //                        " TransId=" + dwTransID +
        //                        " Num=" + nOrderNum +
        //                        " Class=" + ClassCode +
        //                        " Sec=" + SecCode +
        //                        " Price=" + dPrice +
        //                        " Balance=" + nBalance +
        //                        " Value=" + dValue +
        //                        " IsSell=" + nIsSell +
        //                        " Status=" + nStatus;
        //    String addString = "";
        //    String strString = "";
        //    String strProtocol13 = "";

        //    addString = " Qty=" + TRANS2QUIK_ORDER_QTY(nOrderDescriptor) +
        //        " Date=" + TRANS2QUIK_ORDER_DATE(nOrderDescriptor) +
        //        " Time=" + TRANS2QUIK_ORDER_TIME(nOrderDescriptor) +
        //        " TimeMicroSec=" + TRANS2QUIK_ORDER_DATE_TIME(nOrderDescriptor, ORDER_MICROSEC) +
        //        " ActTime=" + TRANS2QUIK_ORDER_ACTIVATION_TIME(nOrderDescriptor) +
        //        " WDDate=" + TRANS2QUIK_ORDER_DATE_TIME(nOrderDescriptor, ORDER_WITHDRAW_QUIKDATE) +
        //        " WDTime=" + TRANS2QUIK_ORDER_WITHDRAW_TIME(nOrderDescriptor) +
        //        " WDTimeMicrosec=" + TRANS2QUIK_ORDER_DATE_TIME(nOrderDescriptor, ORDER_WITHDRAW_MICROSEC) +
        //        " Expiry=" + TRANS2QUIK_ORDER_EXPIRY(nOrderDescriptor) +
        //        " Accruedint=" + TRANS2QUIK_ORDER_ACCRUED_INT(nOrderDescriptor) +
        //        " Yield=" + TRANS2QUIK_ORDER_YIELD(nOrderDescriptor) +
        //        " UID=" + TRANS2QUIK_ORDER_UID(nOrderDescriptor) +
        //        " VisibleQty=" + TRANS2QUIK_ORDER_VISIBLE_QTY(nOrderDescriptor) +
        //        " Period=" + TRANS2QUIK_ORDER_PERIOD(nOrderDescriptor) +
        //        " OrderFileTime=" + DateTimeStr(TRANS2QUIK_ORDER_FILETIME(nOrderDescriptor)) +
        //        " WithdrawFileTime=" + DateTimeStr(TRANS2QUIK_ORDER_WITHDRAW_FILETIME(nOrderDescriptor));

        //    strProtocol13 = ""
        //        + " ValueEntryType=" + TRANS2QUIK_ORDER_VALUE_ENTRY_TYPE(nOrderDescriptor)
        //        + " ExtendedFlags=" + TRANS2QUIK_ORDER_EXTENDED_FLAGS(nOrderDescriptor)
        //        + " MinQty=" + TRANS2QUIK_ORDER_MIN_QTY(nOrderDescriptor)
        //        + " ExecType=" + TRANS2QUIK_ORDER_EXEC_TYPE(nOrderDescriptor)
        //        + " AwgPrice=" + TRANS2QUIK_ORDER_AWG_PRICE(nOrderDescriptor)
        //        ;

        //    try
        //    {
        //        strString = ""
        //            + " USERID=" + TRANS2QUIK_ORDER_USERID(nOrderDescriptor)
        //            + " Account=" + TRANS2QUIK_ORDER_ACCOUNT(nOrderDescriptor)
        //            + " Brokerref=" + TRANS2QUIK_ORDER_BROKERREF(nOrderDescriptor)
        //            + " ClientCode=" + TRANS2QUIK_ORDER_CLIENT_CODE(nOrderDescriptor)
        //            + " Firmid=" + TRANS2QUIK_ORDER_FIRMID(nOrderDescriptor)
        //            ;

        //        strProtocol13 +=
        //                " RejectReason=" + TRANS2QUIK_ORDER_REJECT_REASON(nOrderDescriptor)
        //            ;
        //    }
        //    catch (AccessViolationException e)
        //    {
        //        using (var errorFile = new StreamWriter(@"errors.log"))
        //        {
        //            errorFile.WriteLine(e.ToString());
        //        }
        //    }

        //    try
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(
        //                @"orders.log",
        //                true,
        //                System.Text.Encoding.GetEncoding(1251)))
        //        {
        //            var mes = mainString + addString + strString + strProtocol13;
        //            //Console.WriteLine("ORDER_CALLBACK: " + mes);
        //            file.WriteLine(mes);
        //            file.Close();
        //            if (nMode == 2)
        //                Console.WriteLine("Orders end");
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}
        #endregion

        #region trades

        #region time_const
        const Int32 TRADE_QUIKDATE = 0;
        const Int32 TRADE_QUIKTIME = 1;
        const Int32 TRADE_MICROSEC = 2;
        #endregion

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_SUBSCRIBE_TRADES",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 subscribe_trades(
                [MarshalAs(UnmanagedType.LPStr)] string class_code,
                [MarshalAs(UnmanagedType.LPStr)] string sec_code);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_UNSUBSCRIBE_TRADES",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ubsubscribe_trades();

        public delegate void TRADE_STATUS_CALLBACK(
                Int32 nMode,
                UInt64 nNumber,
                UInt64 nOrderNumber,
                [MarshalAs(UnmanagedType.LPStr)] string ClassCode,
                [MarshalAs(UnmanagedType.LPStr)] string SecCode,
                Double dPrice,
                Int64 nQty,
                Double dValue,
                Int32 nIsSell,
                IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_START_TRADES",
            CallingConvention = CallingConvention.StdCall)]
        static extern Int32 START_TRADES_UNMGR(
            TRADE_STATUS_CALLBACK_UNMGR pfTradeStatusCallback);

        #region trade_descriptor_functions
        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_DATE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_DATE(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_SETTLE_DATE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_SETTLE_DATE(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_TIME(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_IS_MARGINAL",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_IS_MARGINAL(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_ACCRUED_INT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_ACCRUED_INT(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_YIELD",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_YIELD(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_TS_COMMISSION",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_TS_COMMISSION(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_CLEARING_CENTER_COMMISSION",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_CLEARING_CENTER_COMMISSION(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_EXCHANGE_COMMISSION",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_EXCHANGE_COMMISSION(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_TRADING_SYSTEM_COMMISSION",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_TRADING_SYSTEM_COMMISSION(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_PRICE2",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_PRICE2(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_REPO_RATE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_REPO_RATE(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_REPO_VALUE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_REPO_VALUE(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_REPO2_VALUE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_REPO2_VALUE(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_ACCRUED_INT2",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_ACCRUED_INT2(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_REPO_TERM",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_REPO_TERM(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_START_DISCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_START_DISCOUNT(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_LOWER_DISCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_LOWER_DISCOUNT(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_UPPER_DISCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_UPPER_DISCOUNT(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_BLOCK_SECURITIES",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_BLOCK_SECURITIES(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_PERIOD",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_PERIOD(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_DATE_TIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_DATE_TIME(
            IntPtr pTradeDescriptor
            , Int32 nTimeType);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_FILETIME",
            CallingConvention = CallingConvention.StdCall)]
        public static extern System.Runtime.InteropServices.ComTypes.FILETIME
        TRANS2QUIK_TRADE_FILETIME(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_KIND",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_KIND(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_CURRENCY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_CURRENCY_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_CURRENCY(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_CURRENCY_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_SETTLE_CURRENCY",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_SETTLE_CURRENCY_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_SETTLE_CURRENCY(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_SETTLE_CURRENCY_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_SETTLE_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_SETTLE_CODE_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_SETTLE_CODE(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_SETTLE_CODE_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_ACCOUNT",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_ACCOUNT_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_ACCOUNT(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_ACCOUNT_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_BROKERREF",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_BROKERREF_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_BROKERREF(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_BROKERREF_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_CLIENT_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_CLIENT_CODE_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_CLIENT_CODE(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_CLIENT_CODE_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_USERID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_USERID_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_USERID(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_USERID_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_FIRMID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_FIRMID_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_FIRMID(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_FIRMID_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_PARTNER_FIRMID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_PARTNER_FIRMID_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_PARTNER_FIRMID(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_PARTNER_FIRMID_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_EXCHANGE_CODE",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_EXCHANGE_CODE_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_EXCHANGE_CODE(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_EXCHANGE_CODE_IMPL(pTradeDescriptor));
        }

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_STATION_ID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TRANS2QUIK_TRADE_STATION_ID_IMPL(IntPtr pTradeDescriptor);

        public static string TRANS2QUIK_TRADE_STATION_ID(IntPtr pTradeDescriptor)
        {
            return Marshal.PtrToStringAnsi(TRANS2QUIK_TRADE_STATION_ID_IMPL(pTradeDescriptor));
        }

        #region Protocol_1_3

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_BROKER_COMMISSION",
            CallingConvention = CallingConvention.StdCall)]
        public static extern double TRANS2QUIK_TRADE_BROKER_COMMISSION(IntPtr pTradeDescriptor);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_TRADE_TRANSID",
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 TRANS2QUIK_TRADE_TRANSID(IntPtr pTradeDescriptor);

        #endregion // Protocol_1_3


        #endregion

        //public static void trade_status_callback_implementation(
        //        Int32 nMode
        //        , UInt64 nNumber
        //        , UInt64 nOrderNumber
        //        , string ClassCode
        //        , string SecCode
        //        , Double dPrice
        //        , Int64 nQty
        //        , Double dValue
        //        , Int32 nIsSell
        //        , IntPtr pTradeDescriptor)
        //{
        //    String mainString = "Mode=" + nMode + " TradeNum=" + nNumber + " OrderNum=" + nOrderNumber +
        //         " Class=" + ClassCode + " Sec=" + SecCode +
        //         " Price=" + dPrice + " Volume=" + nQty + " Value=" + dValue + " IsSell=" + nIsSell;

        //    String addString = " SettleDate=" + TRANS2QUIK_TRADE_SETTLE_DATE(pTradeDescriptor) +
        //        " TradeDate=" + TRANS2QUIK_TRADE_DATE(pTradeDescriptor) +
        //        " TradeTime=" + TRANS2QUIK_TRADE_TIME(pTradeDescriptor) +
        //        " TradeTimeMicroSec=" + TRANS2QUIK_TRADE_DATE_TIME(pTradeDescriptor, TRADE_MICROSEC) +
        //        " IsMarginal=" + TRANS2QUIK_TRADE_IS_MARGINAL(pTradeDescriptor) +
        //        " AccruedInt=" + TRANS2QUIK_TRADE_ACCRUED_INT(pTradeDescriptor) +
        //        " Yield=" + TRANS2QUIK_TRADE_YIELD(pTradeDescriptor) +
        //        " ClearingComm=" + TRANS2QUIK_TRADE_CLEARING_CENTER_COMMISSION(pTradeDescriptor) +
        //        " ExchangeComm=" + TRANS2QUIK_TRADE_EXCHANGE_COMMISSION(pTradeDescriptor) +
        //        " TSComm=" + TRANS2QUIK_TRADE_TS_COMMISSION(pTradeDescriptor) +
        //        " TradingSysComm=" + TRANS2QUIK_TRADE_TRADING_SYSTEM_COMMISSION(pTradeDescriptor) +
        //        " Price2=" + TRANS2QUIK_TRADE_PRICE2(pTradeDescriptor) +
        //        " RepoRate=" + TRANS2QUIK_TRADE_REPO_RATE(pTradeDescriptor) +
        //        " Repo2Value=" + TRANS2QUIK_TRADE_REPO2_VALUE(pTradeDescriptor) +
        //        " AccruedInt2=" + TRANS2QUIK_TRADE_ACCRUED_INT2(pTradeDescriptor) +
        //        " RepoTerm=" + TRANS2QUIK_TRADE_REPO_TERM(pTradeDescriptor) +
        //        " StartDisc=" + TRANS2QUIK_TRADE_START_DISCOUNT(pTradeDescriptor) +
        //        " LowerDisc=" + TRANS2QUIK_TRADE_LOWER_DISCOUNT(pTradeDescriptor) +
        //        " UpperDisc=" + TRANS2QUIK_TRADE_UPPER_DISCOUNT(pTradeDescriptor) +
        //        " BlockSec=" + TRANS2QUIK_TRADE_BLOCK_SECURITIES(pTradeDescriptor) +
        //        " Period=" + TRANS2QUIK_TRADE_PERIOD(pTradeDescriptor) +
        //        " TradeFileTime=" + DateTimeStr(TRANS2QUIK_TRADE_FILETIME(pTradeDescriptor)) +
        //        " Kind=" + TRANS2QUIK_TRADE_KIND(pTradeDescriptor);

        //    String strString = ""
        //        + "Currency=" + TRANS2QUIK_TRADE_CURRENCY(pTradeDescriptor)
        //        + " SettleCurrency=" + TRANS2QUIK_TRADE_SETTLE_CURRENCY(pTradeDescriptor)
        //        + " SettleCode=" + TRANS2QUIK_TRADE_SETTLE_CODE(pTradeDescriptor)
        //        + " Account=" + TRANS2QUIK_TRADE_ACCOUNT(pTradeDescriptor)
        //        + " Brokerref=" + TRANS2QUIK_TRADE_BROKERREF(pTradeDescriptor)
        //        + " Cliencode=" + TRANS2QUIK_TRADE_CLIENT_CODE(pTradeDescriptor)
        //        + " Userid=" + TRANS2QUIK_TRADE_USERID(pTradeDescriptor)
        //        + " Firmid=" + TRANS2QUIK_TRADE_FIRMID(pTradeDescriptor)
        //        + " PartnFirmid=" + TRANS2QUIK_TRADE_PARTNER_FIRMID(pTradeDescriptor)
        //        + " ExchangeCode=" + TRANS2QUIK_TRADE_EXCHANGE_CODE(pTradeDescriptor)
        //        + " StationId=" + TRANS2QUIK_TRADE_STATION_ID(pTradeDescriptor)
        //        ;

        //    String strProtocol13 = ""
        //        + " BrokerComm=" + TRANS2QUIK_TRADE_BROKER_COMMISSION(pTradeDescriptor)
        //        + " TradeTransId=" + TRANS2QUIK_TRADE_TRANSID(pTradeDescriptor)
        //        ;

        //    try
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(
        //            @"trades.log",
        //            true,
        //            System.Text.Encoding.GetEncoding(1251)))
        //        {
        //            file.WriteLine(mainString + addString + strString + strProtocol13);
        //            file.Close();
        //            if (nMode == 2)
        //                Console.WriteLine("Trades end");
        //        }
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}
        #endregion

        #region Делегаты

        //long TRANS2QUIK_API __stdcall TRANS2QUIK_SET_CONNECTION_STATUS_CALLBACK (
        //    TRANS2QUIK_CONNECTION_STATUS_CALLBACK pfConnectionStatusCallback, 
        //    long* pnExtendedErrorCode, 
        //    LPSTR lpstrErrorMessage, 
        //    DWORD dwErrorMessageSize);

        public delegate void CONNECTION_STATUS_CALLBACK(
            Int32 nConnectionEvent
            , UInt32 nExtendedErrorCode
            , byte[] lpstrInfoMessage);

        [DllImport(DLL_NAME, EntryPoint = "TRANS2QUIK_SET_CONNECTION_STATUS_CALLBACK",
        CallingConvention = CallingConvention.StdCall)]
        static extern Int32 SET_CONNECTION_STATUS_CALLBACK_UNMGR(
            CONNECTION_STATUS_CALLBACK_UNMGR pfConnectionStatusCallback,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize);

        public static Int32 SET_CONNECTION_STATUS_CALLBACK(
            CONNECTION_STATUS_CALLBACK pfConnectionStatusCallback,
            ref Int32 pnExtendedErrorCode,
            byte[] lpstrErrorMessage,
            UInt32 dwErrorMessageSize)
        {
            connection_status_callback = pfConnectionStatusCallback;
            return SET_CONNECTION_STATUS_CALLBACK_UNMGR(
              connection_status_callback_unmgr,
              ref pnExtendedErrorCode,
              lpstrErrorMessage,
              dwErrorMessageSize);
        }

        public static void connection_status_callback_Report(
            Int32 nConnectionEvent, UInt32 nExtendedErrorCode, byte[] lpstrInfoMessage)
        {
            Console.WriteLine("--------------------------------------------------" +
                "\n\t<<<<connection_status_callback_Report>>>>");
            Console.WriteLine("  nConnectionEvent:\t{0} {1}\n  nExtendedErrorCode:\t{2}\n  dwErrorMessageSize:\t{3}",
                nConnectionEvent, ResultToString(nConnectionEvent), (nExtendedErrorCode & 255), lpstrInfoMessage);

            Console.WriteLine("--------------------------------------------------\n");
        }
        #endregion

        // **********************************************************************
        public static Int32 START_ORDERS(
          ORDER_STATUS_CALLBACK pfnOrderStatusCallback)
        {
            order_status_callback = pfnOrderStatusCallback;
            return START_ORDERS_UNMGR(
              order_status_callback_unmgr);
        }

        // **********************************************************************
        public static Int32 START_TRADES(
          TRADE_STATUS_CALLBACK pfnTradeStatusCallback)
        {
            trade_status_callback = pfnTradeStatusCallback;
            return START_TRADES_UNMGR(
              trade_status_callback_unmgr);
        }

        #region Реализация функций-переходников обратных вызовов

        // **********************************************************************
        static void connection_status_callback_impl(
            Int32 nConnectionEvent
            , UInt32 nExtendedErrorCode
            , byte[] lpstrInfoMessage)
        {
            if (connection_status_callback != null)
                connection_status_callback(
                  nConnectionEvent,
                  nExtendedErrorCode,
                  lpstrInfoMessage);
        }

        // **********************************************************************
        static void transaction_reply_callback_impl(
            Int32 nTransactionResult,
            Int32 nTransactionExtendedErrorCode,
            Int32 nTransactionReplyCode,
            UInt32 dwTransId,
            UInt64 nOrderNum,
            [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage,
            IntPtr pTransactionReplyDescriptor)
        {
            if (transaction_reply_callback != null)
                transaction_reply_callback(
                  nTransactionResult,
                  nTransactionExtendedErrorCode,
                  nTransactionReplyCode,
                  dwTransId,
                  nOrderNum,
                  TransactionReplyMessage,
                  pTransactionReplyDescriptor);
        }

        // **********************************************************************
        static void order_status_callback_impl(
          Int32 nMode,
          Int32 dwTransID,
          UInt64 nOrderNum,
          string ClassCode,
          string SecCode,
          double dPrice,
          Int64 nBalance,
          double dValue,
          Int32 nIsSell,
          Int32 nStatus,
          IntPtr pOrderDescriptor)
        {
            if (order_status_callback != null)
                order_status_callback(
                  nMode,
                  dwTransID,
                  nOrderNum,
                  ClassCode,
                  SecCode,
                  dPrice,
                  nBalance,
                  dValue,
                  nIsSell,
                  nStatus,
                  pOrderDescriptor);
        }

        // **********************************************************************
        static void trade_status_callback_impl(
          Int32 nMode,
          UInt64 nNumber,
          UInt64 nOrderNumber,
          string ClassCode,
          string SecCode,
          double dPrice,
          Int64 nQty,
          double dValue,
          Int32 nIsSell,
          IntPtr pTradeDescriptor)
        {
            if (trade_status_callback != null)
                trade_status_callback(
                  nMode,
                  nNumber,
                  nOrderNumber,
                  ClassCode,
                  SecCode,
                  dPrice,
                  nQty,
                  dValue,
                  nIsSell,
                  pTradeDescriptor);
        }

        #endregion


        #region Приватные делегаты функций обратных вызовов

        // **********************************************************************
        // typedef void (__stdcall *TRANS2QUIK_CONNECTION_STATUS_CALLBACK) (
        //   long nConnectionEvent,
        //   long nExtendedErrorCode,
        //   LPCSTR lpcstrInfoMessage);

        delegate void CONNECTION_STATUS_CALLBACK_UNMGR(
            Int32 nConnectionEvent
            , UInt32 nExtendedErrorCode
            , byte[] lpstrInfoMessage);


        // **********************************************************************
        // typedef void (__stdcall *TRANS2QUIK_TRANSACTION_REPLY_CALLBACK) (
        //   long nTransactionResult,
        //   long nTransactionExtendedErrorCode,
        //   long nTransactionReplyCode,
        //   DWORD dwTransId,
        //   double dOrderNum,
        //   LPCSTR lpcstrTransactionReplyMessage);

        delegate void TRANSACTION_REPLY_CALLBACK_UNMGR(
            Int32 nTransactionResult,
            Int32 nTransactionExtendedErrorCode,
            Int32 nTransactionReplyCode,
            UInt32 dwTransId,
            UInt64 dOrderNum,
            [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage,
            IntPtr pTransReplyDescriptor);


        // **********************************************************************
        // typedef void (__stdcall *TRANS2QUIK_ORDER_STATUS_CALLBACK) (
        //   long nMode,
        //   DWORD dwTransID,
        //   double dNumber,
        //   LPCSTR ClassCode,
        //   LPCSTR SecCode,
        //   double dPrice,
        //   long nBalance,
        //   double dValue,
        //   long nIsSell,
        //   long nStatus,
        //   long nOrderDescriptor);

        delegate void ORDER_STATUS_CALLBACK_UNMGR(
          Int32 nMode,
          Int32 dwTransID,
          UInt64 nOrderNum,
          [MarshalAs(UnmanagedType.LPStr)] string ClassCode,
          [MarshalAs(UnmanagedType.LPStr)] string SecCode,
          double dPrice,
          Int64 nBalance,
          double dValue,
          Int32 nIsSell,
          Int32 nStatus,
          IntPtr pOrderDescriptor);


        // **********************************************************************
        // typedef void (__stdcall *TRANS2QUIK_TRADE_STATUS_CALLBACK) (
        //   long nMode,
        //   double dNumber,
        //   double dOrderNumber,
        //   LPCSTR ClassCode,
        //   LPCSTR SecCode,
        //   double dPrice,
        //   long nQty,
        //   double dValue,
        //   long nIsSell,
        //   long nTradeDescriptor);

        delegate void TRADE_STATUS_CALLBACK_UNMGR(
          Int32 nMode,
          UInt64 nNumber,
          UInt64 nOrderNumber,
          [MarshalAs(UnmanagedType.LPStr)] string ClassCode,
          [MarshalAs(UnmanagedType.LPStr)] string SecCode,
          double dPrice,
          Int64 nQty,
          double dValue,
          Int32 nIsSell,
          IntPtr pTradeDescriptor);

        #endregion

        #region Рабочие переменные и конструктор

        // **********************************************************************
        static CONNECTION_STATUS_CALLBACK connection_status_callback;
        static TRANSACTION_REPLY_CALLBACK transaction_reply_callback;
        static ORDER_STATUS_CALLBACK order_status_callback;
        static TRADE_STATUS_CALLBACK trade_status_callback;

        // **********************************************************************
        static CONNECTION_STATUS_CALLBACK_UNMGR connection_status_callback_unmgr;
        static TRANSACTION_REPLY_CALLBACK_UNMGR transaction_reply_callback_unmgr;
        static ORDER_STATUS_CALLBACK_UNMGR order_status_callback_unmgr;
        static TRADE_STATUS_CALLBACK_UNMGR trade_status_callback_unmgr;

        // **********************************************************************
        static GCHandle gc_connection_status;
        static GCHandle gc_transaction_reply;
        static GCHandle gc_order_status;
        static GCHandle gc_trade_status;

        // **********************************************************************
        static Trans2Quik()
        {
            connection_status_callback_unmgr = new Trans2Quik.CONNECTION_STATUS_CALLBACK_UNMGR(connection_status_callback_impl);
            transaction_reply_callback_unmgr = new Trans2Quik.TRANSACTION_REPLY_CALLBACK_UNMGR(transaction_reply_callback_impl);
            order_status_callback_unmgr = new Trans2Quik.ORDER_STATUS_CALLBACK_UNMGR(order_status_callback_impl);
            trade_status_callback_unmgr = new Trans2Quik.TRADE_STATUS_CALLBACK_UNMGR(trade_status_callback_impl);

            gc_connection_status = GCHandle.Alloc(connection_status_callback_unmgr);
            gc_transaction_reply = GCHandle.Alloc(transaction_reply_callback_unmgr);
            gc_order_status = GCHandle.Alloc(order_status_callback_unmgr);
            gc_trade_status = GCHandle.Alloc(trade_status_callback_unmgr);
        }

        #endregion
        const string PATH_2_QUIK = @"C:\utils\QUIK_VTB24";
    }

}
