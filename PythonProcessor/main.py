import json
import os
from pyspark.sql import SparkSession, DataFrame, Row
from pyspark.sql.types import ShortType
from pyspark.sql.functions import explode, expr, element_at, column, array, col, to_json
from PIL import Image
import pandas as pd
from kafka import KafkaProducer
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
    print("Creating spark session", flush=True)
    spark = getSparkSession(config)

    # redImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    # nirImageDF: DataFrame = spark.read.format("image").load(
    #     config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"]),
    print("Opening Images", flush=True)
    redImage: Image.Image = Image.open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestRedImage"])
    nirImage: Image.Image = Image.open(config["hdfsImageIngestPath"] + "/" + config["hdfsImageIngestNirImage"])
    print("Opened Images", flush=True)

    height: int = redImage.height
    width: int = redImage.width
    print("Extracted Dimensions", flush=True)

    xOffset: float = (int(args["long2"]) - int(args["long1"])) / width
    yOffset: float = (int(args["lat2"]) - int(args["lat1"])) / height

    columns: list = ["red", "nir", "x", "y"]

    # Extracting pixels from images
    print("Extracting pixels from images", flush=True)
    isSingleLayer: bool = isinstance(redImage.getpixel((0, 0)), int)
    print(redImage.getpixel((0, 0)), flush=True)
    print("Image type is not multi layer: " + str(isSingleLayer), flush=True)
    nr_iterations: int = 1098
    kafka = KafkaProducer(bootstrap_servers=config["kafkaURL"])
    for iterations in range(nr_iterations):
        data: list = []
        print("Iteration " + str(iterations + 1), flush=True)
        # if not isSingleLayer:
        #     for y in range(((height // nr_iterations) * iterations), ((height // nr_iterations) * (iterations + 1))):
        #         if (y % 100 == 0):
        #             print("Pixel " + str(y))
        #         for x in range(0, width):
        #             data.append([redImage.getpixel((x, y))[0], nirImage.getpixel((x, y))[0], x, y])
        #
        #             # TODO: Generates type error: int object is not subscriptable Find out why
        # else:
        for y in range(((height // nr_iterations) * iterations), ((height // nr_iterations) * (iterations + 1))):
            if (y % 100 == 0):
                print("Pixel " + str(y), flush=True)
            for x in range(0, width):
                data.append([redImage.getpixel((x, y)), nirImage.getpixel((x, y)), x, y])
                # TODO: This might be a solution to above error
        print("Extracted Pixels", flush=True)
        # Create Dataframe
        print("Creating dataframe", flush=True)
        extractedPixels: pd.DataFrame = pd.DataFrame(data=data, columns=columns)

        sep = spark.createDataFrame(extractedPixels)
        print("Created Dataframe", flush=True)

        print("Generating ndvi", flush=True)
        combinedImageDF: DataFrame = generateNDVIPixels(sep, xOffset, yOffset) \
            .withColumn("combined", array(col("ndvi"), col("lattitudeTL"), col("lattitudeBR"), col("longtitudeTL"),
                                          col("longtitudeBR")))
        print("Created combined image dataframe", flush=True)
        # Export
        print("Exporting", flush=True)
        processedPixels: list = combinedImageDF.select(to_json(combinedImageDF.combined).alias("jsonValue")) \
            .selectExpr("CAST(jsonValue AS STRING)") \
            .toDF("jsonValue") \
            .collect()

        #     .write \
        #     .format("kafka") \
        #     .option("kafka.bootstrap.servers", config["kafkaURL"]) \
        #     .option("topic", config["ndviPixelIngestTopic"]) \
        #     .save()

        # processedPixels = combinedImageDF.collect()
        for pixel in processedPixels:
            kafka.send(config["ndviPixelIngestTopic"],  bytes(pixel['jsonValue'], 'utf-8'))



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
    Image.MAX_IMAGE_PIXELS = None
    #os.environ['PYSPARK_SUBMIT_ARGS'] = '--packages org.apache.spark:spark-sql-kafka-0-10_2.12:3.1.1,org.apache.kafka:kafka-clients:2.8.1 PythonProcessor.py'
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
