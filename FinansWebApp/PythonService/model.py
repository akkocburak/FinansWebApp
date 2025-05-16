import pandas as pd
import numpy as np
from statsmodels.tsa.holtwinters import ExponentialSmoothing
from sklearn.linear_model import LinearRegression
from datetime import datetime

def predict(df):
    try:
        print("Gelen veri:", df)  # Debug için veriyi yazdır
        
        # DataFrame'i düzelt
        if 'transactions' in df:
            # Liste içindeki dictionary'leri DataFrame'e dönüştür
            transactions_list = []
            for item in df['transactions']:
                if isinstance(item, dict):
                    transactions_list.append(item)
                elif isinstance(item, str):
                    # Eğer string ise, dictionary'e çevir
                    import ast
                    transactions_list.append(ast.literal_eval(item))
            
            df = pd.DataFrame(transactions_list)
        
        print("İşlenmiş veri:", df)  # Debug için işlenmiş veriyi yazdır
        
        # Tarihleri string'den datetime'a çevir
        df['TransactionDate'] = pd.to_datetime(df['TransactionDate'])
        print("Tarihler dönüştürüldü:", df['TransactionDate'].head())  # Debug mesajı
        
        # Sadece harcamaları al ve aylık topla
        expenses_df = df[df['TransactionType'] == 'Harcamalar']
        if expenses_df.empty:
            print("Harcama verisi bulunamadı")  # Debug mesajı
            return {
                'HoltWintersPrediction': None,
                'LinearRegressionPrediction': None,
                'FinalPrediction': 0
            }
            
        monthly = expenses_df.groupby(
            expenses_df['TransactionDate'].dt.to_period('M')
        )['Amount'].sum().sort_index()
        
        print("Aylık veriler:", monthly)  # Debug için aylık verileri yazdır
        
        # En az 3 ay veri kontrolü
        if len(monthly) < 3:
            print(f"Yetersiz veri: {len(monthly)} ay")  # Debug mesajı
            return {
                'HoltWintersPrediction': None,
                'LinearRegressionPrediction': None,
                'FinalPrediction': float(monthly.iloc[-1]) if len(monthly) > 0 else 0
            }

        # Holt-Winters tahmini
        hw_pred = None
        try:
            series = monthly.astype(float)  # Veri tipini float'a çevir
            series.index = series.index.to_timestamp()
            series.index.freq = 'MS'
            model = ExponentialSmoothing(series, trend='add', seasonal=None, initialization_method='estimated')
            fit = model.fit(smoothing_level=0.3, smoothing_trend=0.1, optimized=False)
            hw_pred = float(fit.forecast(1).iloc[0])
            print(f"Holt-Winters tahmini: {hw_pred}")  # Debug mesajı
        except Exception as e:
            print(f"Holt-Winters hata: {e}")

        # Linear Regression tahmini
        lr_pred = None
        try:
            X = np.arange(len(monthly)).reshape(-1, 1)
            y = monthly.values.astype(float)  # Veri tipini float'a çevir
            lr = LinearRegression()
            lr.fit(X, y)
            lr_pred = float(lr.predict(np.array([[len(monthly)]]))[0])
            print(f"Linear Regression tahmini: {lr_pred}")  # Debug mesajı
        except Exception as e:
            print(f"Linear Regression hata: {e}")

        # Final tahmin hesapla
        predictions = [p for p in [hw_pred, lr_pred] if p is not None]
        if predictions:
            final_pred = sum(predictions) / len(predictions)
        else:
            final_pred = float(monthly.iloc[-1]) if len(monthly) > 0 else 0
            
        print(f"Final tahmin: {final_pred}")  # Debug mesajı

        return {
            'HoltWintersPrediction': hw_pred,
            'LinearRegressionPrediction': lr_pred,
            'FinalPrediction': final_pred
        }
    except Exception as e:
        import traceback
        print(f"Genel hata: {str(e)}")
        print("Hata detayı:")
        print(traceback.format_exc())
        return {
            'HoltWintersPrediction': None,
            'LinearRegressionPrediction': None,
            'FinalPrediction': 0
        }
