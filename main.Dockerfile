# Gebruik de officiële .NET 6 SDK image als basis voor build-stap
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Zet de werkdirectory in de container
WORKDIR /src

# Kopieer de csproj bestanden en herstel de dependencies
COPY ["DepotWebAPI/DepotWebAPI.csproj", "DepotWebAPI/"]

# Restore de NuGet dependencies
RUN dotnet restore "DepotWebAPI/DepotWebAPI.csproj"

# Kopieer alle bestanden naar de container
COPY . .

# Bouw de app
WORKDIR "/src/DepotWebAPI"
RUN dotnet build "DepotWebAPI.csproj" -c Release -o /app/build

# Publiceer de app
RUN dotnet publish "DepotWebAPI.csproj" -c Release -o /app/publish

# Gebruik een basis image voor de runtime (lichtere image)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
EXPOSE 80

# Kopieer de gepubliceerde bestanden naar de runtime image
COPY --from=build /app/publish .

# Stel het startcommando in
ENTRYPOINT ["dotnet", "DepotWebAPI.dll"]
