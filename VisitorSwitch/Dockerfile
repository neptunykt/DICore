FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VisitorSwitch/VisitorSwitch.csproj", "VisitorSwitch/"]
RUN dotnet restore "VisitorSwitch/VisitorSwitch.csproj"
COPY . .
WORKDIR "/src/VisitorSwitch"
RUN dotnet build "VisitorSwitch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VisitorSwitch.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VisitorSwitch.dll"]
