from flask import Flask, request, jsonify
from flask_cors import CORS
from model import ExpensePredictor
import pandas as pd

app = Flask(__name__)
CORS(app)  # CORS desteği ekle
predictor = ExpensePredictor()

@app.route('/predict', methods=['POST'])
def predict():
    try:
        data = request.json
        transactions = pd.DataFrame(data['transactions'])
        prediction = predictor.predict(transactions)
        return jsonify({
            'predicted_amount': float(prediction),
            'status': 'success'
        })
    except Exception as e:
        return jsonify({
            'error': str(e),
            'status': 'error'
        }), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'healthy'})

if __name__ == '__main__':
    app.run(port=5000, debug=True)