<%@ Page Title="Harcamalarım" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FinansWebApp.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <div class="d-flex justify-content-between align-items-center">
                    <h2 class="mb-0">Geçmiş Harcamalar</h2>
                    <div class="form-group mb-0">
                        <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="form-control form-control-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                            <asp:ListItem Text="5 Kayıt" Value="5" />
                            <asp:ListItem Text="10 Kayıt" Value="10" Selected="True" />
                            <asp:ListItem Text="20 Kayıt" Value="20" />
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-12">
                <div class="card shadow-sm">
                    <div class="card-body p-0">
                        <asp:GridView ID="gvTransactions" runat="server" CssClass="table table-hover transaction-table mb-0"
                            AutoGenerateColumns="False" AllowPaging="True" PageSize="10"
                            OnPageIndexChanging="gvTransactions_PageIndexChanging">
                            <Columns>
                                <asp:TemplateField HeaderText="Tarih" ItemStyle-CssClass="align-middle">
                                    <ItemTemplate>
                                        <div class="d-flex align-items-center">
                                            <div class="transaction-date text-muted">
                                                <div class="date-day"><%# ((DateTime)Eval("TransactionDate")).ToString("dd") %></div>
                                                <div class="date-month"><%# ((DateTime)Eval("TransactionDate")).ToString("MMM") %></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Kategori" ItemStyle-CssClass="align-middle">
                                    <ItemTemplate>
                                        <span class="badge" style="background-color: rgba(220, 53, 69, 0.1); color: #dc3545;">
                                            <%# Eval("CategoryName") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="Description" HeaderText="Açıklama" ItemStyle-CssClass="align-middle" />

                                <asp:TemplateField HeaderText="Tutar" ItemStyle-CssClass="align-middle">
                                    <ItemTemplate>
                                        <span class='<%# Convert.ToString(Eval("TransactionType")) == "Gelir" ? "text-success" : "text-danger" %>'>
                                            <%# Convert.ToString(Eval("TransactionType")) == "Gelir" ? "+" : "-" %>₺<%# String.Format("{0:N2}", Eval("Amount")) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Bakiye" ItemStyle-CssClass="align-middle">
                                    <ItemTemplate>
                                        <strong>₺<%# String.Format("{0:N2}", Eval("Balance")) %></strong>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pagination-container" />
                            <HeaderStyle CssClass="bg-light" />
                            <EmptyDataTemplate>
                                <div class="text-center py-4">
                                    <i class="fas fa-receipt text-muted" style="font-size: 48px;"></i>
                                    <p class="mt-2 mb-0 text-muted">Henüz hiç işlem bulunmuyor.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        .transaction-table {
            font-size: 0.95rem;
        }

        .transaction-table th {
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.8rem;
            letter-spacing: 0.5px;
        }

        .transaction-date {
            text-align: center;
            line-height: 1.2;
        }

        .date-day {
            font-size: 1.1rem;
            font-weight: 600;
        }

        .date-month {
            font-size: 0.8rem;
            text-transform: uppercase;
        }

        .badge {
            padding: 0.5rem 0.8rem;
            font-weight: 500;
            font-size: 0.85rem;
        }

        .pagination-container {
            background-color: #f8f9fa;
            padding: 1rem;
        }

        .pagination-container table {
            margin: 0 auto;
        }

        .pagination-container a {
            padding: 0.5rem 0.8rem;
            margin: 0 0.2rem;
            border-radius: 4px;
            color: #dc3545;
            text-decoration: none;
            transition: all 0.2s;
        }

        .pagination-container a:hover {
            background-color: rgba(220, 53, 69, 0.1);
        }

        .pagination-container span {
            padding: 0.5rem 0.8rem;
            margin: 0 0.2rem;
            border-radius: 4px;
            background-color: #dc3545;
            color: white;
        }
    </style>
</asp:Content> 