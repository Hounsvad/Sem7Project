FROM python
COPY requirements.txt /tmp/requirements.txt
RUN pip install -r /tmp/requirements.txt
COPY ./src /app
WORKDIR /app
EXPOSE 80
CMD ["python3" , "app.py"]