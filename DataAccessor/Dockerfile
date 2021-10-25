FROM python
RUN apt-get update && apt-get install -y libsasl2-dev && rm -rf /var/lib/apt/lists/* 
COPY requirements.txt /tmp/requirements.txt
RUN pip install -r /tmp/requirements.txt
COPY ./src /app
WORKDIR /app
EXPOSE 80
CMD ["python3" , "app.py"]