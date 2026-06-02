# Ten sam wzorzec multi-stage build co PromptApi
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Contracts/Contracts.csproj Contracts/
COPY WorkerService/WorkerService.csproj WorkerService/
RUN dotnet restore WorkerService/WorkerService.csproj

COPY Contracts/ Contracts/
COPY WorkerService/ WorkerService/
RUN dotnet publish WorkerService/WorkerService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkerService.dll"]
