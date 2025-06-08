# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution file and project files for better layer caching
COPY AutoPartsEcommerce.sln ./
COPY src/Gateway/ApiGateway/*.csproj ./src/Gateway/ApiGateway/

# Restore dependencies
RUN dotnet restore src/Gateway/ApiGateway/ApiGateway.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish src/Gateway/ApiGateway/ApiGateway.csproj -c Release -o /out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

# Create logs directory
RUN mkdir -p logs

# Expose port
EXPOSE 80

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
