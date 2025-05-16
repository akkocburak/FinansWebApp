import pandas as pd
import numpy as np
from sklearn.preprocessing import StandardScaler
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, LSTM

class ExpensePredictor:
    def __init__(self):
        self.model = self._build_model()
        self.scaler = StandardScaler()

    def _build_model(self):
        model = Sequential([
            LSTM(64, input_shape=(12, 1), return_sequences=True),
            LSTM(32),
            Dense(16, activation='relu'),
            Dense(1)
        ])
        model.compile(optimizer='adam', loss='mse')
        return model

    def prepare_data(self, df):
        try:
            # Veriyi hazırla
            df['TransactionDate'] = pd.to_datetime(df['TransactionDate'])
            monthly_expenses = df[df['TransactionType'] == 'Harcamalar'].groupby(
                df['TransactionDate'].dt.strftime('%Y-%m')
            )['Amount'].sum().reset_index()
            
            # Son 12 ay
            if len(monthly_expenses) < 13:
                raise ValueError("En az 13 aylık veri gerekli")
                
            values = monthly_expenses['Amount'].values.reshape(-1, 1)
            scaled = self.scaler.fit_transform(values)
            
            return scaled[-13:]  # Son 13 ay
        except Exception as e:
            print(f"Veri hazırlama hatası: {str(e)}")
            raise
        
    def predict(self, df):
        try:
            scaled_data = self.prepare_data(df)
            X = scaled_data[:12].reshape(1, 12, 1)
            pred_scaled = self.model.predict(X)
            return self.scaler.inverse_transform(pred_scaled)[0][0]
        except Exception as e:
            print(f"Tahmin hatası: {str(e)}")
            raise