import pandas as pd
import numpy as np
from statsmodels.tsa.holtwinters import ExponentialSmoothing
from sklearn.linear_model import LinearRegression
from datetime import datetime

def prepare_dataframe(data):
    """Gelen veriyi DataFrame'e dönüştür"""
    try:
        print("Veri hazırlama başladı. Gelen veri:", data)
        
        if isinstance(data, dict) and 'transactions' in data:
            transactions_list = []
            for item in data['transactions']:
                if isinstance(item, dict):
                    transactions_list.append(item)
                elif isinstance(item, str):
                    import ast
                    transactions_list.append(ast.literal_eval(item))
            
            df = pd.DataFrame(transactions_list)
        else:
            df = pd.DataFrame(data)
            
        # Tarihleri dönüştür
        if 'TransactionDate' in df.columns:
            df['TransactionDate'] = pd.to_datetime(df['TransactionDate'])
            
        print("Hazırlanan DataFrame:", df.head())
        print("DataFrame sütunları:", df.columns.tolist())
        return df
    except Exception as e:
        print(f"Veri hazırlama hatası: {str(e)}")
        raise

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

def predict_category_expenses(data):
    try:
        print("Kategori tahmini başladı")
        df = prepare_dataframe(data)
        
        if df.empty:
            print("Veri bulunamadı")
            return []
            
        df = df[df['TransactionType'] == 'Harcamalar']
        if df.empty:
            print("Harcama verisi bulunamadı")
            return []

        print("İşlenecek veri:", df)

        current_month = df['TransactionDate'].max().replace(day=1)
        current_month_data = df[df['TransactionDate'].dt.to_period('M') == current_month.to_period('M')]

        predictions = []
        for category in df['CategoryName'].unique():
            category_data = df[df['CategoryName'] == category]
            print(f"Kategori {category} için veri sayısı: {len(category_data)}")
            
            if len(category_data) >= 3:
                monthly_expenses = category_data.groupby(
                    category_data['TransactionDate'].dt.to_period('M')
                )['Amount'].sum()

                if len(monthly_expenses) >= 3:
                    X = np.arange(len(monthly_expenses)).reshape(-1, 1)
                    y = monthly_expenses.values
                    
                    model = LinearRegression()
                    model.fit(X, y)
                    
                    next_month_prediction = float(model.predict([[len(monthly_expenses)]])[0])
                    current_month_amount = float(current_month_data[
                        current_month_data['CategoryName'] == category
                    ]['Amount'].sum())

                    change_percentage = ((next_month_prediction - current_month_amount) / current_month_amount * 100) if current_month_amount > 0 else 0

                    print(f"Kategori: {category}, Şu anki: {current_month_amount}, Tahmin: {next_month_prediction}")

                    predictions.append({
                        'Category': category,
                        'CurrentMonth': current_month_amount,
                        'PredictedAmount': next_month_prediction,
                        'ChangePercentage': float(change_percentage)
                    })

        print(f"Tahmin edilen kategori sayısı: {len(predictions)}")
        return predictions

    except Exception as e:
        import traceback
        print(f"Kategori tahmini hatası: {str(e)}")
        print("Hata detayı:")
        print(traceback.format_exc())
        return []

def generate_savings_recommendations(data):
    try:
        print("Tasarruf önerileri oluşturma başladı")
        df = prepare_dataframe(data)
        
        if df.empty:
            print("Veri bulunamadı")
            return []
            
        df = df[df['TransactionType'] == 'Harcamalar']
        if df.empty:
            print("Harcama verisi bulunamadı")
            return []

        print("İşlenecek veri:", df)

        recommendations = []
        recent_months = df[df['TransactionDate'] >= df['TransactionDate'].max() - pd.DateOffset(months=3)]
        print(f"Son 3 ay için veri sayısı: {len(recent_months)}")
        
        category_totals = recent_months.groupby('CategoryName')['Amount'].agg(['sum', 'mean', 'count']).reset_index()
        
        for _, category in category_totals.iterrows():
            category_name = category['CategoryName']
            monthly_avg = float(category['mean'])
            transaction_count = int(category['count'])
            
            print(f"Kategori: {category_name}, Aylık Ort: {monthly_avg}, İşlem Sayısı: {transaction_count}")
            
            if transaction_count >= 3:
                if monthly_avg > 1000:
                    potential_saving = monthly_avg * 0.2
                    recommendations.append({
                        'Title': f"{category_name} Harcamalarında Tasarruf",
                        'Description': f"Bu kategoride aylık ortalama {monthly_avg:.2f} TL harcıyorsunuz. "
                                     f"Harcamalarınızı %20 azaltarak önemli tasarruf sağlayabilirsiniz.",
                        'Icon': "fa-chart-line",
                        'PotentialSaving': float(potential_saving)
                    })
                elif transaction_count > 10 and monthly_avg < 500:
                    potential_saving = monthly_avg * 0.3
                    recommendations.append({
                        'Title': f"{category_name} İşlemlerini Optimize Et",
                        'Description': "Bu kategoride çok sayıda küçük harcama yapıyorsunuz. "
                                     "Toplu alımlar yaparak tasarruf sağlayabilirsiniz.",
                        'Icon': "fa-shopping-basket",
                        'PotentialSaving': float(potential_saving)
                    })

        total_monthly_avg = recent_months.groupby(
            recent_months['TransactionDate'].dt.to_period('M')
        )['Amount'].sum().mean()

        recommendations.append({
            'Title': "Aylık Bütçe Planı",
            'Description': f"Aylık ortalama {total_monthly_avg:.2f} TL harcıyorsunuz. "
                         "Bir bütçe planı oluşturarak harcamalarınızı daha iyi yönetebilirsiniz.",
            'Icon': "fa-piggy-bank",
            'PotentialSaving': float(total_monthly_avg * 0.15)
        })

        print(f"Oluşturulan öneri sayısı: {len(recommendations)}")
        return recommendations

    except Exception as e:
        import traceback
        print(f"Tasarruf önerileri hatası: {str(e)}")
        print("Hata detayı:")
        print(traceback.format_exc())
        return []
