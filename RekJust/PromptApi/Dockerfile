# Etap 1: BUILD – kompilujemy aplikację
# Używamy oficjalnego obrazu SDK który zawiera kompilator .NET
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopiujemy pliki .csproj PRZED kodem źródłowym.
# Docker cache: jeśli .csproj nie zmienił się, `dotnet restore` jest pominięty.
# To przyspiesza buildy – restore jest wolny, kopiowanie kodu szybkie.
COPY Contracts/Contracts.csproj Contracts/
COPY PromptApi/PromptApi.csproj PromptApi/
RUN dotnet restore PromptApi/PromptApi.csproj

# Teraz kopiujemy kod źródłowy i budujemy
COPY Contracts/ Contracts/
COPY PromptApi/ PromptApi/
RUN dotnet publish PromptApi/PromptApi.csproj -c Release -o /app/publish

# Etap 2: RUNTIME – tylko środowisko uruchomieniowe, bez SDK
# Obraz runtime jest ~4x mniejszy niż SDK – nie zawiera kompilatora
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Kopiujemy tylko skompilowany output z etapu build
COPY --from=build /app/publish .

# Ustawiamy port i punkt wejścia
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PromptApi.dll"]
