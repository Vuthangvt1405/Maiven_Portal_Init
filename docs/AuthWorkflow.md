# Authentication workflow

## Endpoints

| Method | URL | Authentication | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Create a student account and return a JWT |
| `POST` | `/api/auth/login` | Anonymous | Authenticate any account with at least one active role and return a JWT |

## Student registration

```text
HTTP request
  -> AuthController.Register
  -> request DTO validation
  -> AuthService normalizes the email
  -> AuthRepository checks all users, including soft-deleted users
  -> password is hashed
  -> AuthRepository creates USERS + active STUDENT USER_ROLES
  -> JwtTokenService creates a 60-minute access token
  -> HTTP 201 AuthResponse
```

The client cannot choose a role. Registration always assigns the seeded `STUDENT` role.

## Login

```text
HTTP request
  -> AuthController.Login
  -> request DTO validation
  -> AuthService normalizes the email
  -> AuthRepository loads the account and active roles
  -> password hash is verified
  -> password hash is upgraded when required
  -> JwtTokenService creates a 60-minute access token
  -> HTTP 200 AuthResponse
```

Unknown email, wrong password, deleted user, and accounts without an active role all return the same `401` response.

## JWT usage

Send the returned token to protected endpoints with:

```http
Authorization: Bearer <access-token>
```

The token contains the user ID in `sub`, the email, a unique `jti`, and one `role` claim per active role.
