<%@ Page Title="Harcama Tahmini" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaTahmini.aspx.cs" Inherits="FinansWebApp.HarcamaTahmini" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <h2>Gelecek Ay Harcama Tahmini</h2>
                <p class="text-muted">Son 12 aylık harcama verilerinize dayanarak yapılan tahminler aşağıda gösterilmektedir.</p>
            </div>
        </div>
        
        <!-- Genel Tahmin Kartları -->
        <div class="row mb-4">
            <div class="col-md-4">
                <div class="card h-100">
                    <div class="card-body">
                        <h5 class="card-title">Holt-Winters Tahmini</h5>
                        <div class="prediction-info">
                            <i class="fas fa-chart-line prediction-icon"></i>
                            <h3 class="prediction-amount">
                                <asp:Label ID="lblHoltWinters" runat="server" CssClass="text-primary" />
                            </h3>
                        </div>
                        <p class="text-muted small">
                            Zaman serisi analizi kullanılarak yapılan tahmin
                        </p>
                    </div>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="card h-100">
                    <div class="card-body">
                        <h5 class="card-title">Linear Regression Tahmini</h5>
                        <div class="prediction-info">
                            <i class="fas fa-chart-bar prediction-icon"></i>
                            <h3 class="prediction-amount">
                                <asp:Label ID="lblLinearRegression" runat="server" CssClass="text-success" />
                            </h3>
                        </div>
                        <p class="text-muted small">
                            Doğrusal regresyon analizi ile yapılan tahmin
                        </p>
                    </div>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="card h-100">
                    <div class="card-body">
                        <h5 class="card-title">Ortalama Tahmin</h5>
                        <div class="prediction-info">
                            <i class="fas fa-calculator prediction-icon"></i>
                        <h3 class="prediction-amount">
                                <asp:Label ID="lblFinalPrediction" runat="server" CssClass="text-danger" />
                        </h3>
                        </div>
                        <p class="text-muted small">
                            Her iki yöntemin ortalaması alınarak hesaplanan nihai tahmin
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Yapay Zeka Analiz Bölümü -->
        <div class="row mb-4">
            <div class="col-12">
                <h3 class="section-title">
                    <i class="fas fa-robot mr-2"></i>
                    Yapay Zeka Destekli Analiz
                </h3>
            </div>
        </div>

        <!-- Kategori Bazlı Tahminler -->
        <div class="row mb-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">
                            <i class="fas fa-tags text-info"></i>
                            Kategori Bazlı Tahminler
                        </h5>
                        <asp:Label ID="lblCategoryError" runat="server" CssClass="alert alert-warning d-block" Visible="false" />
                        <div class="table-responsive">
                            <asp:GridView ID="gvCategoryPredictions" runat="server" CssClass="table table-hover"
                                AutoGenerateColumns="False">
                                <Columns>
                                    <asp:BoundField DataField="Category" HeaderText="Kategori" />
                                    <asp:BoundField DataField="CurrentMonth" HeaderText="Bu Ay" DataFormatString="₺{0:N2}" />
                                    <asp:BoundField DataField="PredictedAmount" HeaderText="Gelecek Ay Tahmini" DataFormatString="₺{0:N2}" />
                                    <asp:TemplateField HeaderText="Değişim">
                                        <ItemTemplate>
                                            <span class='<%# Convert.ToDecimal(Eval("ChangePercentage")) > 0 ? "text-danger" : "text-success" %>'>
                                                <%# Convert.ToDecimal(Eval("ChangePercentage")) > 0 ? "+" : "" %><%# Eval("ChangePercentage", "{0:N1}") %>%
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tasarruf Önerileri -->
        <div class="row mb-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">
                            <i class="fas fa-piggy-bank text-success"></i>
                            Kişiselleştirilmiş Tasarruf Önerileri
                        </h5>
                        <asp:Label ID="lblRecommendationError" runat="server" CssClass="alert alert-warning d-block" Visible="false" />
                        <div class="savings-recommendations">
                            <asp:Repeater ID="rptSavingsRecommendations" runat="server">
                                <ItemTemplate>
                                    <div class="recommendation-item">
                                        <div class="recommendation-icon">
                                            <i class="fas <%# Eval("Icon") %>"></i>
                                        </div>
                                        <div class="recommendation-content">
                                            <h6><%# Eval("Title") %></h6>
                                            <p class="mb-0"><%# Eval("Description") %></p>
                                            <div class="potential-saving">
                                                Potansiyel Tasarruf: <strong class="text-success">₺<%# Eval("PotentialSaving", "{0:N2}") %></strong>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row mt-4">
            <div class="col-12">
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    Bu tahminler geçmiş harcama verilerinize dayanmaktadır ve sadece bilgilendirme amaçlıdır.
                </div>
            </div>
        </div>
    </div>

    <style>
        .prediction-amount {
            font-size: 2rem;
            font-weight: 600;
            margin: 1rem 0;
        }
        .card {
            margin-bottom: 20px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            transition: transform 0.2s;
        }
        .card:hover {
            transform: translateY(-5px);
        }
        .card-title {
            color: #495057;
            font-weight: 500;
            margin-bottom: 1.5rem;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .prediction-info {
            text-align: center;
            padding: 1rem 0;
        }
        .prediction-icon {
            font-size: 2rem;
            margin-bottom: 1rem;
            color: #6c757d;
        }
        .alert {
            border-radius: 8px;
        }
        .text-muted.small {
            font-size: 0.875rem;
        }
        .section-title {
            font-size: 1.5rem;
            margin-bottom: 1.5rem;
            color: #2d3436;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .recommendation-item {
            display: flex;
            align-items: flex-start;
            padding: 1rem;
            border-bottom: 1px solid #e9ecef;
            gap: 1rem;
        }
        .recommendation-item:last-child {
            border-bottom: none;
        }
        .recommendation-icon {
            color: #28a745;
            font-size: 1.5rem;
            padding-top: 0.25rem;
        }
        .recommendation-content h6 {
            margin-bottom: 0.25rem;
            color: #2d3436;
        }
        .potential-saving {
            margin-top: 0.5rem;
            font-size: 0.9rem;
        }
        .table th {
            font-weight: 600;
            background-color: #f8f9fa;
        }
    </style>
</asp:Content>