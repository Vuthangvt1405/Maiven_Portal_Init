# Authentication workflow

## Endpoints

| Method | URL | Authentication | Purpose |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Create a student account and return a JWT |
| `POST` | `/api/auth/login` | Anonymous | Authenticate an active Student or Teacher account and return a JWT |
| `POST` | `/api/auth/admin/login` | Anonymous | Authenticate an account with an active `ADMIN` role and return a JWT |

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

Unknown email, wrong password, deleted user, accounts without an active role, and Admin accounts all return the same `401` response from the regular login endpoint.

## Admin bootstrap and login

Applying the complete EF Core migration set automatically ensures the following default administrator exists:

- Email: `admin@example.com`
- Temporary password: `123456`
- Display name: `System Administrator`

The migration stores an ASP.NET Core Identity password hash rather than the plaintext password. If the email already exists, its profile and password are preserved and the active `ADMIN` role is ensured. Change the temporary password before using the account outside development.

```text
HTTP POST /api/auth/admin/login
  -> request DTO validation
  -> AuthService verifies the email and password
  -> AuthService requires an active ADMIN role
  -> JwtTokenService creates an access token containing the ADMIN role
  -> HTTP 200 AuthResponse
```

A non-admin account receives the same generic `401` response as invalid credentials. Admin accounts also receive `401` from the regular `/api/auth/login` endpoint and must use `/api/auth/admin/login`. There is no admin registration endpoint; `/api/auth/register` always creates a student account.

## JWT usage

Send the returned token to protected endpoints with:

```http
Authorization: Bearer <access-token>
```

Each user has exactly one active role assignment. A filtered unique database index enforces this rule while allowing soft-deleted assignment history.

Authentication responses expose the assignment as a singular role and its `USER_ROLES.id`:

```json
{
  "role": "STUDENT",
  "roleUserId": 42
}
```

The `roles` array is no longer part of authentication or current-user responses. `roleUserId` is the exact `USER_ROLES.id`; the migration-seeded administrator keeps its reserved ID of `-1`, while normally created assignments use positive identity values. The token contains the user ID in `sub`, the email, a unique `jti`, one `role` claim, and one `roleUserId` claim. Existing tokens issued before this change do not contain `roleUserId`; users must log in again before calling `/api/users/me`.
