from flask import Flask, request, jsonify
import pandas as pd
from model import predict

app = Flask(__name__)

@app.route('/predict', methods=['POST'])
def predict_route():
    try:
        # JSON'dan DataFrame oluştur
        data = request.get_json()
        df = pd.DataFrame(data)
        # Modelin predict fonksiyonunu çağır
        result = predict(df)
        return jsonify(result)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True)