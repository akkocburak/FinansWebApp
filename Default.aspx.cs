using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FinansWebApp
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["AccountNumber"] != null)
                {
                    string accountNumber = Session["AccountNumber"].ToString();
                    LoadBalanceInfo(accountNumber);
                    LoadTransactions(accountNumber);
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private void LoadBalanceInfo(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Toplam bakiye sorgusu
                string balanceQuery = @"
                    SELECT TOP 1 Balance 
                    FROM Transactions 
                    WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                    ORDER BY TransactionDate DESC";

                // Bu ayki gelir ve gider sorgusu
                string monthlyQuery = @"
                    SELECT 
                        SUM(CASE WHEN TransactionType = 'Gelir' THEN Amount ELSE 0 END) as MonthlyIncome,
                        SUM(CASE WHEN TransactionType = 'Gider' THEN Amount ELSE 0 END) as MonthlyExpense
                    FROM Transactions 
                    WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                    AND MONTH(TransactionDate) = MONTH(GETDATE())
                    AND YEAR(TransactionDate) = YEAR(GETDATE())";

                // Geçen aya göre değişim sorgusu
                string changeQuery = @"
                    SELECT 
                        (SELECT SUM(CASE WHEN TransactionType = 'Gelir' THEN Amount ELSE 0 END)
                         FROM Transactions 
                         WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                         AND MONTH(TransactionDate) = MONTH(GETDATE())
                         AND YEAR(TransactionDate) = YEAR(GETDATE())) -
                        (SELECT SUM(CASE WHEN TransactionType = 'Gelir' THEN Amount ELSE 0 END)
                         FROM Transactions 
                         WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                         AND MONTH(TransactionDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
                         AND YEAR(TransactionDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as IncomeChange,
                        (SELECT SUM(CASE WHEN TransactionType = 'Gider' THEN Amount ELSE 0 END)
                         FROM Transactions 
                         WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                         AND MONTH(TransactionDate) = MONTH(GETDATE())
                         AND YEAR(TransactionDate) = YEAR(GETDATE())) -
                        (SELECT SUM(CASE WHEN TransactionType = 'Gider' THEN Amount ELSE 0 END)
                         FROM Transactions 
                         WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                         AND MONTH(TransactionDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
                         AND YEAR(TransactionDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))) as ExpenseChange";

                // Toplam bakiyeyi çek
                using (SqlCommand cmd = new SqlCommand(balanceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    var balance = cmd.ExecuteScalar();
                    lblTotalBalance.Text = string.Format("₺{0:N2}", balance ?? 0);
                }

                // Bu ayki gelir ve giderleri çek
                using (SqlCommand cmd = new SqlCommand(monthlyQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblMonthlyIncome.Text = string.Format("₺{0:N2}", reader["MonthlyIncome"] ?? 0);
                            lblMonthlyExpense.Text = string.Format("₺{0:N2}", reader["MonthlyExpense"] ?? 0);
                        }
                    }
                }

                // Değişimleri çek
                using (SqlCommand cmd = new SqlCommand(changeQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal incomeChange = reader["IncomeChange"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["IncomeChange"]);
                            decimal expenseChange = reader["ExpenseChange"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ExpenseChange"]);

                            lblIncomeChange.Text = string.Format("{0}{1:N2}% geçen aya göre", 
                                incomeChange >= 0 ? "+" : "", 
                                incomeChange);

                            lblExpenseChange.Text = string.Format("{0}{1:N2}% geçen aya göre", 
                                expenseChange >= 0 ? "+" : "", 
                                expenseChange);
                        }
                    }
                }
            }
        }

        private void LoadTransactions(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;

            string query = @"
                SELECT t.TransactionDate, t.TransactionType, c.CategoryName, t.Description, t.Amount, t.Balance
                FROM Transactions t
                INNER JOIN Accounts a ON t.AccountID = a.AccountID
                INNER JOIN Categories c ON t.CategoryID = c.CategoryID
                WHERE a.AccountNumber = @accountNumber
                ORDER BY t.TransactionDate DESC";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvTransactions.DataSource = dt;
                gvTransactions.DataBind();
            }
        }

        protected void gvTransactions_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTransactions.PageIndex = e.NewPageIndex;
            LoadTransactions(Session["AccountNumber"].ToString());
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvTransactions.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            LoadTransactions(Session["AccountNumber"].ToString());
        }
    }
} 