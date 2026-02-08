FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DICore3/DICore3.csproj", "DICore3/"]
RUN dotnet restore "DICore3/DICore3.csproj"
COPY . .
WORKDIR "/src/DICore3"
RUN dotnet build "DICore3.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DICore3.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DICore3.dll"]
