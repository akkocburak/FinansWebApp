<%@ Page Title="Harcama Tahmini" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaTahmini.aspx.cs" Inherits="FinansWebApp.HarcamaTahmini" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <h2>Gelecek Ay Harcama Tahmini</h2>
                <p class="text-muted">Son 12 aylık harcama verilerinize dayanarak yapılan tahminler aşağıda gösterilmektedir.</p>
            </div>
        </div>
        
        <div class="row">
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
    </style>
</asp:Content>