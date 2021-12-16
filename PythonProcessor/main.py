import json
import os
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import ShortType
from pyspark.sql.functions import explode, expr, element_at, column, array, col, to_json
from hdfs import InsecureClient
import sys


# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.
def getSparkSession(config: dict) -> SparkSession:
    spark = SparkSession.builder \
        .appName(config["sparkAppName"]) \
        .config("spark.master", config["sparkMasterURL"]) \
        .config("spark.executor.memory", config["sparkMaxMemory"]) \
        .config("spark.sql.streaming.checkpointLocation", config["sparkCheckpointLocationURL"]) \
        .getOrCreate()
    return spark


def generateNDVIPixels(redPixelDF: DataFrame, nirPixelDF: DataFrame, xOffset: float, yOffset: float):
    cond = [redPixelDF.x == nirPixelDF.x,
            redPixelDF.y == nirPixelDF.y,
            redPixelDF.height == nirPixelDF.height,
            redPixelDF.width == nirPixelDF.width, ]

    return redPixelDF.join(nirPixelDF, cond) \
        .withColumn("ndvi", expr("(nirIntensity-redIntensity)/(nirIntensity+redIntensity)")) \
        .withColumn("lattitudeTL", expr(f'{args["lat1"]}+({yOffset}*y)')) \
        .withColumn("lattitudeBR", expr(f'{args["lat1"]}+({yOffset}*y)')) \
        .withColumn("longtitudeTL", expr(f'{args["long1"]}+({xOffset}*(y+1))')) \
        .withColumn("longtitudeBR", expr(f'{args["long1"]}+({xOffset}*(y+1))'))


def extractToPixels(valueAlias: str, height: int, width: int, dataframe: DataFrame):
    return dataframe.withColumn("image.data", explode("image.data")) \
        .withColumn("y", range(0, height)) \
        .withColumn("image.data", explode("image.data")) \
        .withColumn("x", range(0, width * height)) \
        .withColumn("x", expr("x % image.width")) \
        .withColumn("value", column("image.data")[0]) \
        .toDF(column("x"), column("y"), column("value").alias(valueAlias), column("image.height").alias("height"),
              column("image.width").alias("width"))

def main(args: dict, config: dict):
    print("Entered main")
    spark = getSparkSession(config)

    client = InsecureClient("http://namenode:14000")
    redImageDFFile = open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    client.write(f'/img/{config["hdfsImageIngestRedImage"]}', redImageDFFile, overwrite=True)
    print("Uploaded red image")
    nirImageDFFile = open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"])
    client.write(f'/img/{config["hdfsImageIngestNirImage"]}', nirImageDFFile, overwrite=True)
    print("Uploaded nir image")

    # redImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    # nirImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"]),

    redImageDF: DataFrame = spark.read.format("image").load(
        "hdfs://namenode:9000/img/" + config["hdfsImageIngestRedImage"])
    print("loaded red image")
    nirImageDF: DataFrame = spark.read.format("image").load(
        "hdfs://namenode:9000/img/" + config["hdfsImageIngestNirImage"])
    print("loaded nir image")

    nirImageDF.show()
    print("showed nir")
    redImageDF.show()
    print("showed red")



    height: int = redImageDF.select("image.height").collect()[0]["image.height"]
    width: int = redImageDF.select("image.width").collect()[0]["image.width"]
    print("Gathered dimensions")

    xOffset: int = (args["long2"] - args["long1"]) / width
    yOffset: int = (args["lat2"] - args["lat1"]) / height
    print("Calculated offsets")


    redDF: DataFrame = extractToPixels("redIntensity", height, width, redImageDF)
    print(redDF.collect())
    nirDF: DataFrame = extractToPixels("nirIntensity", height, width, nirImageDF)
    print(redDF.collect())
    combinedImageDF: DataFrame = generateNDVIPixels(redDF, nirDF, xOffset, yOffset) \
        .withColumn("combined", array(col("ndvi"), col("lattitudeTL"), col("lattitudeBR"), col("longtitudeTL"),
                                      col("longtitudeBR")))
    print("Created combined image dataframe")
    # Export
    combinedImageDF.select(to_json(combinedImageDF.combined).alias("jsonValue")) \
        .selectExpr("CAST(jsonValue AS STRING)") \
        .writeStream \
        .format("kafka") \
        .option("kafka.bootstrap.servers", config["kafkaURL"]) \
        .option("topic", config["ndviPixelIngestTopic"]).outputMode("complete") \
        .start() \
        .awaitTermination()
    print("Sent to kafka")

def validateArgsContents(args: dict):
    requiredArgsFields: list = ["lat1", "lat2", "long1", "long2", "polygon"]
    for item in requiredArgsFields:
        if item not in args:
            print(f'Item:"{item}", is missing from the args json')
            exit(2)
    if len(args["polygon"]) < 3:
        print('Polygon field in args contained less than 3 items')
    for item in args["polygon"]:
        if "lat" not in item:
            print("lat missing from an entry in args.polygon")
            exit(4)
        if "long" not in item:
            print("long missing from an entry in args.polygon")


def validateConfigContents(config: dict):
    requiredAppsettingsFields: list = ["hdfsImageIngestPath", "hdfsImageIngestRedImage", "hdfsImageIngestNirImage",
                                       "kafkaURL", "ndviPixelIngestTopic", "sparkAppName", "sparkMasterURL",
                                       "sparkMaxMemory", "sparkCheckpointLocationURL"]
    for item in requiredAppsettingsFields:
        if item not in config:
            print(f'Item:"{item}", is missing from the appsettings.json file')
            exit(8)


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    print('Python doing stuff')
    # Validate Input
    if len(sys.argv) < 2:
        print("Needs json configuration")
        exit(1)

    # Extract Args As Json
    args: dict = json.loads(sys.argv[1])
    
    # Validate Args exists
    validateArgsContents(args)

    # Extract config values from appsettings
    config: dict = json.load(open(args["appsettings"]))

    # Validate Appsettings
    validateConfigContents(config)

    print('All is valid')
    main(args, config)
