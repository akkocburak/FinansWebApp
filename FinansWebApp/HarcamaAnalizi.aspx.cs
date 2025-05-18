using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
                    LoadMonthDropdown();
                    string accountNumber = Session["AccountNumber"].ToString();
                    LoadFinancialData(accountNumber);
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void LoadMonthDropdown()
        {
            // Son 12 ayı dropdown'a ekle
            var months = new List<object>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < 12; i++)
            {
                var date = currentDate.AddMonths(-i);
                months.Add(new
                {
                    Text = date.ToString("MMMM yyyy"),
                    Value = date.ToString("yyyy-MM")
                });
            }

            ddlAySecimi.DataSource = months;
            ddlAySecimi.DataTextField = "Text";
            ddlAySecimi.DataValueField = "Value";
            ddlAySecimi.DataBind();

            // Seçili ayı göster
            lblSeciliDonem.Text = months.First().GetType().GetProperty("Text").GetValue(months.First(), null).ToString();
        }

        protected void ddlAySecimi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["AccountNumber"] != null)
            {
                string accountNumber = Session["AccountNumber"].ToString();
                LoadFinancialData(accountNumber);
                
                // Seçili ayı güncelle
                lblSeciliDonem.Text = ddlAySecimi.SelectedItem.Text;
            }
        }

        private void LoadFinancialData(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;
            var selectedDate = DateTime.ParseExact(ddlAySecimi.SelectedValue, "yyyy-MM", null);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Seçili ayın özet verileri
                LoadMonthlySummary(conn, accountNumber, selectedDate);

                // Kategori bazlı harcamalar
                var (categories, categoryExpenses) = LoadCategoryExpenses(conn, accountNumber, selectedDate);

                // Yıllık trend verileri
                var yearlyTrend = LoadYearlyTrend(conn, accountNumber);

                // JavaScript'e veri aktarımı için script oluştur
                StringBuilder script = new StringBuilder();
                script.Append("<script type='text/javascript'>");
                script.Append("document.addEventListener('DOMContentLoaded', function() {");
                
                // Aylık veriler
                script.AppendFormat("var aylar = {0};", JsonConvert.SerializeObject(yearlyTrend.Months));
                script.AppendFormat("var harcamalar = {0};", JsonConvert.SerializeObject(yearlyTrend.Expenses));
                
                // Kategori verileri
                script.AppendFormat("var kategoriler = {0};", JsonConvert.SerializeObject(categories));
                script.AppendFormat("var kategoriHarcamalari = {0};", JsonConvert.SerializeObject(categoryExpenses));

                // Yıllık trend verileri
                var yillikVeriler = new
                {
                    labels = yearlyTrend.Months,
                    gelirler = yearlyTrend.Incomes,
                    giderler = yearlyTrend.Expenses
                };
                script.AppendFormat("var yillikVeriler = {0};", JsonConvert.SerializeObject(yillikVeriler));

                script.Append("grafikOlustur(aylar, harcamalar, kategoriler, kategoriHarcamalari, yillikVeriler);");
                script.Append("});");
                script.Append("</script>");

                ltrlHarcamaVerileri.Text = script.ToString();
            }
        }

        private void LoadMonthlySummary(SqlConnection conn, string accountNumber, DateTime selectedDate)
        {
            string query = @"
                SELECT 
                    SUM(CASE WHEN TransactionType = 'Gelir' THEN Amount ELSE 0 END) as TotalIncome,
                    SUM(CASE WHEN TransactionType = 'Harcamalar' THEN Amount ELSE 0 END) as TotalExpense
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                WHERE a.AccountNumber = @accountNumber
                    AND MONTH(t.TransactionDate) = @month
                    AND YEAR(t.TransactionDate) = @year";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@month", selectedDate.Month);
                cmd.Parameters.AddWithValue("@year", selectedDate.Year);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal totalIncome = reader.GetDecimal(0);
                        decimal totalExpense = reader.GetDecimal(1);
                        decimal netAmount = totalIncome - totalExpense;

                        lblToplamGelir.Text = string.Format("₺{0:N2}", totalIncome);
                        lblToplamGider.Text = string.Format("₺{0:N2}", totalExpense);
                        lblNetDurum.Text = string.Format("₺{0:N2}", netAmount);
                    }
                }
            }
        }

        private (List<string> Categories, List<decimal> Expenses) LoadCategoryExpenses(SqlConnection conn, string accountNumber, DateTime selectedDate)
        {
            var categories = new List<string>();
            var expenses = new List<decimal>();

            string query = @"
                SELECT 
                    c.CategoryName,
                    SUM(t.Amount) as TotalExpense
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                INNER JOIN Categories c ON t.CategoryID = c.CategoryID
                WHERE a.AccountNumber = @accountNumber
                    AND t.TransactionType = 'Harcamalar'
                    AND MONTH(t.TransactionDate) = @month
                    AND YEAR(t.TransactionDate) = @year
                GROUP BY c.CategoryName
                ORDER BY TotalExpense DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@month", selectedDate.Month);
                cmd.Parameters.AddWithValue("@year", selectedDate.Year);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader["CategoryName"].ToString());
                        expenses.Add(reader.GetDecimal(1));
                    }
                }
            }

            return (categories, expenses);
        }

        private (List<string> Months, List<decimal> Incomes, List<decimal> Expenses) LoadYearlyTrend(SqlConnection conn, string accountNumber)
        {
            var months = new List<string>();
            var incomes = new List<decimal>();
            var expenses = new List<decimal>();

            string query = @"
                SELECT 
                    FORMAT(t.TransactionDate, 'yyyy-MM') as YearMonth,
                    FORMAT(t.TransactionDate, 'MMM yyyy') as MonthYear,
                    SUM(CASE WHEN t.TransactionType = 'Gelir' THEN t.Amount ELSE 0 END) as TotalIncome,
                    SUM(CASE WHEN t.TransactionType = 'Harcamalar' THEN t.Amount ELSE 0 END) as TotalExpense
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                WHERE a.AccountNumber = @accountNumber
                    AND t.TransactionDate >= DATEADD(MONTH, -11, @selectedDate)
                    AND t.TransactionDate <= @selectedDate
                GROUP BY FORMAT(t.TransactionDate, 'yyyy-MM'), FORMAT(t.TransactionDate, 'MMM yyyy')
                ORDER BY YearMonth";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@selectedDate", DateTime.ParseExact(ddlAySecimi.SelectedValue, "yyyy-MM", null));
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        months.Add(reader["MonthYear"].ToString());
                        incomes.Add(reader.GetDecimal(2));
                        expenses.Add(reader.GetDecimal(3));
                    }
                }
            }

            return (months, incomes, expenses);
        }
    }
}