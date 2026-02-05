FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Closure/Closure.csproj", "Closure/"]
RUN dotnet restore "Closure/Closure.csproj"
COPY . .
WORKDIR "/src/Closure"
RUN dotnet build "Closure.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Closure.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Closure.dll"]
