using System;
using System.Threading.Tasks;
using System.Web.UI;
using FinansWebApp.Services;

namespace FinansWebApp
{
    public partial class HarcamaTahmini : System.Web.UI.Page
    {
        private readonly MLService _mlService;

        public HarcamaTahmini()
        {
            _mlService = new MLService();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["AccountNumber"] == null)
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        protected void btnPredict_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () => await PredictExpense()));
        }

        private async Task PredictExpense()
        {
            try
            {
                string accountNumber = Session["AccountNumber"].ToString();
                decimal predictedExpense = await _mlService.PredictNextMonthExpense(accountNumber);
                lblPrediction.Text = string.Format("₺{0:N2}", predictedExpense);
            }
            catch (Exception ex)
            {
                lblPrediction.Text = "Tahmin yapılamadı. Lütfen daha sonra tekrar deneyin.";
                System.Diagnostics.Debug.WriteLine($"Tahmin hatası: {ex.Message}");
            }
        }
    }
}