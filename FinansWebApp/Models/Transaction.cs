using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinansWebApp.Models
{


    public class Transaction
    {
        public int AccoundID { get; set; }
        public string TransactionType { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public DateTime TransactionDate { get; set; }
    }

}