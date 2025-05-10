<%@ Page Title="Harcama Analizi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaAnalizi.aspx.cs" Inherits="FinansWebApp.HarcamaAnalizi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mt-4">
        <h4>Geçmiş Harcamalar</h4>
        <asp:GridView ID="gvTransactions" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="TransactionDate" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
                <asp:BoundField DataField="CategoryName" HeaderText="Kategori" />
                <asp:BoundField DataField="Amount" HeaderText="Tutar (₺)" DataFormatString="{0:N2}" />
            </Columns>
        </asp:GridView>

        <div class="mt-4">
            <h5>Gelecek Harcama Tahmini:</h5>
            <asp:Label ID="lblPredictedAmount" runat="server" CssClass="text-success"></asp:Label>
        </div>
    </div>

</asp:Content>
