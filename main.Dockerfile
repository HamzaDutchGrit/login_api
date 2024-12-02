# Use .NET 8.0 SDK as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore any dependencies (via dotnet restore)
COPY DepotWebAPI/DepotWebAPI.csproj DepotWebAPI/
RUN dotnet restore "DepotWebAPI/DepotWebAPI.csproj"

# Copy the rest of the source code and build the project
COPY . .
WORKDIR /src/DepotWebAPI
RUN dotnet build "DepotWebAPI.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "DepotWebAPI.csproj" -c Release -o /app/publish

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the build output from the build container
COPY --from=build /app/publish .

# Set the entrypoint to your app
ENTRYPOINT ["dotnet", "DepotWebAPI.dll"]
