ARG Version=1.6.5-wackermann

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG Version
WORKDIR /src

COPY . .

RUN dotnet restore "src/BaGetter/BaGetter.csproj"

FROM build AS publish
ARG Version

RUN dotnet publish "src/BaGetter/BaGetter.csproj" \
    --configuration Release \
    --output /app \
    --no-restore \
    -p:Version=${Version} \
    -p:DebugType=none \
    -p:DebugSymbols=false \
    -p:GenerateDocumentationFile=false \
    -p:UseAppHost=false

RUN mkdir -p /data/packages /data/symbols /data/db

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-composite AS base

RUN apk add --no-cache icu-libs icu-data-full tzdata

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV Storage__Path="/data"
ENV Search__Type="Database"
ENV Database__Type="Sqlite"
ENV Database__ConnectionString="Data Source=/data/db/bagetter.db"

LABEL org.opencontainers.image.source="https://github.com/spreedated/BaGetter"

COPY --from=publish /data /data

WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "BaGetter.dll"]
