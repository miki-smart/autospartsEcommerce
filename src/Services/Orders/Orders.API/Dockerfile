# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution file and project files for better layer caching
COPY AutoPartsEcommerce.sln ./
COPY src/Services/Orders/Orders.API/*.csproj ./src/Services/Orders/Orders.API/
COPY src/Services/Orders/Orders.Application/*.csproj ./src/Services/Orders/Orders.Application/
COPY src/Services/Orders/Orders.Domain/*.csproj ./src/Services/Orders/Orders.Domain/
COPY src/Services/Orders/Orders.Infrastructure/*.csproj ./src/Services/Orders/Orders.Infrastructure/
COPY src/Services/Orders/Orders.Persistence/*.csproj ./src/Services/Orders/Orders.Persistence/

# Restore dependencies
RUN dotnet restore src/Services/Orders/Orders.API/Orders.API.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish src/Services/Orders/Orders.API/Orders.API.csproj -c Release -o /out

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

ENTRYPOINT ["dotnet", "Orders.API.dll"]
