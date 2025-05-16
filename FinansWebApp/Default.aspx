<%@ Page Title="Harcamalarım" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FinansWebApp.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Bakiye Kartları -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="balance-wrapper">
                <div class="balance-card main-balance">
                    <div class="balance-icon">
                        <i class="fas fa-wallet"></i>
                    </div>
                    <div class="balance-info">
                        <h6>Toplam Bakiye</h6>
                        <h3 class="balance-amount">
                            <asp:Label ID="lblTotalBalance" runat="server" />
                        </h3>
                    </div>
                </div>

                <div class="balance-card income">
                    <div class="balance-icon">
                        <i class="fas fa-arrow-up"></i>
                    </div>
                    <div class="balance-info">
                        <h6>Bu Ay Gelir</h6>
                        <h3 class="balance-amount">
                            <asp:Label ID="lblMonthlyIncome" runat="server" />
                        </h3>
                        <div class="balance-change positive">
                            <i class="fas fa-chart-line"></i>
                            <asp:Label ID="lblIncomeChange" runat="server" />
                        </div>
                    </div>
                </div>

                <div class="balance-card expense">
                    <div class="balance-icon">
                        <i class="fas fa-arrow-down"></i>
                    </div>
                    <div class="balance-info">
                        <h6>Bu Ay Gider</h6>
                        <h3 class="balance-amount">
                            <asp:Label ID="lblMonthlyExpense" runat="server" />
                        </h3>
                        <div class="balance-change negative">
                            <i class="fas fa-chart-line"></i>
                            <asp:Label ID="lblExpenseChange" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

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
 
                      


                                <asp:BoundField DataField="Description" HeaderText="Açıklama" ItemStyle-CssClass="align-middle" />

                                <asp:TemplateField HeaderText="Tutar" ItemStyle-CssClass="align-middle">
                                    <ItemTemplate>
                                        <span class='<%# Convert.ToString(Eval("TransactionType")) == "Gelir" ? "text-success" : "text-danger" %>'>
                                            <%# Convert.ToString(Eval("TransactionType")) == "Gelir" ? "+" : "-" %>₺<%# String.Format("{0:N2}", Eval("Amount")) %>
                                        </span>
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

        .balance-wrapper {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 1.5rem;
            margin-bottom: 2rem;
        }

        .balance-card {
            background: white;
            border-radius: 15px;
            padding: 1.5rem;
            display: flex;
            align-items: center;
            gap: 1.5rem;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }

        .balance-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(45deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0) 100%);
            z-index: 1;
        }

        .balance-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
        }

        .balance-icon {
            width: 60px;
            height: 60px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            flex-shrink: 0;
        }

        .main-balance .balance-icon {
            background: linear-gradient(135deg, #dc3545 0%, #8b0000 100%);
            color: white;
        }

        .income .balance-icon {
            background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%);
            color: white;
        }

        .expense .balance-icon {
            background: linear-gradient(135deg, #dc3545 0%, #8b0000 100%);
            color: white;
        }

        .balance-info {
            flex-grow: 1;
        }

        .balance-info h6 {
            color: #6c757d;
            font-size: 0.9rem;
            margin-bottom: 0.5rem;
            font-weight: 500;
        }

        .balance-amount {
            font-size: 1.8rem;
            font-weight: 600;
            color: #2d3436;
            margin-bottom: 0.5rem;
        }

        .main-balance .balance-amount {
            color: #dc3545;
        }

        .balance-change {
            font-size: 0.9rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .balance-change.positive {
            color: #28a745;
        }

        .balance-change.negative {
            color: #dc3545;
        }

        .balance-change i {
            font-size: 0.8rem;
        }

        @media (max-width: 768px) {
            .balance-wrapper {
                grid-template-columns: 1fr;
            }

            .balance-card {
                padding: 1.2rem;
            }

            .balance-icon {
                width: 50px;
                height: 50px;
                font-size: 20px;
            }

            .balance-amount {
                font-size: 1.5rem;
            }
        }
    </style>
</asp:Content> 