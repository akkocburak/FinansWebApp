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
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Encoding = Encoding.UTF8;
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

        public async Task<List<CategoryPrediction>> PredictCategoryExpenses(string accountNumber)
        {
            try
            {
                var transactions = await GetTransactionHistory(accountNumber);
                System.Diagnostics.Debug.WriteLine(string.Format("Kategori tahmini için veri sayısı: {0}", transactions.Count));

                // Veriyi hazırla ve kontrol et
                var transactionList = transactions.Select(t => new
                {
                    TransactionDate = t.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    CategoryName = GetCategoryName(t.CategoryID)
                }).ToList();

                // İlk birkaç işlemi logla
                for (int i = 0; i < Math.Min(5, transactionList.Count); i++)
                {
                    var t = transactionList[i];
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "Örnek işlem {0}: Tarih={1}, Tip={2}, Tutar={3}, Kategori={4}",
                        i + 1, t.TransactionDate, t.TransactionType, t.Amount, t.CategoryName));
                }

                var request = new Dictionary<string, object>
                {
                    { "transactions", transactionList }
                };

                var response = await SendMLRequest("/predict_categories", request);
                System.Diagnostics.Debug.WriteLine(string.Format("Kategori tahmini yanıtı: {0}", response));

                if (string.IsNullOrEmpty(response) || response == "[]")
                {
                    System.Diagnostics.Debug.WriteLine("Kategori tahmini boş yanıt döndü");
                    return new List<CategoryPrediction>();
                }

                var result = JsonConvert.DeserializeObject<List<CategoryPrediction>>(response);
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Tahmin edilen kategori sayısı: {0}", result.Count));
                    foreach (var prediction in result)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format(
                            "Tahmin: Kategori={0}, Bu Ay={1:C2}, Gelecek Ay={2:C2}, Değişim=%{3:F2}",
                            prediction.Category, prediction.CurrentMonth, prediction.PredictedAmount, prediction.ChangePercentage));
                    }
                }

                return result ?? new List<CategoryPrediction>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Kategori tahmini hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                throw;
            }
        }

        public async Task<List<SavingsRecommendation>> GetSavingsRecommendations(string accountNumber)
        {
            try
            {
                var transactions = await GetTransactionHistory(accountNumber);
                System.Diagnostics.Debug.WriteLine(string.Format("Tasarruf önerileri için veri sayısı: {0}", transactions.Count));

                // Veriyi hazırla ve kontrol et
                var transactionList = transactions.Select(t => new
                {
                    TransactionDate = t.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    CategoryName = GetCategoryName(t.CategoryID)
                }).ToList();

                // İlk birkaç işlemi logla
                for (int i = 0; i < Math.Min(5, transactionList.Count); i++)
                {
                    var t = transactionList[i];
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "Örnek işlem {0}: Tarih={1}, Tip={2}, Tutar={3}, Kategori={4}",
                        i + 1, t.TransactionDate, t.TransactionType, t.Amount, t.CategoryName));
                }

                var request = new Dictionary<string, object>
                {
                    { "transactions", transactionList }
                };

                var response = await SendMLRequest("/recommend", request);
                System.Diagnostics.Debug.WriteLine(string.Format("Tasarruf önerileri yanıtı: {0}", response));

                if (string.IsNullOrEmpty(response) || response == "[]")
                {
                    System.Diagnostics.Debug.WriteLine("Tasarruf önerileri boş yanıt döndü");
                    return new List<SavingsRecommendation>();
                }

                var result = JsonConvert.DeserializeObject<List<SavingsRecommendation>>(response);
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Oluşturulan öneri sayısı: {0}", result.Count));
                    foreach (var recommendation in result)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format(
                            "Öneri: Başlık={0}, Tasarruf={1:C2}",
                            recommendation.Title, recommendation.PotentialSaving));
                    }
                }

                return result ?? new List<SavingsRecommendation>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tasarruf önerileri hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                throw;
            }
        }

        private string GetCategoryName(int categoryId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT CategoryName FROM Categories WHERE CategoryID = @categoryId", conn))
                    {
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);
                        var result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "Diğer";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Kategori adı getirme hatası: {0}", ex.Message));
                return "Diğer";
            }
        }

        private async Task<string> SendMLRequest(string endpoint, object data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("SendMLRequest başladı - Endpoint: {0}", endpoint));
                
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Encoding = Encoding.UTF8;
                    
                    // Veriyi düzenle
                    var jsonRequest = string.Empty;
                    if (data is Dictionary<string, object>)
                    {
                        var dict = data as Dictionary<string, object>;
                        if (dict.ContainsKey("transactions"))
                        {
                            var transactions = dict["transactions"] as IEnumerable<object>;
                            if (transactions != null)
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("İşlem sayısı: {0}", transactions.Count()));
                                if (!transactions.Any())
                                {
                                    System.Diagnostics.Debug.WriteLine("UYARI: Boş işlem listesi!");
                                    return "[]";
                                }
                            }
                        }
                    }
                    
                    jsonRequest = JsonConvert.SerializeObject(data, new JsonSerializerSettings 
                    { 
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss",
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    
                    // İstek detaylarını logla
                    System.Diagnostics.Debug.WriteLine(string.Format("Endpoint: {0}{1}", _apiUrl, endpoint));
                    System.Diagnostics.Debug.WriteLine(string.Format("Request Headers: Content-Type={0}", client.Headers[HttpRequestHeader.ContentType]));
                    System.Diagnostics.Debug.WriteLine(string.Format("Request Body: {0}", jsonRequest));
                    
                    var response = await client.UploadStringTaskAsync(
                        new Uri(_apiUrl + endpoint),
                        "POST",
                        jsonRequest
                    );
                    
                    // Yanıt kontrolü
                    if (string.IsNullOrEmpty(response))
                    {
                        System.Diagnostics.Debug.WriteLine("UYARI: Boş yanıt alındı!");
                        return "[]";
                    }

                    try
                    {
                        // Yanıtın geçerli JSON olduğunu kontrol et
                        var testParse = JsonConvert.DeserializeObject(response);
                        System.Diagnostics.Debug.WriteLine(string.Format("Geçerli JSON yanıtı alındı: {0}", response));
                        return response;
                    }
                    catch (JsonReaderException jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("HATA: Geçersiz JSON yanıtı: {0}", response));
                        System.Diagnostics.Debug.WriteLine(string.Format("JSON parse hatası: {0}", jsonEx.Message));
                        throw new Exception("Python servisinden geçersiz JSON yanıtı alındı", jsonEx);
                    }
                }
            }
            catch (WebException webEx)
            {
                var errorResponse = string.Empty;
                try
                {
                    using (var reader = new StreamReader(webEx.Response.GetResponseStream(), Encoding.UTF8))
                    {
                        errorResponse = reader.ReadToEnd();
                    }
                }
                catch { }

                System.Diagnostics.Debug.WriteLine(string.Format("Web isteği hatası: {0}", webEx.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Hata detayı: {0}", webEx.Status));
                System.Diagnostics.Debug.WriteLine(string.Format("Sunucu yanıtı: {0}", errorResponse));
                
                throw new Exception(string.Format("Python servisine istek gönderilirken hata oluştu: {0}", webEx.Message), webEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("SendMLRequest genel hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                throw;
            }
        }

        private async Task<List<TransactionData>> GetTransactionHistory(string accountNumber)
        {
            string connStr = ConfigurationManager.ConnectionStrings["BaglantiCumlem"].ConnectionString;
            var transactions = new List<TransactionData>();

            try 
            {
                System.Diagnostics.Debug.WriteLine(string.Format("GetTransactionHistory başladı - Account: {0}", accountNumber));

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"
                        SELECT TransactionDate, TransactionType, Amount, CategoryID
                    FROM Transactions
                    WHERE AccountID = (SELECT AccountID FROM Accounts WHERE AccountNumber = @accountNumber)
                    AND TransactionDate >= DATEADD(MONTH, -13, GETDATE())
                    ORDER BY TransactionDate";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
                        await conn.OpenAsync();

                        System.Diagnostics.Debug.WriteLine("Veritabanı bağlantısı açıldı, sorgu çalıştırılıyor...");
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var transaction = new TransactionData
                                {
                                    TransactionDate = reader.GetDateTime(0),
                                    TransactionType = reader.GetString(1),
                                    Amount = reader.GetDecimal(2),
                                    CategoryID = reader.GetInt32(3)
                                };
                                transactions.Add(transaction);

                                // Her 100 işlemde bir log
                                if (transactions.Count % 100 == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine(string.Format("{0} işlem okundu...", transactions.Count));
                                }
                            }
                        }
                    }
                }

                // İşlem tiplerinin dağılımını logla
                var typeDistribution = transactions
                    .GroupBy(t => t.TransactionType)
                    .ToDictionary(g => g.Key, g => g.Count());

                System.Diagnostics.Debug.WriteLine("İşlem tipi dağılımı:");
                foreach (var type in typeDistribution)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("- {0}: {1} adet", type.Key, type.Value));
                }

                // Kategori dağılımını logla
                var categoryDistribution = transactions
                    .GroupBy(t => t.CategoryID)
                    .ToDictionary(g => GetCategoryName(g.Key), g => g.Count());

                System.Diagnostics.Debug.WriteLine("Kategori dağılımı:");
                foreach (var category in categoryDistribution)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("- {0}: {1} adet", category.Key, category.Value));
                }

                System.Diagnostics.Debug.WriteLine(string.Format("GetTransactionHistory tamamlandı - Toplam {0} işlem", transactions.Count));
                return transactions;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("GetTransactionHistory hatası: {0}", ex.Message));
                System.Diagnostics.Debug.WriteLine(string.Format("Stack trace: {0}", ex.StackTrace));
                throw;
            }
        }
    }

    public class TransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public int CategoryID { get; set; }
    }

    public class PredictionResult
    {
        public decimal HoltWintersPrediction { get; set; }
        public decimal LinearRegressionPrediction { get; set; }
        public decimal FinalPrediction { get; set; }
    }

    public class CategoryPrediction
    {
        public string Category { get; set; }
        public decimal CurrentMonth { get; set; }
        public decimal PredictedAmount { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class SavingsRecommendation
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public decimal PotentialSaving { get; set; }
    }
}