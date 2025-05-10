<%@ Page Title="Harcamalarım" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FinansWebApp.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

   
   
    <!-- Harcama Listesi -->
    <div class="container mt-4">
        <h4>Geçmiş Harcamalar</h4>
        <asp:GridView ID="gvTransactions" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="TransactionDate" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
                <asp:BoundField DataField="CategoryName" HeaderText="Kategori" />
                <asp:BoundField DataField="Description" HeaderText="Açıklama" />

                <asp:TemplateField HeaderText="Tutar (₺)">
                    <ItemTemplate>
                        <asp:Label ID="lblAmount" runat="server" 
                                   Text='<%# Eval("Amount", "{0:N2}") %>' 
                                   ForeColor='<%# Eval("TransactionType").ToString() == "Gelir" ? System.Drawing.Color.Green : System.Drawing.Color.Red %>'>
                        </asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="Balance" HeaderText="Bakiye (₺)" DataFormatString="{0:N2}" />
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>
