# Search Service

Provides search functionality across products, orders, and other entities in the AutoParts Ecommerce platform.

## Features
- Full-text search for products, orders, vendors, etc.
- Advanced filtering and sorting
- Permission-based access control (see IdentityServer)

## How to Run

1. **Configure Database and Search Engine:**
   - Update `appsettings.Development.json` with your database and search engine (e.g., Elasticsearch) connection strings.
2. **Run the Service:**
   - `dotnet run`

## Docker Support

### Build and Run
```powershell
cd src/Services/Search/Search.API
# docker build -t autoparts-search .
# docker run -p 5007:80 --env-file .env autoparts-search
```

### Environment Variables
- `ConnectionStrings__DefaultConnection`
- `SEARCH_ENGINE_URL`
- `ASPNETCORE_ENVIRONMENT`

## Configuration Required
- Database and search engine connection strings
- JWT settings for authentication

## Functionality
- Exposes search endpoints for frontend and other services
- Supports advanced queries and filters

## Notes
- Requires valid JWT with appropriate permissions (e.g., `search.read`).
- See IdentityServer for user/role/permission setup.
