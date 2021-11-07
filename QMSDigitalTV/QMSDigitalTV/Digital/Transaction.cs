using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMSDigitalTV.Digital
{
    class Transaction
    {
        public string Token { set; get; }
        public bool IsPriority { set; get; }
        public int Window { set; get; }

        public Transaction(string token, bool isPriority, int window)
        {
            this.Token = token;
            this.IsPriority = isPriority;
            this.Window = window;
        }
    }
}
