FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ./DataCollectorAndProcessor ./DataCollectorAndProcessor
COPY ./PythonProcessor ./PythonProcessor
RUN dotnet restore "./DataCollectorAndProcessor/Common/Common.csproj"
RUN dotnet restore "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj"
WORKDIR "/src/"
RUN dotnet build "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DataCollectorAndProcessor/DataCollector/DataCollector.csproj" -c Release -o /app/publish

FROM ghcr.io/hounsvad/sem7base
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/PythonProcessor/main.py ./PythonProcessor.py
COPY --from=build /src/PythonProcessor/img.py ./img.py
ENTRYPOINT ["dotnet", "DataCollector.dll"]