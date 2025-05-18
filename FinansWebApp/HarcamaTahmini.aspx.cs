using System;
using System.Threading.Tasks;
using System.Web.UI;
using FinansWebApp.Services;
using System.Collections.Generic;
using System.Linq;

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
                RegisterAsyncTask(new PageAsyncTask(async () => await LoadAllPredictions()));
            }
        }

        private async Task LoadAllPredictions()
        {
            try
            {
                string accountNumber = Session["AccountNumber"].ToString();
                System.Diagnostics.Debug.WriteLine(string.Format("Tahmin yapılıyor - Hesap No: {0}", accountNumber));

                // Genel tahminleri yükle
                var predictionResult = await _mlService.PredictNextMonthExpense(accountNumber);
                LoadGeneralPredictions(predictionResult);

                // Kategori bazlı tahminleri yükle
                try
                {
                    var categoryPredictions = await _mlService.PredictCategoryExpenses(accountNumber);
                    LoadCategoryPredictions(categoryPredictions);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Kategori tahmini hatası: {0}", ex.Message));
                    gvCategoryPredictions.Visible = false;
                    lblCategoryError.Text = "Kategori bazlı tahminler hesaplanamadı. Lütfen daha sonra tekrar deneyin.";
                    lblCategoryError.Visible = true;
                }

                // Tasarruf önerilerini yükle
                try
                {
                    var recommendations = await _mlService.GetSavingsRecommendations(accountNumber);
                    LoadSavingsRecommendations(recommendations);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Tasarruf önerileri hatası: {0}", ex.Message));
                    rptSavingsRecommendations.Visible = false;
                    lblRecommendationError.Text = "Tasarruf önerileri oluşturulamadı. Lütfen daha sonra tekrar deneyin.";
                    lblRecommendationError.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Genel tahmin hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                HandleError("Tahminler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
            }
        }

        private void LoadGeneralPredictions(PredictionResult result)
        {
            if (result == null)
            {
                HandleError("Tahmin sonuçları alınamadı.");
                return;
            }

            if (result.HoltWintersPrediction != 0)
                lblHoltWinters.Text = string.Format("₺{0:N2}", result.HoltWintersPrediction);
            else
                lblHoltWinters.Text = "Hesaplanamadı";

            if (result.LinearRegressionPrediction != 0)
                lblLinearRegression.Text = string.Format("₺{0:N2}", result.LinearRegressionPrediction);
            else
                lblLinearRegression.Text = "Hesaplanamadı";

            if (result.FinalPrediction != 0)
                lblFinalPrediction.Text = string.Format("₺{0:N2}", result.FinalPrediction);
            else
                lblFinalPrediction.Text = "Hesaplanamadı";
        }

        private void LoadCategoryPredictions(List<CategoryPrediction> predictions)
        {
            if (predictions == null || !predictions.Any())
            {
                gvCategoryPredictions.Visible = false;
                lblCategoryError.Text = "Kategori bazlı tahmin bulunamadı.";
                lblCategoryError.Visible = true;
                return;
            }

            gvCategoryPredictions.DataSource = predictions;
            gvCategoryPredictions.DataBind();
            gvCategoryPredictions.Visible = true;
            lblCategoryError.Visible = false;
        }

        private void LoadSavingsRecommendations(List<SavingsRecommendation> recommendations)
        {
            if (recommendations == null || !recommendations.Any())
            {
                rptSavingsRecommendations.Visible = false;
                lblRecommendationError.Text = "Tasarruf önerisi bulunamadı.";
                lblRecommendationError.Visible = true;
                return;
            }

            rptSavingsRecommendations.DataSource = recommendations;
            rptSavingsRecommendations.DataBind();
            rptSavingsRecommendations.Visible = true;
            lblRecommendationError.Visible = false;
        }

        private void HandleError(string message)
        {
            lblHoltWinters.Text = "Hata";
            lblLinearRegression.Text = "Hata";
            lblFinalPrediction.Text = message;
            
            // Hide all sections on general error
            gvCategoryPredictions.Visible = false;
            rptSavingsRecommendations.Visible = false;
            
            // Show error messages
            lblCategoryError.Text = message;
            lblRecommendationError.Text = message;
            
            lblCategoryError.Visible = true;
            lblRecommendationError.Visible = true;
        }
    }
}