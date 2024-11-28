
using System;
using System.Collections.Generic;

namespace OSHFT_Q_R.Market
{
    // ************************************************************************

    delegate void StatusUpdateHandler(string text);

    delegate void StockHandler(Quote[] quotes, Spread spread);
    delegate void TickHandler(Tick tick);
    delegate void SettingsHandler(Setting settings);
    delegate void TradesHandler(Trade trade);
    delegate void PutOrdersHandler(PutOrder putOrder);

    // ************************************************************************

    interface IConnector
    {
        string Name { get; }
        void Setup();
    }

    // ************************************************************************

    interface IStockProvider : IConnector
    {
        event StatusUpdateHandler Connected;
        event StatusUpdateHandler Disconnected;
        event StatusUpdateHandler Broken;

        event StockHandler StockHandler;

        void Subscribe(string symbol);
        void Unsubscribe();
    }

    // ************************************************************************

    interface ITicksProvider : IConnector
    {
        event StatusUpdateHandler Connected;
        event StatusUpdateHandler Disconnected;
        event StatusUpdateHandler Broken;

        event TickHandler TickHandler;

        void Subscribe();
        void Unsubscribe();
    }

    // ************************************************************************
    interface ISettingsProvider : IConnector
    {
        event StatusUpdateHandler Connected;
        event StatusUpdateHandler Disconnected;
        event StatusUpdateHandler Broken;

        event SettingsHandler SettingsHandler;

        void Subscribe();
        void Unsubscribe();
    }
    // ************************************************************************

    interface ITradesProvider : IConnector
    {
        event StatusUpdateHandler Connected;
        event StatusUpdateHandler Disconnected;
        event StatusUpdateHandler Broken;

        event TradesHandler TradesHandler;

        void Subscribe();
        void Unsubscribe();
    }
    // ************************************************************************

    interface IPutOrdersProvider : IConnector
    {
        event StatusUpdateHandler Connected;
        event StatusUpdateHandler Disconnected;
        event StatusUpdateHandler Broken;

        event PutOrdersHandler PutOrdersHandler;

        void Subscribe();
        void Unsubscribe();
    }

    interface ISecListProvider : IConnector
    {
        void GetSecList(Action<SecList> callback);
    }

    // ************************************************************************
}
