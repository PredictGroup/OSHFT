
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using OSHFT_Q_R;
using OSHFT_Q_R.Market;
using QuikDdeConnector.Internals;
using XlDde;

namespace QuikDdeConnector
{
    sealed class QuikDde : IStockProvider, ITicksProvider, ISettingsProvider, ITradesProvider, IPutOrdersProvider, ISecListProvider
    {
        // **********************************************************************

        StockChannel stockChannel = new StockChannel();
        TicksChannel ticksChannel = new TicksChannel();
        SettingsChannel settingsChannel = new SettingsChannel();
        TradesChannel tradesChannel = new TradesChannel();
        PutOrdersChannel putOrdersChannel = new PutOrdersChannel();

        bool stockActive;
        bool ticksActive;
        bool settingsActive;
        bool tradesActive;
        bool putOrdersActive;

        string service;
        XlDdeServer server;

        StatusUpdateHandler errorHandler;

        // **********************************************************************

        void CreateServer()
        {
            if (server == null)
                try
                {
                    server = new XlDdeServer(service);
                    server.Register();
                }
                catch (Exception e)
                {
                    if (errorHandler != null)
                        errorHandler.Invoke("Ошибка создания сервера DDE: " + e.Message);
                }
        }

        // **********************************************************************

        void DisposeServer()
        {
            if (server != null)
            {
                try
                {
                    server.Disconnect();
                    server.Dispose();
                }
                catch (Exception e)
                {
                    if (errorHandler != null)
                        errorHandler.Invoke("Ошибка удаления сервера DDE: " + e.Message);
                }

                server = null;

                stockActive = false;
                ticksActive = false;
                settingsActive = false;
                tradesActive = false;
                putOrdersActive = false;
            }
        }

        // **********************************************************************

        string IConnector.Name { get { return "Quik DDE"; } }

        // **********************************************************************

        void IConnector.Setup()
        {
            if (service != cfg.u.DdeServerName)
            {
                DisposeServer();
                service = cfg.u.DdeServerName;
            }
        }

        // **********************************************************************

        event StatusUpdateHandler IStockProvider.Connected
        {
            add { stockChannel.Connected += value; }
            remove { stockChannel.Connected -= value; }
        }

        event StatusUpdateHandler IStockProvider.Disconnected
        {
            add { stockChannel.Disconnected += value; }
            remove { stockChannel.Disconnected -= value; }
        }

        event StatusUpdateHandler IStockProvider.Broken
        {
            add { stockChannel.Broken += value; errorHandler += value; }
            remove { stockChannel.Broken -= value; errorHandler -= value; }
        }

        event StockHandler IStockProvider.StockHandler
        {
            add { stockChannel.StockHandler += value; }
            remove { stockChannel.StockHandler -= value; }
        }

        // **********************************************************************

        void IStockProvider.Subscribe(string symbol)
        {
            CreateServer();

            if (!stockActive && server != null)
            {
                stockActive = true;
                server.AddChannel(stockChannel);
            }
        }

        // **********************************************************************

        void IStockProvider.Unsubscribe()
        {
            if (stockActive && server != null)
            {
                stockActive = false;
                server.RmvChannel(stockChannel);

                if (!ticksActive && !settingsActive && !tradesActive && !putOrdersActive)
                    DisposeServer();
            }
        }

        // **********************************************************************

        event StatusUpdateHandler ITicksProvider.Connected
        {
            add { ticksChannel.Connected += value; }
            remove { ticksChannel.Connected -= value; }
        }

        event StatusUpdateHandler ITicksProvider.Disconnected
        {
            add { ticksChannel.Disconnected += value; }
            remove { ticksChannel.Disconnected -= value; }
        }

        event StatusUpdateHandler ITicksProvider.Broken
        {
            add { ticksChannel.Broken += value; errorHandler += value; }
            remove { ticksChannel.Broken -= value; errorHandler -= value; }
        }

        event TickHandler ITicksProvider.TickHandler
        {
            add { ticksChannel.TickHandler += value; }
            remove { ticksChannel.TickHandler -= value; }
        }

        // **********************************************************************

        void ITicksProvider.Subscribe()
        {
            CreateServer();

            if (!ticksActive && server != null)
            {
                ticksActive = true;
                server.AddChannel(ticksChannel);
            }
        }

        // **********************************************************************

        void ITicksProvider.Unsubscribe()
        {
            if (ticksActive && server != null)
            {
                ticksActive = false;
                server.RmvChannel(ticksChannel);

                if (!stockActive && !settingsActive && !tradesActive && !putOrdersActive)
                    DisposeServer();
            }
        }

        // **********************************************************************

        event StatusUpdateHandler ISettingsProvider.Connected
        {
            add { settingsChannel.Connected += value; }
            remove { settingsChannel.Connected -= value; }
        }

        event StatusUpdateHandler ISettingsProvider.Disconnected
        {
            add { settingsChannel.Disconnected += value; }
            remove { settingsChannel.Disconnected -= value; }
        }

        event StatusUpdateHandler ISettingsProvider.Broken
        {
            add { settingsChannel.Broken += value; errorHandler += value; }
            remove { settingsChannel.Broken -= value; errorHandler -= value; }
        }

        event SettingsHandler ISettingsProvider.SettingsHandler
        {
            add { settingsChannel.SettingsHandler += value; }
            remove { settingsChannel.SettingsHandler -= value; }
        }

        // **********************************************************************

        void ISettingsProvider.Subscribe()
        {
            CreateServer();

            if (!settingsActive && server != null)
            {
                settingsActive = true;
                server.AddChannel(settingsChannel);
            }
        }

        // **********************************************************************

        void ISettingsProvider.Unsubscribe()
        {
            if (settingsActive && server != null)
            {
                settingsActive = false;
                server.RmvChannel(settingsChannel);

                if (!stockActive && !ticksActive && !tradesActive && !putOrdersActive)
                    DisposeServer();
            }
        }

        // **********************************************************************
        // **********************************************************************

        event StatusUpdateHandler ITradesProvider.Connected
        {
            add { tradesChannel.Connected += value; }
            remove { tradesChannel.Connected -= value; }
        }

        event StatusUpdateHandler ITradesProvider.Disconnected
        {
            add { tradesChannel.Disconnected += value; }
            remove { tradesChannel.Disconnected -= value; }
        }

        event StatusUpdateHandler ITradesProvider.Broken
        {
            add { tradesChannel.Broken += value; errorHandler += value; }
            remove { tradesChannel.Broken -= value; errorHandler -= value; }
        }

        event TradesHandler ITradesProvider.TradesHandler
        {
            add { tradesChannel.TradesHandler += value; }
            remove { tradesChannel.TradesHandler -= value; }
        }

        // **********************************************************************

        void ITradesProvider.Subscribe()
        {
            CreateServer();

            if (!tradesActive && server != null)
            {
                tradesActive = true;
                server.AddChannel(tradesChannel);
            }
        }

        // **********************************************************************

        void ITradesProvider.Unsubscribe()
        {
            if (tradesActive && server != null)
            {
                tradesActive = false;
                server.RmvChannel(tradesChannel);

                if (!stockActive && !ticksActive && !settingsActive && !putOrdersActive)
                    DisposeServer();
            }
        }

        // **********************************************************************
        // **********************************************************************

        event StatusUpdateHandler IPutOrdersProvider.Connected
        {
            add { putOrdersChannel.Connected += value; }
            remove { putOrdersChannel.Connected -= value; }
        }

        event StatusUpdateHandler IPutOrdersProvider.Disconnected
        {
            add { putOrdersChannel.Disconnected += value; }
            remove { putOrdersChannel.Disconnected -= value; }
        }

        event StatusUpdateHandler IPutOrdersProvider.Broken
        {
            add { putOrdersChannel.Broken += value; errorHandler += value; }
            remove { putOrdersChannel.Broken -= value; errorHandler -= value; }
        }

        event PutOrdersHandler IPutOrdersProvider.PutOrdersHandler
        {
            add { putOrdersChannel.PutOrdersHandler += value; }
            remove { putOrdersChannel.PutOrdersHandler -= value; }
        }

        // **********************************************************************

        void IPutOrdersProvider.Subscribe()
        {
            CreateServer();

            if (!putOrdersActive && server != null)
            {
                putOrdersActive = true;
                server.AddChannel(putOrdersChannel);
            }
        }

        // **********************************************************************

        void IPutOrdersProvider.Unsubscribe()
        {
            if (putOrdersActive && server != null)
            {
                putOrdersActive = false;
                server.RmvChannel(putOrdersChannel);

                if (!stockActive && !ticksActive && !settingsActive && !tradesActive)
                    DisposeServer();
            }
        }

        // **********************************************************************

        void ISecListProvider.GetSecList(Action<SecList> callback)
        {
            const string secListFileName = "seclist.csv";

            const int classNameIndex = 0;
            const int classCodeIndex = 1;
            const int secNameIndex = 2;
            const int secCodeIndex = 3;
            const int priceStepIndex = 4;

            SecList secList = new SecList();

            try
            {
                using (StreamReader stream = new StreamReader(cfg.AsmPath + secListFileName))
                {
                    char[] delimiter = new char[] { ';' };
                    string line;

                    while ((line = stream.ReadLine()) != null)
                    {
                        string[] str = line.Split(delimiter);
                        double step;

                        if (str.Length < 5 || !double.TryParse(str[priceStepIndex],
                          NumberStyles.Float, NumberFormatInfo.InvariantInfo, out step))
                            throw new FormatException("Неверный формат файла.");

                        secList.Add(str[secNameIndex], str[secCodeIndex],
                          str[classNameIndex], str[classCodeIndex], step);
                    }
                }
            }
            catch (Exception e)
            {
                secList.Error = e.Message;
            }

            callback.Invoke(secList);
        }

        // **********************************************************************
    }
}
