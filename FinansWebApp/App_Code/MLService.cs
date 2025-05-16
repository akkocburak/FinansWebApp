using System;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

                var request = new
                {
                    transactions = transactions
                };

                var jsonRequest = JsonConvert.SerializeObject(request);
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var response = await client.UploadStringTaskAsync(
                        new Uri(_apiUrl + "/predict"),
                        "POST",
                        jsonRequest
                    );
                    
                    // Python servisinden gelen yanıtı PredictionResponse'a dönüştür
                    var predictionResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    
                    // PredictionResult nesnesini oluştur
                    var result = new PredictionResult
                    {
                        HoltWintersPrediction = Convert.ToDecimal(predictionResponse["HoltWintersPrediction"]),
                        LinearRegressionPrediction = Convert.ToDecimal(predictionResponse["LinearRegressionPrediction"]),
                        FinalPrediction = Convert.ToDecimal(predictionResponse["FinalPrediction"])
                    };
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Tahmin hatası: " + ex.Message);
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