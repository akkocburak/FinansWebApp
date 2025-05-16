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
                var predictionResult = await _mlService.PredictNextMonthExpense(accountNumber);

                // Holt-Winters tahmini göster
                lblHoltWinters.Text = string.Format("₺{0:N2}", predictionResult.HoltWintersPrediction);

                // Linear Regression tahmini göster
                lblLinearRegression.Text = string.Format("₺{0:N2}", predictionResult.LinearRegressionPrediction);

                // Final tahmin göster
                lblFinalPrediction.Text = string.Format("₺{0:N2}", predictionResult.FinalPrediction);
            }
            catch (Exception ex)
            {
                lblHoltWinters.Text = "Hata";
                lblLinearRegression.Text = "Hata";
                lblFinalPrediction.Text = "Tahmin yapılamadı. Lütfen daha sonra tekrar deneyin.";
                System.Diagnostics.Debug.WriteLine($"Tahmin hatası: {ex.Message}");
            }
        }
    }
}