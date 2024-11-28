
using System;
using System.Globalization;

using OSHFT_Q_R;
using OSHFT_Q_R.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
    sealed class SettingsChannel : DdeChannel
    {
        // **********************************************************************

        public override string Topic { get { return "settings"; } }

        public event SettingsHandler SettingsHandler;

        // **********************************************************************

        int cCode;
        int cClassName;
        int cClassCode;
        int cOptionBase;
        int cTrdDateCode;
        int cMatDate;
        int cDaysToMatDate;
        int cTime;
        int cNumbids;
        int cNumoffers;
        int cBiddeptht;
        int cOfferdeptht;
        int cVoltoday;
        int cValtoday;
        int cNumtrades;
        int cNumcontracts;
        int cSelldepo;
        int cBuydepo;
        int cChangetime;
        int cOptionStrike;
        int cOptionType;
        int cVolatility;

        // **********************************************************************

        bool columnsUnknown = true;

        // **********************************************************************

        const string cnCode = "CODE"; // код бумаги
        const string cnClassName = "CLASSNAME"; // класс бумаги
        const string cnClassCode = "CLASS_CODE"; // код класса
        const string cnOptionBase = "OPTIONBASE"; // +1 базовый актив опциона
        const string cnTrdDateCode = "TRADE_DATE_CODE"; // дата торгов
        const string cnMatDate = "MAT_DATE"; // +1 дата погашения
        const string cnDaysToMatDate = "DAYS_TO_MAT_DATE"; // дней до погашения
        const string cnTime = "TIME"; // время последней сделки
        const string cnNumbids = "NUMBIDS"; // количество заявкок на покупку
        const string cnNumoffers = "NUMOFFERS"; // количество заявок на продажу
        const string cnBiddeptht = "BIDDEPTHT"; // суммарный спрос контрактов
        const string cnOfferdeptht = "OFFERDEPTHT"; // суммарное предложение контрактов
        const string cnVoltoday = "VOLTODAY"; // количество во всех сделках 
        const string cnValtoday = "VALTODAY"; // текущий оборот в деньгах
        const string cnNumtrades = "NUMTRADES"; // количество сделок за сегодня
        const string cnNumcontracts = "NUMCONTRACTS"; // количество открытых позиций (Открытый Интерес)
        const string cnSelldepo = "SELLDEPO"; // ГО продавца
        const string cnBuydepo = "BUYDEPO"; // ГО покупателя
        const string cnChangetime = "CHANGETIME"; // Время последнего изменения
        const string cnOptionStrike = "STRIKE"; // +1 цена страйк
        const string cnOptionType = "OPTIONTYPE"; // +1 тип опциона CALL PUT
        const string cnVolatility = "VOLATILITY"; // +1 волатильность опциона

        // **********************************************************************

        DateTimeFormatInfo dtFmtInfo;
        string dtFmt;

        // **********************************************************************

        public SettingsChannel()
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

                // -------------------проверка столбцов-----------------------------------
                cCode = -1; cClassName = -1; cClassCode = -1; cOptionBase = -1; cTrdDateCode = -1; cMatDate = -1;
                cDaysToMatDate = -1; cTime = -1; cNumbids = -1; cNumoffers = -1; cBiddeptht = -1; cOfferdeptht = -1;
                cVoltoday = -1; cValtoday = -1; cNumtrades = -1; cNumcontracts = -1; cSelldepo = -1; cBuydepo = -1;
                cChangetime = -1; cOptionStrike = -1; cOptionType = -1; cVolatility = -1;


                for (int col = 0; col < xt.Columns; col++)
                {
                    xt.ReadValue();

                    if (xt.ValueType == XlTable.BlockType.String)
                        switch (xt.StringValue)
                        {
                            case cnCode:
                                cCode = col;
                                break;
                            case cnClassName:
                                cClassName = col;
                                break;
                            case cnClassCode:
                                cClassCode = col;
                                break;
                            case cnOptionBase:
                                cOptionBase = col;
                                break;
                            case cnTrdDateCode:
                                cTrdDateCode = col;
                                break;
                            case cnMatDate:
                                cMatDate = col;
                                break;
                            case cnDaysToMatDate:
                                cDaysToMatDate = col;
                                break;
                            case cnTime:
                                cTime = col;
                                break;
                            case cnNumbids:
                                cNumbids = col;
                                break;
                            case cnNumoffers:
                                cNumoffers = col;
                                break;
                            case cnBiddeptht:
                                cBiddeptht = col;
                                break;
                            case cnOfferdeptht:
                                cOfferdeptht = col;
                                break;
                            case cnVoltoday:
                                cVoltoday = col;
                                break;
                            case cnValtoday:
                                cValtoday = col;
                                break;
                            case cnNumtrades:
                                cNumtrades = col;
                                break;
                            case cnNumcontracts:
                                cNumcontracts = col;
                                break;
                            case cnSelldepo:
                                cSelldepo = col;
                                break;
                            case cnBuydepo:
                                cBuydepo = col;
                                break;
                            case cnChangetime:
                                cChangetime = col;
                                break;
                            case cnOptionStrike:
                                cOptionStrike = col;
                                break;
                            case cnOptionType:
                                cOptionType = col;
                                break;
                            case cnVolatility:
                                cVolatility = col;
                                break;
                        }
                }

                //if (cCode < 0 || cClassName < 0 || cClassCode < 0 || cOptionBase < 0 || cTrdDateCode < 0 || cMatDate < 0 ||
                //    cDaysToMatDate < 0 || cTime < 0 || cNumbids < 0 || cNumoffers < 0 || cBiddeptht < 0 || cOfferdeptht < 0 ||
                //    cVoltoday < 0 || cValtoday < 0 || cNumtrades < 0 || cNumcontracts < 0 || cSelldepo < 0 || cBuydepo < 0 ||
                //    cChangetime < 0 || cOptionStrike < 0 || cOptionType < 0 || cVolatility < 0) // убрали поле cOptionType
                if (cCode < 0 || cClassName < 0 || cClassCode < 0 || cOptionBase < 0 || cTrdDateCode < 0 || cMatDate < 0 ||
                        cDaysToMatDate < 0 || cTime < 0 || cNumbids < 0 || cNumoffers < 0 || cBiddeptht < 0 || cOfferdeptht < 0 ||
                        cVoltoday < 0 || cValtoday < 0 || cNumtrades < 0 || cNumcontracts < 0 || cSelldepo < 0 || cBuydepo < 0 ||
                        cChangetime < 0 || cOptionStrike < 0 || cVolatility < 0)
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

                string secCode = string.Empty;
                string classCode = string.Empty;
                string className = string.Empty;

                string date = string.Empty;
                string time = string.Empty;

                string matDate = string.Empty;

                Setting s = new Setting();



                // ----------------------------------------------------------

                for (int col = 0; col < xt.Columns; col++)
                {
                    xt.ReadValue();

                    if (col == cCode)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            s.SecCode = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cClassName)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            s.ClassName = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cClassCode)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            s.ClassCode = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cOptionBase)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            s.OptionBase = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cTrdDateCode)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            date = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cChangetime) // cTime
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            time = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cMatDate)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            matDate = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cDaysToMatDate)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.DaysToMatDate = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cNumbids)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Numbids = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cNumoffers)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Numoffers = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cBiddeptht)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Biddeptht = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cOfferdeptht)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Offerdeptht = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cVoltoday)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Voltoday = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cValtoday)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Valtoday = xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cNumtrades)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Numtrades = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cNumcontracts)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Numcontracts = (int)xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cSelldepo)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Selldepo = xt.FloatValue;
                        else
                            if (xt.ValueType == XlTable.BlockType.String)
                                s.Strike = 0;
                            else
                                rowCorrect = false;
                    }
                    else if (col == cBuydepo)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Buydepo = xt.FloatValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cOptionStrike)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Strike = xt.FloatValue;
                        else
                            if (xt.ValueType == XlTable.BlockType.String)
                                s.Strike = 0;
                            else
                                rowCorrect = false;
                    }
                    else if (col == cOptionType)
                    {
                        if (xt.ValueType == XlTable.BlockType.String)
                            s.OptionType = xt.StringValue;
                        else
                            rowCorrect = false;
                    }
                    else if (col == cVolatility)
                    {
                        if (xt.ValueType == XlTable.BlockType.Float)
                            s.Volatility = xt.FloatValue;
                        else
                            if (xt.ValueType == XlTable.BlockType.String)
                                s.Volatility = 0;
                            else
                                rowCorrect = false;
                    }
                }

                // ----------------------------------------------------------



                if (rowCorrect)
                {
                    // + 22.08.2013 GAS проверка даты погашения
                    DateTime sMatDate = s.MatDate;
                    if (DateTime.TryParseExact(matDate, "dd.MM.yyyy", dtFmtInfo,
                            DateTimeStyles.None, out sMatDate))
                    {
                        s.MatDate = sMatDate;
                    }
                    else
                    {
                        SetError("не распознан формат даты или времени");
                        return;
                    }
                    // - 22.08.2013 GAS

                    DateTime sDateTime = s.DateTime;
                    if (DateTime.TryParseExact(date + time, dtFmt, dtFmtInfo,
                      DateTimeStyles.None, out sDateTime))
                    {
                        s.DateTime = sDateTime;
                        if (SettingsHandler != null)
                            SettingsHandler(s);
                    }
                    else
                    {
                        SetError("не распознан формат даты или времени");
                        return;
                    }
                }
                else
                {
                    SetError("ошибка в данных");
                    return;
                }

                // ----------------------------------------------------------
            }
        }

    }
}
