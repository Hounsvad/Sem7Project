from kafka import KafkaConsumer
from pyhive import hive
import os
import time
import json

if __name__ == '__main__':
    print("Creation Hive Connection", flush=True)
    conn: hive.Connection = hive.Connection(host=os.environ.get("HIVE_HOSTNAME", "hive"), port=10000, username="hive",
                                            password="hive", auth='CUSTOM')
    print("Creation consumer", flush=True)
    consumer = KafkaConsumer("img", bootstrap_servers=['kafka:9092'],
                             value_deserializer=lambda m: json.loads(m.decode('utf-8')))
    with conn.cursor() as cur:
        timestamp = 164059672265 #int((time.time() * 100) // 1)
        cur.execute('INSERT INTO latestdate values (%s)', (timestamp,))
        print("Staring the devouroring", flush=True)
        messageQueue: list = []
        for message in consumer:
            message_value = message.value
            #print(message)
            #print(message.value)
            if(str(message_value[0]) == 'None'):
                message_value[0] = 0
            messageQueue.append(f'({timestamp}, {message_value[0]}, {int(message_value[3])}, {int(message_value[4])}, {int(message_value[1])}, {int(message_value[2])})')
            if(len(messageQueue) != 1000):
                #print("At " + str(len(messageQueue)) + ", Continuing", flush=True)
                continue
            print("Getting ready to send", flush=True)
            values :str = ", ".join(messageQueue)
            messageQueue = []
            print("Reset message queue")
            message_value = message.value
            #print(message)
            #print(message.value)
            
            #cur.execute('DELETE FROM latestdate')
            
            # cur.execute(
            #     f'INSERT INTO ndvi (`entrydate`, `value`, `x1`, `x2`, `y1`, `y2`) VALUES ({timestamp},'
            #     f' {message_value[0]}, {int(message_value[3])}, {int(message_value[4])},'
            #     f' {int(message_value[1])}, {int(message_value[2])})')
            cur.execute(f'INSERT INTO ndvi (`entrydate`, `value`, `x1`, `x2`, `y1`, `y2`) VALUES ' + values)
