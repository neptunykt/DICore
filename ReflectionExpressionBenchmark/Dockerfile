FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ReflectionExpressionBenchmark/ReflectionExpressionBenchmark.csproj", "ReflectionExpressionBenchmark/"]
RUN dotnet restore "ReflectionExpressionBenchmark/ReflectionExpressionBenchmark.csproj"
COPY . .
WORKDIR "/src/ReflectionExpressionBenchmark"
RUN dotnet build "ReflectionExpressionBenchmark.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReflectionExpressionBenchmark.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReflectionExpressionBenchmark.dll"]
