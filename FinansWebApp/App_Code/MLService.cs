using System;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace FinansWebApp.Services
{
    public class MLService
    {
        private readonly string _apiUrl = "http://localhost:5000";

        public async Task<PredictionResult> PredictNextMonthExpense(string accountNumber)
        {
            try
            {
                // Son 13 ayın verilerini al
                var transactions = await GetTransactionHistory(accountNumber);
                
                // Debug: İşlem sayısını kontrol et
                System.Diagnostics.Debug.WriteLine(string.Format("Toplam işlem sayısı: {0}", transactions.Count));

                // JSON serileştirme ayarlarını yapılandır
                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss",
                    Formatting = Formatting.Indented
                };

                // TransactionData listesini düz dictionary listesine çevir
                var transactionsList = transactions.Select(t => new Dictionary<string, object>
                {
                    { "TransactionDate", t.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss") },
                    { "TransactionType", t.TransactionType },
                    { "Amount", t.Amount }
                }).ToList();

                var request = new Dictionary<string, object>
                {
                    { "transactions", transactionsList }
                };

                var jsonRequest = JsonConvert.SerializeObject(request, jsonSettings);
                
                // Debug: Gönderilen JSON'ı kontrol et
                System.Diagnostics.Debug.WriteLine(string.Format("Gönderilen JSON: {0}", jsonRequest));

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var response = await client.UploadStringTaskAsync(
                        new Uri(_apiUrl + "/predict"),
                        "POST",
                        jsonRequest
                    );
                    
                    // Debug: Alınan yanıtı kontrol et
                    System.Diagnostics.Debug.WriteLine(string.Format("Python servisinden gelen yanıt: {0}", response));

                    // Python servisinden gelen yanıtı PredictionResponse'a dönüştür
                    var predictionResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    
                    // Debug: Dictionary içeriğini kontrol et
                    foreach (var kvp in predictionResponse)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Key: {0}, Value: {1}", kvp.Key, kvp.Value));
                    }

                    // PredictionResult nesnesini oluştur
                    var result = new PredictionResult();

                    // Null kontrolü yaparak değerleri ata
                    if (predictionResponse["HoltWintersPrediction"] != null && 
                        predictionResponse["HoltWintersPrediction"].ToString() != "null")
                    {
                        result.HoltWintersPrediction = Convert.ToDecimal(predictionResponse["HoltWintersPrediction"]);
                    }
                    
                    if (predictionResponse["LinearRegressionPrediction"] != null && 
                        predictionResponse["LinearRegressionPrediction"].ToString() != "null")
                    {
                        result.LinearRegressionPrediction = Convert.ToDecimal(predictionResponse["LinearRegressionPrediction"]);
                    }
                    
                    if (predictionResponse["FinalPrediction"] != null && 
                        predictionResponse["FinalPrediction"].ToString() != "null")
                    {
                        result.FinalPrediction = Convert.ToDecimal(predictionResponse["FinalPrediction"]);
                    }

                    // Debug: Sonuç değerlerini kontrol et
                    System.Diagnostics.Debug.WriteLine(string.Format("HoltWinters: {0}", result.HoltWintersPrediction));
                    System.Diagnostics.Debug.WriteLine(string.Format("LinearRegression: {0}", result.LinearRegressionPrediction));
                    System.Diagnostics.Debug.WriteLine(string.Format("Final: {0}", result.FinalPrediction));
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tahmin hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack Trace: {0}", ex.StackTrace));
                throw;
            }
        }

        private async Task<List<TransactionData>> GetTransactionHistory(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;
            var transactions = new List<TransactionData>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TransactionDate, TransactionType, Amount
                    FROM Transactions
                    WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                    AND TransactionDate >= DATEADD(MONTH, -13, GETDATE())
                    ORDER BY TransactionDate";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new TransactionData
                            {
                                TransactionDate = reader.GetDateTime(0),
                                TransactionType = reader.GetString(1),
                                Amount = reader.GetDecimal(2)
                            });
                        }
                    }
                }
            }

            return transactions;
        }
    }

    public class TransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
    }

    public class PredictionResult
    {
        public decimal HoltWintersPrediction { get; set; }
        public decimal LinearRegressionPrediction { get; set; }
        public decimal FinalPrediction { get; set; }
    }
}