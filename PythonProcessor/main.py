import json
import os
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import ShortType
from pyspark.sql.functions import explode, expr, element_at, column, array, col, to_json
from hdfs import InsecureClient
from PIL import Image
import numby as np
import pandas as pd

import sys
from time import sleep


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


def generateNDVIPixels(df: DataFrame, xOffset: float, yOffset: float):

    return df.withColumn("ndvi", expr("(nir-red)/(nir+red)")) \
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
    print("Delaying 1 min", flush=True)
    spark = getSparkSession(config)


    # redImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    # nirImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"]),

    redImage: Image.Image = Image.open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    nirImage: Image.Image = Image.open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"])

    height: int = redImage.height
    width: int = redImage.width

    xOffset: int = (args["long2"] - args["long1"]) / width
    yOffset: int = (args["lat2"] - args["lat1"]) / height

    columns: list = ["red", "nir", "x", "y"]
    data: list = []

    #Extracting pixels from images
    print("Extracting pixels from images", flush=True)
    for y in range(0, height):
        for x in range(0, width):
            data.append([redImage.getpixel((x, y))[0], nirImage.getpixel((x, y))[0], x, y])

    #Create Dataframe
    print("Creating dataframe", flush=True)
    extractedPixels: pd.DataFrame = pd.DataFrame(data, columns)

    sep = spark.createDataFrame(extractedPixels)

    combinedImageDF: DataFrame = generateNDVIPixels(sep, xOffset, yOffset) \
        .withColumn("combined", array(col("ndvi"), col("lattitudeTL"), col("lattitudeBR"), col("longtitudeTL"),
                                      col("longtitudeBR")))
    print("Created combined image dataframe", flush=True)
    # Export
    combinedImageDF.select(to_json(combinedImageDF.combined).alias("jsonValue")) \
        .selectExpr("CAST(jsonValue AS STRING)") \
        .writeStream \
        .format("kafka") \
        .option("kafka.bootstrap.servers", config["kafkaURL"]) \
        .option("topic", config["ndviPixelIngestTopic"]).outputMode("complete") \
        .start() \
        .awaitTermination()
    print("Sent to kafka", flush=True)

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
    print('Python doing stuff', flush=True)
    # Validate Input
    if len(sys.argv) < 2:
        print("Needs json configuration", flush=True)
        exit(1)

    # Extract Args As Json
    args: dict = json.loads(sys.argv[1])
    
    # Validate Args exists
    validateArgsContents(args)

    # Extract config values from appsettings
    config: dict = json.load(open(args["appsettings"]))

    # Validate Appsettings
    validateConfigContents(config)

    print('All is valid', flush=True)
    main(args, config)
