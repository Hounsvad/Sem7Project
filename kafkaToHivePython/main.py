from kafka import KafkaConsumer
from pyhive import hive
import os
import time
import json

if __name__ == '__main__':
    conn: hive.Connection = hive.Connection(host=os.environ.get("HIVE_HOSTNAME", "hive"), port=10000, username="hive",
                                            password="hive", auth='CUSTOM')
    consumer = KafkaConsumer("img", bootstrap_servers=['kafka:9092'],
                             value_deserializer=lambda m: json.loads(m.decode('utf-8')))

    for message in consumer:
        cur: hive.Cursor = conn.cursor()
        cur.excecute(
            f'INSERT INTO ndvi (`latestdate`, `value`, `x1`, `x2`, `y1`, `y2`) VALUES ({(time.time() * 100) // 1},'
            f' {message["ndvi"]}, {int(message["longtitudeTL"])}, {int(message["longtitudeBR"])},'
            f' {int(message["lattitudeTL"])}, {int(message["longtitudeBR"])})')
