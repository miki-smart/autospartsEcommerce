# Identity Server MVP Completion Checklist

## Account Confirmation Flow (To be completed with Notification Service)

- [ ] Implement email confirmation after registration (deferred)
- [ ] Prevent login until email is confirmed (deferred)

<!-- Email Service Integration excluded for MVP -->

## Rate Limiting

- [x] Add rate limiting to login endpoint
- [x] Add rate limiting to registration endpoint (optional)

## Password Validation Rules

- [x] Enforce password strength rules on registration
- [x] Enforce password strength rules on password reset

## Profile Management

- [x] Implement `PUT /api/users/profile` (update profile)
- [x] Implement change password functionality

## Login History

- [x] Implement `GET /api/users/login-history` (track and return login events)

## User Management (Admin)

- [x] Admin can search, filter, and paginate users
- [x] Admin can view user details and status
- [x] Admin can activate, deactivate, or lock user accounts
