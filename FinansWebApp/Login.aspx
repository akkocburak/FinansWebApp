<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FinansWebApp.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Login</title>
    <!-- Bootstrap CDN -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-pzjw8f+ua7Kw1TIq0g0/6a7gG4EppOTYVRSYh9nff/7sq2zXip4YFaaLwJ-1J4Pz" crossorigin="anonymous">
    <style>
        .login-container {
            max-width: 400px;
            margin: 100px auto;
            background-color: #f8f9fa;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0px 0px 20px rgba(0, 0, 0, 0.1);
        }

        .login-header {
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            color: #333;
        }

        .btn-custom {
            background-color: #4CAF50;
            color: white;
            border-radius: 5px;
            width: 100%;
        }

        .btn-custom:hover {
            background-color: #45a049;
        }

        .form-group label {
            font-size: 14px;
        }

        .alert {
            font-size: 14px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
          
     <div class="login-container">
        <h3 class="login-header">Hesabınıza Giriş Yapın</h3>
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="alert alert-danger" Visible="false" />
        
        <div class="form-group">
            <label for="txtAccountNumber">Hesap Numarası:</label>
            <asp:TextBox ID="txtAccountNumber" runat="server" CssClass="form-control" placeholder="Hesap Numarasını Girin" />
        </div>
        
        <div class="form-group">
            <label for="txtPassword">Şifre:</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Şifrenizi Girin" />
        </div>
         <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
        <asp:Button ID="btnLogin" runat="server" Text="Giriş Yap" CssClass="btn btn-custom" OnClick="btnLogin_Click" />
    </div>

        </div>
    </form>
</body>
</html>
