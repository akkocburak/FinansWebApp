<%@ Page Title="Harcama Analizi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HarcamaAnalizi.aspx.cs" Inherits="FinansWebApp.HarcamaAnalizi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row mb-4">
            <div class="col-12">
                <h2>Aylık Harcama Analizi</h2>
            </div>
        </div>

        <div class="row">
            <div class="col-md-8">
                <div class="card">
                    <div class="card-body">
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

        <div class="row mt-4">
            <div class="col-12">
                <asp:Literal ID="ltrlHarcamaVerileri" runat="server"></asp:Literal>
            </div>
        </div>
    </div>

    <!-- Chart.js kütüphanesi -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <script type="text/javascript">
        function grafikOlustur(aylar, harcamalar, kategoriler, kategoriHarcamalari) {
            // Çizgi grafiği
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
                                callback: function(value) {
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
                                label: function(context) {
                                    return '₺' + context.parsed.y.toLocaleString('tr-TR');
                                }
                            }
                        }
                    }
                }
            });

            // Pasta grafiği
            var ctxPie = document.getElementById('kategoriGrafigi').getContext('2d');
            new Chart(ctxPie, {
                type: 'doughnut',
                data: {
                    labels: kategoriler,
                    datasets: [{
                        data: kategoriHarcamalari,
                        backgroundColor: [
                            'rgba(220, 53, 69, 0.8)',
                            'rgba(220, 53, 69, 0.6)',
                            'rgba(220, 53, 69, 0.4)',
                            'rgba(220, 53, 69, 0.2)',
                            'rgba(108, 117, 125, 0.8)',
                            'rgba(108, 117, 125, 0.6)',
                            'rgba(108, 117, 125, 0.4)',
                            'rgba(108, 117, 125, 0.2)'
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
                                font: {
                                    size: 12
                                }
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
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
        }
    </script>
</asp:Content> 