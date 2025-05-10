using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinansWebApp.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}