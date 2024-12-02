# Gebruik een .NET image als basis
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Gebruik een .NET SDK image voor de build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DepotWebAPI/DepotWebAPI.csproj", "DepotWebAPI/"]
RUN dotnet restore "DepotWebAPI/DepotWebAPI.csproj"
COPY . .
WORKDIR "/src/DepotWebAPI"
RUN dotnet build "DepotWebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DepotWebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DepotWebAPI.dll"]
