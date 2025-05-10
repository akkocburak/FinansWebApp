using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Principal;

namespace FinansWebApp.Models
{
    public class FinansDbContext : DbContext
    {
        public FinansDbContext() : base("FinansConnectionString") // Web.config içindeki connection string adıyla aynı olmalı
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }
}