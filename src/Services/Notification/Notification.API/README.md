# Notification Service

Handles user notifications (email, SMS, push) for the AutoParts Ecommerce platform.

## Features
- Send order, payment, and support notifications
- Supports email, SMS, and push channels
- Permission-based access control (see IdentityServer)

## How to Run

1. **Configure Notification Providers:**
   - Update `appsettings.Development.json` with SMTP, SMS, or push provider credentials.
2. **Run the Service:**
   - `dotnet run`

## Docker Support

### Build and Run
```powershell
cd src/Services/Notification/Notification.API
# docker build -t autoparts-notification .
# docker run -p 5006:80 --env-file .env autoparts-notification
```

### Environment Variables
- `SMTP_HOST`, `SMTP_USER`, `SMTP_PASS` (for email)
- `SMS_API_KEY` (for SMS)
- `ASPNETCORE_ENVIRONMENT`

## Configuration Required
- Notification provider credentials
- JWT settings for authentication

## Functionality
- Receives notification requests from other services
- Sends notifications to users via configured channels

## Notes
- Requires valid JWT with appropriate permissions (e.g., `notifications.send`).
- See IdentityServer for user/role/permission setup.
