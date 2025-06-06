# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files for better layer caching
COPY AutoPartsEcommerce.sln ./
COPY src/Identity/Identity.API/*.csproj ./src/Identity/Identity.API/
COPY src/Identity/Identity.Application/*.csproj ./src/Identity/Identity.Application/
COPY src/Identity/Identity.Domain/*.csproj ./src/Identity/Identity.Domain/
COPY src/Identity/Identity.Infrastructure/*.csproj ./src/Identity/Identity.Infrastructure/
COPY src/Identity/Identity.Persistence/*.csproj ./src/Identity/Identity.Persistence/

# Restore dependencies
RUN dotnet restore src/Identity/Identity.API/Identity.API.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish src/Identity/Identity.API/Identity.API.csproj -c Release -o /out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install PostgreSQL client tools for health checks
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

# Create logs directory
RUN mkdir -p logs

# Expose port 80 for HTTP
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Identity.API.dll"]
