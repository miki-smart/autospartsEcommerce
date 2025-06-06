# Payment Service

Handles payment processing, refunds, and related financial operations for the AutoParts Ecommerce platform.

## Features
- Process payments for orders
- Handle refunds and payouts
- Integrates with external payment gateways
- Permission-based access control (see IdentityServer)

## How to Run

1. **Configure Database and Payment Gateway:**
   - Update `appsettings.Development.json` with your database connection string and payment gateway credentials.
2. **Run the Service:**
   - `dotnet run`

## Docker Support

### Build and Run
```powershell
cd src/Services/Payment/Payment.API
# docker build -t autoparts-payment .
# docker run -p 5005:80 --env-file .env autoparts-payment
```

### Environment Variables
- `ConnectionStrings__DefaultConnection`
- `PAYMENT_GATEWAY_API_KEY` (or similar)
- `ASPNETCORE_ENVIRONMENT`

## Configuration Required
- Database connection string
- Payment gateway credentials
- JWT settings for authentication

## Functionality
- Accepts payment requests from Orders service
- Processes and verifies payments
- Handles refunds and payout operations

## Notes
- Requires valid JWT with appropriate permissions (e.g., `payments.process`).
- See IdentityServer for user/role/permission setup.
