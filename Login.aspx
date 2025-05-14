<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FinansWebApp.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Login</title>
    <!-- Bootstrap CDN -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet">
    <!-- Font Awesome -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet">
    <style>
        body {
            background: linear-gradient(135deg, #dc3545 0%, #8b0000 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .login-container {
            max-width: 400px;
            width: 90%;
            background-color: rgba(255, 255, 255, 0.95);
            padding: 40px;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        }

        .login-header {
            text-align: center;
            font-size: 28px;
            font-weight: bold;
            color: #dc3545;
            margin-bottom: 30px;
        }

        .form-group {
            margin-bottom: 25px;
        }

        .form-group label {
            font-size: 14px;
            color: #666;
            margin-bottom: 8px;
        }

        .form-control {
            border: 2px solid #ced4da;
            border-radius: 8px;
            padding: 12px 15px;
            transition: all 0.3s ease;
        }

        .form-control:focus {
            border-color: #dc3545;
            box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25);
        }

        .btn-custom {
            background-color: #dc3545;
            color: white;
            border: none;
            border-radius: 8px;
            padding: 12px;
            font-size: 16px;
            font-weight: 600;
            width: 100%;
            transition: all 0.3s ease;
        }

        .btn-custom:hover {
            background-color: #c82333;
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(220, 53, 69, 0.3);
        }

        .input-icon {
            position: relative;
        }

        .input-icon i {
            position: absolute;
            left: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #666;
        }

        .input-icon .form-control {
            padding-left: 40px;
        }

        .alert {
            border-radius: 8px;
            font-size: 14px;
            padding: 12px 15px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <h3 class="login-header">
                <i class="fas fa-lock mr-2"></i>
                Hesap Girişi
            </h3>
            
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="alert alert-danger d-block mb-4" Visible="false" />
            
            <div class="form-group">
                <label for="txtAccountNumber">Hesap Numarası</label>
                <div class="input-icon">
                    <i class="fas fa-user"></i>
                    <asp:TextBox ID="txtAccountNumber" runat="server" CssClass="form-control" placeholder="Hesap numaranızı girin" />
                </div>
            </div>
            
            <div class="form-group">
                <label for="txtPassword">Şifre</label>
                <div class="input-icon">
                    <i class="fas fa-key"></i>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Şifrenizi girin" />
                </div>
            </div>

            <asp:Label ID="Label1" runat="server" CssClass="text-danger mb-3 d-block" Text=""></asp:Label>
            
            <asp:Button ID="btnLogin" runat="server" Text="Giriş Yap" CssClass="btn btn-custom" OnClick="btnLogin_Click" />
        </div>
    </form>

    <!-- Bootstrap JS ve dependencies -->
    <script src="https://code.jquery.com/jquery-3.5.1.slim.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.5.4/dist/umd/popper.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
</body>
</html> 