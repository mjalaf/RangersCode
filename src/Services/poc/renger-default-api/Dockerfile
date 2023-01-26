FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 AS build
# Create a working directory
RUN mkdir -p /app

#ENV DOTNET_URLS=http://+:5203

WORKDIR /app

# Copy the project files
COPY . .

# Restore the project dependencies
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17 as runtime
WORKDIR /app
COPY --from=build /app/out/ /app

# Run the application
ENTRYPOINT ["dotnet", "/app/renger-default-api.dll"]

#FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 AS base
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 443

#FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 AS build
#WORKDIR /src
#COPY ["bci-cosmosdb/bci-cosmosdb.csproj", "bci-cosmosdb/"]
#RUN dotnet restore "bci-cosmosdb/bci-cosmosdb.csproj"
#COPY . .
#WORKDIR "/src/bci-cosmosdb"
#RUN dotnet build "bci-cosmosdb.csproj" -c Release