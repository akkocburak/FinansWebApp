using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Util;

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
                    LoadTransactions(accountNumber);
                }
                else
                {
                    Response.Redirect("Login.aspx");
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
    }
}