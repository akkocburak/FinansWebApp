<%@ Page Title="Harcama Analizi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaAnalizi.aspx.cs" Inherits="FinansWebApp.HarcamaAnalizi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <div class="d-flex justify-content-between align-items-center">
                    <h2>Finansal Analiz Paneli</h2>
                    <div class="date-selector">
                        <asp:DropDownList ID="ddlAySecimi" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlAySecimi_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="selected-date mt-2">
                    <span class="text-muted">Seçili Dönem: </span>
                    <asp:Label ID="lblSeciliDonem" runat="server" CssClass="fw-bold"></asp:Label>
                </div>
            </div>
        </div>

        <!-- Özet Kartları -->
        <div class="row mb-4">
            <div class="col-md-4">
                <div class="card summary-card">
                    <div class="card-body">
                        <h5 class="card-title text-primary">
                            <i class="fas fa-wallet"></i> Toplam Gelir
                        </h5>
                        <h3 class="mb-0">
                            <asp:Label ID="lblToplamGelir" runat="server" CssClass="amount-text" />
                        </h3>
                        <small class="text-muted">Bu ay</small>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card summary-card">
                    <div class="card-body">
                        <h5 class="card-title text-danger">
                            <i class="fas fa-shopping-cart"></i> Toplam Gider
                        </h5>
                        <h3 class="mb-0">
                            <asp:Label ID="lblToplamGider" runat="server" CssClass="amount-text" />
                        </h3>
                        <small class="text-muted">Bu ay</small>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card summary-card">
                    <div class="card-body">
                        <h5 class="card-title text-success">
                            <i class="fas fa-piggy-bank"></i> Net Durum
                        </h5>
                        <h3 class="mb-0">
                            <asp:Label ID="lblNetDurum" runat="server" CssClass="amount-text" />
                        </h3>
                        <small class="text-muted">Bu ay</small>
                    </div>
                </div>
            </div>
        </div>

        <!-- Ana Grafikler -->
        <div class="row mb-4">
            <div class="col-md-8">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Aylık Harcama Trendi</h5>
                        <canvas id="harcamaGrafigi"></canvas>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Kategori Dağılımı</h5>
                        <canvas id="kategoriGrafigi"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <!-- Yıllık Trend -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Yıllık Trend Analizi</h5>
                        <canvas id="yillikTrendGrafigi"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <div class="row mt-4">
            <div class="col-12">
                <asp:Literal ID="ltrlHarcamaVerileri" runat="server"></asp:Literal>
            </div>
        </div>
    </div>

    <!-- Chart.js kütüphanesi -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style>
        .summary-card {
            transition: transform 0.2s;
            border: none;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .summary-card:hover {
            transform: translateY(-5px);
        }
        .amount-text {
            font-size: 1.8rem;
            font-weight: 600;
        }
        .card-title {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 1rem;
        }
        .card-title i {
            font-size: 1.2rem;
        }
    </style>

    <script type="text/javascript">
        function grafikOlustur(aylar, harcamalar, kategoriler, kategoriHarcamalari, yillikVeriler, gelirGiderVerileri) {
            // Aylık harcama grafiği
            var ctxLine = document.getElementById('harcamaGrafigi').getContext('2d');
            new Chart(ctxLine, {
                type: 'line',
                data: {
                    labels: aylar,
                    datasets: [{
                        label: 'Aylık Toplam Harcama (₺)',
                        data: harcamalar,
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        borderColor: 'rgba(220, 53, 69, 1)',
                        borderWidth: 3,
                        tension: 0.4,
                        fill: true,
                        pointBackgroundColor: 'rgba(220, 53, 69, 1)',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 5,
                        pointHoverRadius: 7
                    }]
                },
                options: {
                    responsive: true,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) {
                                    return '₺' + value.toLocaleString('tr-TR');
                                }
                            },
                            grid: {
                                color: 'rgba(0, 0, 0, 0.1)'
                            }
                        },
                        x: {
                            grid: {
                                display: false
                            }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return '₺' + context.parsed.y.toLocaleString('tr-TR');
                                }
                            }
                        }
                    }
                }
            });

            // Kategori dağılımı grafiği
            var ctxPie = document.getElementById('kategoriGrafigi').getContext('2d');
            new Chart(ctxPie, {
                type: 'doughnut',
                data: {
                    labels: kategoriler,
                    datasets: [{
                        data: kategoriHarcamalari,
                        backgroundColor: [
                            '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFBE0B',
                            '#9B5DE5', '#00BBF9', '#00F5D4', '#7209B7', '#3A86FF',
                            '#38B000', '#F15BB5'
                        ],
                        borderColor: 'white',
                        borderWidth: 2
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                font: { size: 12 }
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    var value = context.raw;
                                    var percentage = ((value / total) * 100).toFixed(1);
                                    return context.label + ': ₺' + value.toLocaleString('tr-TR') + ' (' + percentage + '%)';
                                }
                            }
                        }
                    }
                }
            });

            // Yıllık trend grafiği
            var ctxYillik = document.getElementById('yillikTrendGrafigi').getContext('2d');
            new Chart(ctxYillik, {
                type: 'line',
                data: {
                    labels: yillikVeriler.labels,
                    datasets: [{
                        label: 'Gelir',
                        data: yillikVeriler.gelirler,
                        borderColor: '#38B000',
                        backgroundColor: 'rgba(56, 176, 0, 0.1)',
                        fill: true
                    },
                    {
                        label: 'Gider',
                        data: yillikVeriler.giderler,
                        borderColor: '#DC3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) {
                                    return '₺' + value.toLocaleString('tr-TR');
                                }
                            }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.dataset.label + ': ₺' + context.parsed.y.toLocaleString('tr-TR');
                                }
                            }
                        }
                    }
                }
            });
        }
    </script>
</asp:Content> 