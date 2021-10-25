from flask import Flask, request, jsonify
import logging
import pyhs2

conn = pyhs2.connect(host="hive", port=10000, username="hive", password="hive")
app = Flask(__name__)

@app.route("/ndvi", methods=["POST"])
def index():
    with conn.cursor() as cur:
        try:
            content = request.get_json(silent=True)
            longtitude = content['longtitude']
            latitude = content['latitude']
            cur.execute("select value from nvdi where date = (select latestDate from latestDate) and x1 <= ? and x2 >= ? and y1 <= ? and y2 >= ?,", (latitude, latitude, longtitude, longtitude))
            return cur.fetchone()
        except:
            return "", 404

# Plz no log GPS Cords request in stdout
app.logger.disabled = True
log = logging.getLogger('werkzeug')
log.disabled = True

app.run(host="0.0.0.0", port=80)