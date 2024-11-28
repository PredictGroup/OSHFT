
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSHFT_Q_R
{
    public abstract class BaseExchange
    {
        
        private string _name;
        private bool _active;


        protected Dictionary<DateTime, TradeOp> results;

        // **********************************************************************

        protected const string uaDescr = "User";

        // **********************************************************************

        public BaseExchange(string name)
        {
            _name = name;

            _active = false;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public bool Active
        {
            get
            {
                return _active;
            }
        }

        // **********************************************************************

        public abstract void ProcessSpread(Spread spread);

        public abstract void ProcessQuotes(Quote[] quotes);

        public abstract void ProcessTick(Tick tick); // получение данных

        public abstract void ProcessSetting(Setting setting); // получение данных

        public abstract void ProcessTrade(Trade trade); // получение сделок

        public abstract void ProcessPutOrder(PutOrder putOrder); // получение заявок

        // **********************************************************************

        public void Start()
        {
            _active = true;
        }

        public void Stop()
        {
            _active = false;
        }
    }
}
