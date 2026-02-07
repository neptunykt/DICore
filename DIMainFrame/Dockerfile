FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DIMainFrame/DIMainFrame.csproj", "DIMainFrame/"]
RUN dotnet restore "DIMainFrame/DIMainFrame.csproj"
COPY . .
WORKDIR "/src/DIMainFrame"
RUN dotnet build "DIMainFrame.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DIMainFrame.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DIMainFrame.dll"]
