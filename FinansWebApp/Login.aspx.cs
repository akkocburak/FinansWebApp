using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FinansWebApp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string accountNumber = txtAccountNumber.Text.Trim();
            string password = txtPassword.Text.Trim();

            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT COUNT(*) FROM Accounts WHERE AccountNumber = @accountNumber AND Password = @password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                if (count == 1)
                {
                    int accountId = Convert.ToInt32(Session["AccountID"]);
                    Session["AccountNumber"] = accountNumber; 
                    Response.Redirect("Default.aspx");
                }
                else
                {
                    Label1.Text = "Hatalı hesap numarası veya şifre.";
                }
            }
        }
    }
}