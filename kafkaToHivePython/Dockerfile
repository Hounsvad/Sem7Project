FROM python
WORKDIR /app
RUN pip install kafka-python
RUN pip install pyhive
COPY ./main.py /app/main.py
CMD python main.py