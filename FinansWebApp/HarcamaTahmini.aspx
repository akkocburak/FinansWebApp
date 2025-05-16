<%@ Page Title="Harcama Tahmini" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaTahmini.aspx.cs" Inherits="FinansWebApp.HarcamaTahmini" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <h2>Gelecek Ay Harcama Tahmini</h2>
            </div>
        </div>
        
        <div class="row">
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Holt-Winters Tahmini</h5>
                        <h3 class="prediction-amount">
                            <asp:Label ID="lblHoltWinters" runat="server" CssClass="text-primary" />
                        </h3>
                    </div>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Linear Regression Tahmini</h5>
                        <h3 class="prediction-amount">
                            <asp:Label ID="lblLinearRegression" runat="server" CssClass="text-success" />
                        </h3>
                    </div>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Ortalama Tahmin</h5>
                        <h3 class="prediction-amount">
                            <asp:Label ID="lblFinalPrediction" runat="server" CssClass="text-danger" />
                        </h3>
                        <p class="text-muted">
                            <small>Son 12 aylık harcama verilerine dayanarak yapılan tahmin</small>
                        </p>
                        <div class="mt-3">
                            <asp:Button ID="btnPredict" runat="server" Text="Tahmin Yap" 
                                      CssClass="btn btn-primary" OnClick="btnPredict_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        .prediction-amount {
            font-size: 2rem;
            font-weight: 600;
            margin: 1.5rem 0;
        }
        .card {
            margin-bottom: 20px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
        }
        .card-title {
            color: #495057;
            font-weight: 500;
        }
    </style>
</asp:Content>