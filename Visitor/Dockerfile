FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Visitor/Visitor.csproj", "Visitor/"]
RUN dotnet restore "Visitor/Visitor.csproj"
COPY . .
WORKDIR "/src/Visitor"
RUN dotnet build "Visitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Visitor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Visitor.dll"]
