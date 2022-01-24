FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
RUN apt-get install git -y
RUN git clone https://github.com/Hounsvad/Sem7Project.git && cp ./Sem7Project/DataCollectorAndProcessor . -r && cp ./Sem7Project/PythonProcessor . -r && rm -r ./Sem7Project
RUN dotnet restore "./DataCollectorAndProcessor/Common/Common.csproj"
RUN dotnet restore "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj"
WORKDIR "/src/"
RUN dotnet build "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj" -c Release -o /app/publish

FROM base AS final
RUN apt-get update
RUN apt-get install -y openssh-client python3 python3-pip g++ libgraphicsmagick++1-dev libboost-python-dev openjdk-11-jdk
RUN pip3 install pyspark pgmagick
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/PythonProcessor/main.py ./PythonProcessor.py
COPY --from=build /src/PythonProcessor/img.py ./img.py
ENTRYPOINT ["dotnet", "DataCollector.dll"]