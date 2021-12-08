from flask import Flask, request, jsonify
import logging
from pyhive import hive
import traceback
import sys
conn = hive.Connection(host="hive-server", port=10000, username="hive", password="hive", auth='CUSTOM')
app = Flask(__name__)

@app.route("/ndvi", methods=["POST"])
def index():
    with conn.cursor() as cur:
        try:
            content = request.get_json(silent=True)
            longitude = int(content['longitude'])
            latitude = int(content['latitude'])
            cur.execute("select latestdate from latestdate")
            latestdate = cur.fetchone()[0]
            cur.execute("select value from ndvi where entrydate = %s and x1 <= %d and x2 >= %d and y1 <= %d and y2 >= %d limit 1", (latestdate, latitude, latitude, longitude, longitude))
            return str(cur.fetchone()[0])
        except:
            traceback.print_exc()
            return "", 404

# Plz no log GPS Cords request in stdout
app.logger.disabled = True
log = logging.getLogger('werkzeug')
log.disabled = True

app.run(host="0.0.0.0", port=80)