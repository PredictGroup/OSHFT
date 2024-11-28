using System;
using System.Globalization;

using OSHFT_Q_R;
using OSHFT_Q_R.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
    sealed class PutOrdersChannel : DdeChannel
    {
        // **********************************************************************

        public override string Topic { get { return "orders"; } }

        public event PutOrdersHandler PutOrdersHandler;

        // **********************************************************************
        const string cnOrderNum = "ORDERNUM";
        const string cnDate = "ORDERDATE";
        const string cnTime = "ORDERTIME";
        const string cnWithdrawDate = "WITHDRAW_DATE";
        const string cnWithdrawTime = "WITHDRAW_TIME";
        const string cnPeriod = "PERIOD";
        const string cnSecCode = "SECCODE";
        const string cnSecName = "SECNAME";
        const string cnOperation = "BUYSELL";
        const string cnAccount = "ACCOUNT";
        const string cnPrice = "PRICE";
        const string cnQuantity = "QTY";
        const string cnBalance = "BALANCE";
        const string cnValue = "VALUE";
        const string cnStatus = "STATUS";

        const string strBuyOp = "B";
        const string strSellOp = "S";

        // **********************************************************************

        DateTimeFormatInfo dtFmtInfo;
        string dtFmt;

        // **********************************************************************
        int cOrderNum;
        int cDate;
        int cTime;
        int cWithdrawDate;
        int cWithdrawTime;
        int cPeriod;
        int cSecCode;
        int cSecName;
        int cOperation;
        int cAccount;
        int cPrice;
        int cQuantity;
        int cBalance;
        int cValue;
        int cStatus;

        bool columnsUnknown = true;

        // **********************************************************************

        public PutOrdersChannel()
        {
            dtFmtInfo = DateTimeFormatInfo.CurrentInfo;
            dtFmt = dtFmtInfo.ShortDatePattern + dtFmtInfo.LongTimePattern;

            this.ConversationRemoved += () => { columnsUnknown = true; };
        }


        // **********************************************************************

        public override void ProcessTable(XlTable xt)
        {
            int row = 0;

            // ------------------------------------------------------------

            if (columnsUnknown)
            {
                cOrderNum = -1;
                cDate = -1;
                cTime = -1;
                cWithdrawDate = -1;
                cWithdrawTime = -1;
                cPeriod = -1;
                cSecCode = -1;
                cSecName = -1;
                cOperation = -1;
                cAccount = -1;
                cPrice = -1;
                cQuantity = -1;
                cBalance = -1;
                cValue = -1;
                cStatus = -1;

                for (int col = 0; col < xt.Columns; col++)
                {
                    xt.ReadValue();

                    if (xt.ValueType == XlTable.BlockType.String)
                        switch (xt.StringValue)
                        {
                            case cnOrderNum:
                                cOrderNum = col;
                                break;
                            case cnDate:
                                cDate = col;
                                break;
                            case cnTime:
                                cTime = col;
                                break;
                            case cnWithdrawDate:
                                cWithdrawDate = col;
                                break;
                            case cnWithdrawTime:
                                cWithdrawTime = col;
                                break;
                            case cnPeriod:
                                cPeriod = col;
                                break;
                            case cnSecCode:
                                cSecCode = col;
                                break;
                            case cnSecName:
                                cSecName = col;
                                break;
                            case cnOperation:
                                cOperation = col;
                                break;
                            case cnAccount:
                                cAccount = col;
                                break;
                            case cnPrice:
                                cPrice = col;
                                break;
                            case cnQuantity:
                                cQuantity = col;
                                break;
                            case cnBalance:
                                cBalance = col;
                                break;
                            case cnValue:
                                cValue = col;
                                break;
                            case cnStatus:
                                cStatus = col;
                                break;
                        }
                }

                if (cOrderNum < 0
                  || cDate < 0
                  || cTime < 0
                  || cWithdrawDate < 0
                  || cWithdrawTime < 0
                  || cPeriod < 0
                  || cSecCode < 0
                  || cSecName < 0
                  || cOperation < 0
                  || cAccount < 0
                  || cPrice < 0
                  || cQuantity < 0
                  || cBalance < 0
                  || cValue < 0
                  || cStatus < 0
                  )
                {
                    SetError("нет нужных столбцов");
                    return;
                }

                row++;
                columnsUnknown = false;
            }

            // ------------------------------------------------------------

            while (row++ < xt.Rows)
            {
                bool rowCorrect = true;

                string orderNum = "";

                string secCode = string.Empty;
                string secName = string.Empty;

                string period = string.Empty;
                string account = string.Empty;

                string date = string.Empty;
                string time = string.Empty;

                string withdraw_date = string.Empty;
                string withdraw_time = string.Empty;

                PutOrder t = new PutOrder();

                // ----------------------------------------------------------

                for (int col = 0; col < xt.Columns; col++)
                {
                    xt.ReadValue();

                    if (col == cDate)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            date = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cTime)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            time = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if(col == cWithdrawDate)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            withdraw_date = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cWithdrawTime)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            withdraw_time = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cOrderNum)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                        {
                            orderNum = xt.StringValue;
                            t.OrderNum = orderNum;
                        }
                        else
                            rowCorrect = false;
                    }
                    else if (col == cPeriod)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                        {
                            period = xt.StringValue;
                        }
                        else
                            rowCorrect = false;
                    }
                    else if (col == cSecCode)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                        {
                            secCode = xt.StringValue;
                            t.SecCode = secCode;
                        }
                        else
                            rowCorrect = false;
                    }
                    else if (col == cSecName)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            secName = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cOperation)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            switch (xt.StringValue)
                            {
                                case strBuyOp:
                                    t.Op = TradeOp.Buy;
                                    break;
                                case strSellOp:
                                    t.Op = TradeOp.Sell;
                                    break;
                            }
                        else
                            rowCorrect = false;
                    }
                    else if (col == cAccount)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                        {
                            account = xt.StringValue;
                        }
                        else
                            rowCorrect = false;
                    }
                    else if (col == cPrice)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            t.RawPrice = xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cQuantity)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            t.Quantity = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cBalance)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            t.Balance = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cValue)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            t.Value = xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cStatus)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            switch (xt.StringValue)
                            {
                                case "ACTIVE":
                                    t.Status = OrderStatus.Active;
                                    break;
                                case "KILLED":
                                    t.Status = OrderStatus.Killed;
                                    break;
                            }
                        else
                            rowCorrect = false;
                    }
                }



                if (rowCorrect)
                {
                    // ----------------------------------------------------------

                    if (!DateTime.TryParseExact(date + time, dtFmt, dtFmtInfo,
                      DateTimeStyles.None, out t.PutDateTime))
                    {
                        SetError("не распознан формат даты или времени");
                        return;
                    }
                    if (!DateTime.TryParseExact(withdraw_date + withdraw_time, dtFmt, dtFmtInfo,
                        DateTimeStyles.None, out t.WithdrawDateTime))
                    {
                        SetError("не распознан формат даты или времени");
                        return;
                    }
                    // ----------------------------------------------------------

                    if (PutOrdersHandler != null)
                        PutOrdersHandler(t); // Отсюда отправляем данные получателю
                }
                else
                {
                    SetError("ошибка в данных");
                    return;
                }

                // ----------------------------------------------------------
            }
        }

        // **********************************************************************


        // **********************************************************************
    }
}
