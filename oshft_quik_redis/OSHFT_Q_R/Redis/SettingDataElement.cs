using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSHFT_Q_R
{
    [Serializable()]
    public struct SettingDataElement
    {
        public string SecCode; // код бумаги
        public string ClassCode; // код класса
        public string ClassName; // класс бумаги
        public string OptionBase; // +1 базовый актив опциона
        public DateTime? MatDate; // +1 дата погашения
        public int DaysToMatDate; // дней до погашения

        public int NumbidsOpen; // количество заявкок на покупку
        public int NumbidsHigh; // количество заявкок на покупку
        public int NumbidsLow; // количество заявкок на покупку
        public int NumbidsClose; // количество заявкок на покупку

        public int NumoffersOpen; // количество заявок на продажу
        public int NumoffersHigh; // количество заявок на продажу
        public int NumoffersLow; // количество заявок на продажу
        public int NumoffersClose; // количество заявок на продажу

        public int BiddepthtOpen; // суммарный спрос контрактов
        public int BiddepthtHigh; // суммарный спрос контрактов
        public int BiddepthtLow; // суммарный спрос контрактов
        public int BiddepthtClose; // суммарный спрос контрактов

        public int OfferdepthtOpen; // суммарное предложение контрактов
        public int OfferdepthtHigh; // суммарное предложение контрактов
        public int OfferdepthtLow; // суммарное предложение контрактов
        public int OfferdepthtClose; // суммарное предложение контрактов

        public int Voltoday; // количество во всех сделках 
        public double Valtoday; // текущий оборот в деньгах
        public int Numtrades; // количество сделок за сегодня

        public int NumcontractsOpen; // количество открытых позиций (Открытый Интерес)
        public int NumcontractsHigh; // количество открытых позиций (Открытый Интерес)
        public int NumcontractsLow; // количество открытых позиций (Открытый Интерес)
        public int NumcontractsClose; // количество открытых позиций (Открытый Интерес)

        public double Selldepo; // ГО продавца
        public double Buydepo; // ГО покупателя
        public DateTime DateTime;
        public double Strike; // +1 цена страйк
        public string OptionType; // +1 тип опциона CALL PUT
        public double Volatility; // +1 волатильность опциона

        // ------------------------------------------------------------

        public SettingDataElement(string secCode, DateTime dt)
        {
            this.DateTime = dt;

            this.SecCode = secCode; // код бумаги
            this.ClassCode = ""; // код класса
            this.ClassName = ""; // класс бумаги
            this.OptionBase = ""; // +1 базовый актив опциона
            this.MatDate = null; // +1 дата погашения
            this.DaysToMatDate = 0; // дней до погашения

            this.NumbidsOpen = 0; // количество заявкок на покупку
            this.NumbidsHigh = 0; // количество заявкок на покупку
            this.NumbidsLow = 0; // количество заявкок на покупку
            this.NumbidsClose = 0; // количество заявкок на покупку

            this.NumoffersOpen = 0; // количество заявок на продажу
            this.NumoffersHigh = 0; // количество заявок на продажу
            this.NumoffersLow = 0; // количество заявок на продажу
            this.NumoffersClose = 0; // количество заявок на продажу

            this.BiddepthtOpen = 0; // суммарный спрос контрактов
            this.BiddepthtHigh = 0; // суммарный спрос контрактов
            this.BiddepthtLow = 0; // суммарный спрос контрактов
            this.BiddepthtClose = 0; // суммарный спрос контрактов

            this.OfferdepthtOpen = 0; // суммарное предложение контрактов
            this.OfferdepthtHigh = 0; // суммарное предложение контрактов
            this.OfferdepthtLow = 0; // суммарное предложение контрактов
            this.OfferdepthtClose = 0; // суммарное предложение контрактов

            this.Voltoday = 0; // количество во всех сделках 
            this.Valtoday = 0; // текущий оборот в деньгах
            this.Numtrades = 0; // количество сделок за сегодня

            this.NumcontractsOpen = 0; // количество открытых позиций (Открытый Интерес)
            this.NumcontractsHigh = 0; // количество открытых позиций (Открытый Интерес)
            this.NumcontractsLow = 0; // количество открытых позиций (Открытый Интерес)
            this.NumcontractsClose = 0; // количество открытых позиций (Открытый Интерес)

            this.Selldepo = 0; // ГО продавца
            this.Buydepo = 0; // ГО покупателя
            this.Strike = 0; // +1 цена страйк
            this.OptionType = ""; // +1 тип опциона CALL PUT
            this.Volatility = 0; // +1 волатильность опциона
        }
    }
}
