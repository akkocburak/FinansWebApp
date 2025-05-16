import pandas as pd
import numpy as np
from statsmodels.tsa.holtwinters import ExponentialSmoothing
from sklearn.linear_model import LinearRegression

def predict(df):
    try:
        # Tarihleri datetime'a çevir
        df['TransactionDate'] = pd.to_datetime(df['TransactionDate'])
        # Sadece harcamaları al ve aylık topla
        monthly = df[df['TransactionType'] == 'Harcamalar'].groupby(
            df['TransactionDate'].dt.to_period('M')
        )['Amount'].sum().sort_index()
        # En az 3 ay veri kontrolü
        if len(monthly) < 3:
            return {
                'HoltWintersPrediction': None,
                'LinearRegressionPrediction': None,
                'FinalPrediction': float(monthly.iloc[-1]) if len(monthly) > 0 else None
            }

        # Holt-Winters tahmini
        try:
            series = monthly
            series.index = series.index.to_timestamp()
            series.index.freq = 'MS'
            model = ExponentialSmoothing(series, trend='add', seasonal=None, initialization_method='estimated')
            fit = model.fit(smoothing_level=0.3, smoothing_trend=0.1, optimized=False)
            hw_pred = float(fit.forecast(1).iloc[0])
        except Exception as e:
            print(f"Holt-Winters hata: {e}")
            hw_pred = None

        # Linear Regression tahmini
        try:
            X = np.arange(len(monthly)).reshape(-1, 1)
            y = monthly.values
            lr = LinearRegression()
            lr.fit(X, y)
            lr_pred = float(lr.predict(np.array([[len(monthly)]]))[0])
        except Exception as e:
            print(f"Linear Regression hata: {e}")
            lr_pred = None

        # FinalPrediction: İki tahminin ortalaması (veya biri yoksa diğeri)
        if hw_pred is not None and lr_pred is not None:
            final_pred = (hw_pred + lr_pred) / 2
        elif hw_pred is not None:
            final_pred = hw_pred
        elif lr_pred is not None:
            final_pred = lr_pred
        else:
            final_pred = float(monthly.iloc[-1])

        return {
            'HoltWintersPrediction': hw_pred,
            'LinearRegressionPrediction': lr_pred,
            'FinalPrediction': final_pred
        }
    except Exception as e:
        print(f"Genel hata: {e}")
        return {
            'HoltWintersPrediction': None,
            'LinearRegressionPrediction': None,
            'FinalPrediction': None
        }
