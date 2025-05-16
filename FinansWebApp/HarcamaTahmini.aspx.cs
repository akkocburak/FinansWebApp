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
                    return;
                }

                // Sayfa yüklendiğinde tahmin işlemini başlat
                RegisterAsyncTask(new PageAsyncTask(async () => await PredictExpense()));
            }
        }

        private async Task PredictExpense()
        {
            try
            {
                string accountNumber = Session["AccountNumber"].ToString();
                System.Diagnostics.Debug.WriteLine(string.Format("Tahmin yapılıyor - Hesap No: {0}", accountNumber));

                var predictionResult = await _mlService.PredictNextMonthExpense(accountNumber);

                // Debug bilgileri
                System.Diagnostics.Debug.WriteLine("Tahmin sonuçları:");
                System.Diagnostics.Debug.WriteLine(string.Format("HoltWinters: {0}", predictionResult.HoltWintersPrediction));
                System.Diagnostics.Debug.WriteLine(string.Format("LinearRegression: {0}", predictionResult.LinearRegressionPrediction));
                System.Diagnostics.Debug.WriteLine(string.Format("Final: {0}", predictionResult.FinalPrediction));

                // Holt-Winters tahmini göster
                if (predictionResult.HoltWintersPrediction != 0)
                {
                    lblHoltWinters.Text = string.Format("₺{0:N2}", predictionResult.HoltWintersPrediction);
                }
                else
                {
                    lblHoltWinters.Text = "Hesaplanamadı";
                }

                // Linear Regression tahmini göster
                if (predictionResult.LinearRegressionPrediction != 0)
                {
                    lblLinearRegression.Text = string.Format("₺{0:N2}", predictionResult.LinearRegressionPrediction);
                }
                else
                {
                    lblLinearRegression.Text = "Hesaplanamadı";
                }

                // Final tahmin göster
                if (predictionResult.FinalPrediction != 0)
                {
                    lblFinalPrediction.Text = string.Format("₺{0:N2}", predictionResult.FinalPrediction);
                }
                else
                {
                    lblFinalPrediction.Text = "Hesaplanamadı";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tahmin hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                lblHoltWinters.Text = "Hata";
                lblLinearRegression.Text = "Hata";
                lblFinalPrediction.Text = "Tahmin yapılamadı. Lütfen daha sonra tekrar deneyin.";
            }
        }
    }
}