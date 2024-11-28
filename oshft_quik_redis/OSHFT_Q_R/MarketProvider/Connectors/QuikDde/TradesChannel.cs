
using System;
using System.Globalization;

using OSHFT_Q_R;
using OSHFT_Q_R.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
    sealed class TradesChannel : DdeChannel
    {
        // **********************************************************************

        public override string Topic { get { return "deals"; } }

        public event TradesHandler TradesHandler;

        // **********************************************************************
        const string cnTradeNum = "TRADENUM";
        const string cnOrderNum = "ORDERNUM";
        const string cnDate = "TRADEDATE";
        const string cnTime = "TRADETIME";
        const string cnSecCode = "SECCODE";
        const string cnClassCode = "CLASSCODE";
        const string cnPrice = "PRICE";
        const string cnQuantity = "QTY";
        const string cnOperation = "BUYSELL";

        const string strBuyOp = "BUY";
        const string strSellOp = "SELL";

        // **********************************************************************

        DateTimeFormatInfo dtFmtInfo;
        string dtFmt;

        // **********************************************************************

        int cDate;
        int cTime;
        int cPrice;
        int cQuantity;
        int cOp;
        int cSecCode;
        int cClassCode;
        int cTradeNum;
        int cOrderNum;

        bool columnsUnknown = true;

        // **********************************************************************

        public TradesChannel()
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
                cDate = -1;
                cTime = -1;
                cPrice = -1;
                cQuantity = -1;
                cOp = -1;
                cSecCode = -1;
                cClassCode = -1;
                cTradeNum = -1;
                cOrderNum = -1;

                for (int col = 0; col < xt.Columns; col++)
                {
                    xt.ReadValue();

                    if (xt.ValueType == XlTable.BlockType.String)
                        switch (xt.StringValue)
                        {
                            case cnDate:
                                cDate = col;
                                break;
                            case cnTime:
                                cTime = col;
                                break;
                            case cnPrice:
                                cPrice = col;
                                break;
                            case cnQuantity:
                                cQuantity = col;
                                break;
                            case cnOperation:
                                cOp = col;
                                break;
                            case cnSecCode:
                                cSecCode = col;
                                break;
                            case cnClassCode:
                                cClassCode = col;
                                break;
                            case cnTradeNum:
                                cTradeNum = col;
                                break;
                            case cnOrderNum:
                                cOrderNum = col;
                                break;
                        }
                }

                if (cDate < 0
                  || cTime < 0
                  || cPrice < 0
                  || cQuantity < 0
                  || cOp < 0
                  || cSecCode < 0
                  || cClassCode < 0
                  || cTradeNum < 0
                  || cOrderNum < 0
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

                string tradeNum = "";
                string orderNum = "";

                string secCode = string.Empty;
                string classCode = string.Empty;

                string date = string.Empty;
                string time = string.Empty;

                Trade t = new Trade();

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
                    else if (col == cOp)
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
                    else if (col == cClassCode)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            classCode = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cTradeNum)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                        {
                            tradeNum = xt.StringValue;
                            t.TradeNum = tradeNum;
                        }
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
                }



                if (rowCorrect)
                {
                    // ----------------------------------------------------------

                    if (!DateTime.TryParseExact(date + time, dtFmt, dtFmtInfo,
                      DateTimeStyles.None, out t.DateTime))
                    {
                        SetError("не распознан формат даты или времени");
                        return;
                    }

                    // ----------------------------------------------------------

                    if (TradesHandler != null)
                        TradesHandler(t); // Отсюда отправляем данные получателю
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
