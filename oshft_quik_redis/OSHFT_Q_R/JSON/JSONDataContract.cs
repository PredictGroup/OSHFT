using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace OSHFT_Q_R
{
    [DataContract]
    public class JSONQuotes
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "Quotes", Order = 1)]
        public List<JSONQuote> QuotesList { get; set; }

        public JSONQuotes()
        {
            QuotesList = new List<JSONQuote>();
        }

        [DataMember(Name = "DateTime", Order = 2)]
        public string DateTime { get; set; }

        [DataMember(Name = "Version", Order = 3)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONQuote
    {
        [DataMember(Name = "Price", Order = 0)]
        public double Price { get; set; }

        [DataMember(Name = "Qty", Order = 1)]
        public int Volume { get; set; }

        [DataMember(Name = "QuoteType", Order = 2)]
        public QuoteType Type { get; set; }

        [DataMember(Name = "Version", Order = 3)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONSpread
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "Ask", Order = 1)]
        public double Ask { get; set; }

        [DataMember(Name = "Bid", Order = 2)]
        public double Bid { get; set; }

        [DataMember(Name = "DateTime", Order = 3)]
        public string DateTime { get; set; }

        [DataMember(Name = "Version", Order = 4)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONTick
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "Price", Order = 1)]
        public double Price { get; set; }

        [DataMember(Name = "Qty", Order = 2)]
        public int Volume { get; set; }

        [DataMember(Name = "TradeType", Order = 3)]
        public TradeOp Op { get; set; }

        [DataMember(Name = "DateTime", Order = 4)]
        public string DateTime { get; set; }

        [DataMember(Name = "Version", Order = 5)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONSetting
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "SymbolClass", Order = 1)]
        public string ClassCode { get; set; }

        [IgnoreDataMember]
        public string ClassName { get; set; }

        [DataMember(Name = "OptionBase", Order = 2)]
        public string OptionBase { get; set; }

        [DataMember(Name = "MatDate", Order = 3)]
        public string MatDate { get; set; }

        [DataMember(Name = "DaysToMatDate", Order = 4)]
        public int DaysToMatDate { get; set; }

        [DataMember(Name = "NumBids", Order = 5)]
        public int Numbids { get; set; }

        [DataMember(Name = "NumOffers", Order = 6)]
        public int Numoffers { get; set; }

        [DataMember(Name = "BidDeptht", Order = 7)]
        public int Biddeptht { get; set; }

        [DataMember(Name = "OfferDeptht", Order = 8)]
        public int Offerdeptht { get; set; }

        [DataMember(Name = "VolumeToday", Order = 9)]
        public int Voltoday { get; set; }

        [DataMember(Name = "ValueToday", Order = 10)]
        public double Valtoday { get; set; }

        [DataMember(Name = "NumTrades", Order = 11)]
        public int Numtrades { get; set; }

        [DataMember(Name = "NumContracts", Order = 12)]
        public int Numcontracts { get; set; }

        [DataMember(Name = "SellDeposit", Order = 13)]
        public double Selldepo { get; set; }

        [DataMember(Name = "BuyDeposit", Order = 14)]
        public double Buydepo { get; set; }

        [DataMember(Name = "DateTime", Order = 15)]
        public string DateTime { get; set; }

        [DataMember(Name = "Strike", Order = 16)]
        public double Strike { get; set; }

        [DataMember(Name = "OptionType", Order = 17)]
        public string OptionType { get; set; }

        [DataMember(Name = "Volatility", Order = 18)]
        public double Volatility { get; set; }

        [DataMember(Name = "Version", Order = 19)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONTrade
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "TradeNum", Order = 1)]
        public double TradeNum;

        [DataMember(Name = "OrderNum", Order = 2)]
        public double OrderNum;

        [DataMember(Name = "Price", Order = 3)]
        public double Price { get; set; }

        [DataMember(Name = "Qty", Order = 4)]
        public int Volume { get; set; }

        [DataMember(Name = "TradeType", Order = 5)]
        public TradeOp Op { get; set; }

        [DataMember(Name = "DateTime", Order = 6)]
        public string DateTime { get; set; }

        [DataMember(Name = "Version", Order = 7)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONPutOrder // orders table to Redis
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "OrderNum", Order = 1)]
        public double OrderNum;

        [DataMember(Name = "Price", Order = 2)]
        public double Price { get; set; }

        [DataMember(Name = "Qty", Order = 3)]
        public int Volume { get; set; }

        [DataMember(Name = "Balance", Order = 4)]
        public int Balance { get; set; }

        [DataMember(Name = "OrderValue", Order = 5)]
        public double Value { get; set; }

        [DataMember(Name = "TradeType", Order = 6)]
        public TradeOp Op { get; set; }

        [DataMember(Name = "Status", Order = 7)]
        public OrderStatus Status { get; set; }

        [DataMember(Name = "DateTime", Order = 8)]
        public string DateTime { get; set; }

        [DataMember(Name = "WithdrawDateTime", Order = 9)]
        public string WithdrawDateTime { get; set; }

        [DataMember(Name = "Version", Order = 10)]
        public string ContractVersion { get; set; }
    }

    [DataContract]
    public class JSONOwnOrder // order to market
    {
        [DataMember(Name = "Symbol", Order = 0)]
        public string Symbol { get; set; }

        [DataMember(Name = "Price", Order = 1)]
        public double Price { get; set; }

        [DataMember(Name = "Qty", Order = 2)]
        public int Volume { get; set; }

        [DataMember(Name = "TradeType", Order = 3)]
        public TradeOp Op { get; set; }

        [DataMember(Name = "DateTime", Order = 4)]
        public DateTime DateTime { get; set; }

        [DataMember(Name = "OrderID", Order = 5)]
        public long OrderID { get; set; } // для отмены заявки по номеру

        [DataMember(Name = "Version", Order = 6)]
        public string ContractVersion { get; set; }
    }


}
