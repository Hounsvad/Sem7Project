FROM python
WORKDIR /app
RUN apt-get update && apt-get install -y sasl2-bin libsasl2-2 libsasl2-dev libsasl2-modules && rm -rf /var/lib/apt/lists/*
RUN pip install kafka-python
RUN pip install pyhive
RUN pip install sasl
RUN pip install thrift_sasl
RUN pip install thrift
COPY ./main.py /app/main.py
CMD python main.py