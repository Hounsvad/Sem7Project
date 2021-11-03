import json
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import ShortType
from pyspark.sql.functions import explode, expr, element_at, column, array, col, to_json
import sys


# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.
def getSparkSession(config: dict) -> SparkSession:
    spark = SparkSession.builder \
        .appName(config["sparkAppName"]) \
        .config("spark.master", config["sparkMasterURL"]) \
        .config("spark.executor.memory", config["SparkMaxMemory"]) \
        .config("spark.sql.streaming.checkpointLocation", config["sparkCheckpointLocationURL"]) \
        .getOrCreate()
    return spark

# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    if (len(sys.argv) < 1):
        print("Needs json configuration")
        exit(-1)
    args: dict = json.loads(sys.argv[0])
    # TODO: Insert input validation
    # if not ("appsetting" in args and "" ):
    #    args

    config: dict = json.load(open("./appsettings.json"))

    spark = getSparkSession(config)

    redImageDF: DataFrame = spark.read.format("image").load(
        config["hdfsImageIngestPath"] + config["hdfsImageIngestRedImage"])
    nirImageDF: DataFrame = spark.read.format("image").load(
        config["hdfsImageIngestPath"] + config["hdfsImageIngestNirImage"])

    height: int = redImageDF.select("image.height").collect()[0]["image.height"]
    width: int = redImageDF.select("image.width").collect()[0]["image.width"]

    xOffset: int = (args["long2"] - args["long1"]) / width
    yOffset: int = (args["lat2"] - args["lat1"]) / height

    redDF: DataFrame = redImageDF.withColumn("image.data", explode("image.data")) \
        .withColumn("y", range(0, height)) \
        .withColumn("image.data", explode("image.data")) \
        .withColumn("x", range(0, width * height)) \
        .withColumn("x", expr("x % image.width")) \
        .withColumn("value", column("image.data")[0]) \
        .toDF(column("x"), column("y"), column("value").alias("redIntensity"), column("image.height").alias("height"),
              column("image.width").alias("width"))

    nirDF: DataFrame = nirImageDF.withColumn("image.data", explode("image.data")) \
        .withColumn("y", range(0, height)) \
        .withColumn("image.data", explode("image.data")) \
        .withColumn("x", range(0, width * height)) \
        .withColumn("x", expr("x % image.width")) \
        .withColumn("value", column("image.data")[0]) \
        .toDF(column("x"), column("y"), column("value").alias("nirIntensity"), column("image.height").alias("height"),
              column("image.width").alias("width"))

    cond = [redDF.x == nirDF.x, redDF.y == nirDF.y, redDF.height == nirDF.height, redDF.width == nirDF.width, ]
    combinedImageDF: DataFrame = redDF.join(nirDF, cond) \
        .withColumn("ndvi", expr("(nirIntensity-redIntensity)/(nirIntensity+redIntensity)")) \
        .withColumn("lattitudeTL", expr(f'{args["lat1"]}+({yOffset}*y)')) \
        .withColumn("lattitudeBR", expr(f'{args["lat1"]}+({yOffset}*y)')) \
        .withColumn("longtitudeTL", expr(f'{args["long1"]}+({xOffset}*(y+1))')) \
        .withColumn("longtitudeBR", expr(f'{args["long1"]}+({xOffset}*(y+1))')) \
        .withColumn("combined", array(col("ndvi"), col("lattitudeTL"), col("lattitudeBR"), col("longtitudeTL"),
                                      col("longtitudeBR")))

    # Export
    combinedImageDF.select(to_json(combinedImageDF.combined).alias("jsonValue")) \
        .selectExpr("CAST(jsonValue AS STRING)") \
        .writeStream \
        .format("kafka") \
        .option("kafka.bootstrap.servers", config["kafkaURL"])\
        .option("topic", config["ndviPixelIngestTopic"]).outputMode("complete")\
        .start()\
        .awaitTermination()
