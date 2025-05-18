from flask import Flask, request, jsonify
from flask_cors import CORS
import pandas as pd
import logging
from model import predict, predict_category_expenses, generate_savings_recommendations

# Configure logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

app = Flask(__name__)
CORS(app)  # CORS desteği ekle

# UTF-8 encoding configuration
app.config['JSON_AS_ASCII'] = False
app.config['JSONIFY_MIMETYPE'] = 'application/json; charset=utf-8'

@app.route('/predict', methods=['POST'])
def predict_route():
    try:
        data = request.get_json(force=True)
        logger.debug(f"Received prediction request: {data}")
        
        if not data or 'transactions' not in data:
            return jsonify({'error': 'Invalid request data'}), 400
            
        df = pd.DataFrame(data['transactions'])
        result = predict(df)
        
        logger.debug(f"Prediction result: {result}")
        return jsonify(result)
    except Exception as e:
        logger.error(f"Error in prediction: {str(e)}", exc_info=True)
        return jsonify({'error': str(e)}), 500

@app.route('/predict_categories', methods=['POST'])
def predict_categories_route():
    try:
        logger.info("Kategori tahmini isteği alındı")
        data = request.get_json(force=True)
        logger.debug(f"Gelen veri: {data}")
        
        if not data:
            logger.error("Veri alınamadı")
            return jsonify({'error': 'Veri alınamadı'}), 400
            
        if 'transactions' not in data:
            logger.error("İşlem verisi bulunamadı")
            return jsonify({'error': 'İşlem verisi bulunamadı'}), 400
            
        transactions = data['transactions']
        if not transactions:
            logger.warning("Boş işlem listesi")
            return jsonify([])
            
        logger.info(f"İşlem sayısı: {len(transactions)}")
        result = predict_category_expenses(data)
        
        logger.info(f"Tahmin tamamlandı. Sonuç sayısı: {len(result)}")
        logger.debug(f"Sonuç: {result}")
        return jsonify(result)
    except Exception as e:
        logger.error(f"Kategori tahmini hatası: {str(e)}", exc_info=True)
        return jsonify({'error': str(e)}), 500

@app.route('/recommend', methods=['POST'])
def recommend_route():
    try:
        logger.info("Tasarruf önerileri isteği alındı")
        data = request.get_json(force=True)
        logger.debug(f"Gelen veri: {data}")
        
        if not data:
            logger.error("Veri alınamadı")
            return jsonify({'error': 'Veri alınamadı'}), 400
            
        if 'transactions' not in data:
            logger.error("İşlem verisi bulunamadı")
            return jsonify({'error': 'İşlem verisi bulunamadı'}), 400
            
        transactions = data['transactions']
        if not transactions:
            logger.warning("Boş işlem listesi")
            return jsonify([])
            
        logger.info(f"İşlem sayısı: {len(transactions)}")
        result = generate_savings_recommendations(data)
        
        logger.info(f"Öneriler oluşturuldu. Sonuç sayısı: {len(result)}")
        logger.debug(f"Sonuç: {result}")
        return jsonify(result)
    except Exception as e:
        logger.error(f"Tasarruf önerileri hatası: {str(e)}", exc_info=True)
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    logger.info("Flask uygulaması başlatılıyor...")
    app.run(debug=True, port=5000)