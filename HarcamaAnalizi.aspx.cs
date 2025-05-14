using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FinansWebApp
{
    public partial class HarcamaAnalizi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["AccountNumber"] != null)
                {
                    string accountNumber = Session["AccountNumber"].ToString();
                    LoadMonthlyExpenses(accountNumber);
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void LoadMonthlyExpenses(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;

            // Mevcut aylık harcamalar sorgusu
            string monthlyQuery = @"
                SELECT 
                    FORMAT(t.TransactionDate, 'yyyy-MM') as YearMonth,
                    FORMAT(t.TransactionDate, 'MMMM yyyy') as MonthYear,
                    SUM(CASE WHEN t.TransactionType = 'Gider' THEN t.Amount ELSE 0 END) as TotalExpense
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                WHERE a.AccountNumber = @accountNumber
                    AND t.TransactionDate >= DATEADD(MONTH, -11, GETDATE())
                GROUP BY FORMAT(t.TransactionDate, 'yyyy-MM'), FORMAT(t.TransactionDate, 'MMMM yyyy')
                ORDER BY YearMonth";

            // Yeni kategori bazlı harcamalar sorgusu
            string categoryQuery = @"
                SELECT 
                    c.CategoryName,
                    SUM(CASE WHEN t.TransactionType = 'Gider' THEN t.Amount ELSE 0 END) as TotalExpense
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                INNER JOIN Categories c ON t.CategoryID = c.CategoryID
                WHERE a.AccountNumber = @accountNumber
                    AND t.TransactionDate >= DATEADD(MONTH, -1, GETDATE())  -- Son 1 ayın verileri
                    AND t.TransactionType = 'Gider'
                GROUP BY c.CategoryName
                ORDER BY TotalExpense DESC";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Aylık harcamaları çek
                List<string> months = new List<string>();
                List<decimal> expenses = new List<decimal>();

                using (SqlCommand cmd = new SqlCommand(monthlyQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            months.Add(reader["MonthYear"].ToString());
                            expenses.Add(Convert.ToDecimal(reader["TotalExpense"]));
                        }
                    }
                }

                // Kategori bazlı harcamaları çek
                List<string> categories = new List<string>();
                List<decimal> categoryExpenses = new List<decimal>();

                using (SqlCommand cmd = new SqlCommand(categoryQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(reader["CategoryName"].ToString());
                            categoryExpenses.Add(Convert.ToDecimal(reader["TotalExpense"]));
                        }
                    }
                }

                // JavaScript'e veri aktarımı için script oluştur
                StringBuilder script = new StringBuilder();
                script.Append("<script type='text/javascript'>");
                script.Append("document.addEventListener('DOMContentLoaded', function() {");
                script.AppendFormat("var aylar = {0};", JsonConvert.SerializeObject(months));
                script.AppendFormat("var harcamalar = {0};", JsonConvert.SerializeObject(expenses));
                script.AppendFormat("var kategoriler = {0};", JsonConvert.SerializeObject(categories));
                script.AppendFormat("var kategoriHarcamalari = {0};", JsonConvert.SerializeObject(categoryExpenses));
                script.Append("grafikOlustur(aylar, harcamalar, kategoriler, kategoriHarcamalari);");
                script.Append("});");
                script.Append("</script>");

                ltrlHarcamaVerileri.Text = script.ToString();
            }
        }
    }
} 