# Identity API

ASP.NET Core Identity manages password hashes, users, roles and lockout. JWT bearer
authentication protects user endpoints. Registration creates an active `LEARNER`
account; the request cannot assign privileged roles.

## Configuration

Set `ConnectionStrings:DefaultConnection` to an initialized SQL Server database.
For a new, empty database, apply `database/BA-VES-MVP.Initial.sql` first. The API
does not create, migrate, or overwrite a database at startup.

`Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes` (15) and
`Jwt:RefreshTokenDays` (7) are in `appsettings.json`. `Jwt:SigningKey` is deliberately
not committed. Supply a cryptographically random key containing at least 32 UTF-8
bytes through a secret store or the `Jwt__SigningKey` environment variable.

For a local PowerShell session, from the repository root:

```powershell
$env:Jwt__SigningKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
dotnet run --project src/BAVES.Api --no-restore
```

Swagger is at `http://localhost:5291/swagger` with the default launch profile. Use
the Authorize button with the **access token**, not the refresh token. Startup fails
with a configuration error if the key is missing or too short. The generated local
key lasts for that PowerShell session; changing the key invalidates existing access
tokens. Keep a stable, protected key in deployed environments and use HTTPS.

## Endpoints

| Method | Endpoint | Body / header | Success |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | `fullName`, `email`, `password` | 201, token pair and profile |
| POST | `/api/auth/login` | `email`, `password` | 200, token pair and profile |
| POST | `/api/auth/refresh` | `refreshToken` | 200, new token pair and profile |
| POST | `/api/auth/logout` | `refreshToken` | 204, session revoked |
| GET | `/api/users/me` | `Authorization: Bearer <accessToken>` | 200, current profile |

Example registration:

```json
{
  "fullName": "Demo Learner",
  "email": "learner@example.com",
  "password": "StrongPassword1"
}
```

Passwords require at least 8 characters, an uppercase letter, a lowercase letter
and a digit. Email is unique through Identity validation and the existing database
index. Five failed password attempts lock the account for 15 minutes. Auth endpoints
are limited to 60 requests per IP per minute; rejected requests return 429 with
`Retry-After`. Configure trusted proxy handling before using this policy behind a
reverse proxy; never trust forwarded IP headers from arbitrary clients.

Register, login and refresh return:

```json
{
  "accessToken": "<JWT>",
  "accessTokenExpiresAt": "<UTC timestamp>",
  "refreshToken": "<opaque refresh token>",
  "refreshTokenExpiresAt": "<UTC timestamp>",
  "user": {
    "id": 1,
    "email": "learner@example.com",
    "fullName": "Demo Learner",
    "avatarUrl": null,
    "status": "Active",
    "roles": ["LEARNER"],
    "createdAt": "<UTC timestamp>"
  },
  "tokenType": "Bearer"
}
```

`users/me` returns only the `user` object, never password hashes or security stamps.
Validation errors return 400, invalid credentials/tokens return 401, duplicate
registration returns 409. Login failures use the same message for missing accounts
and wrong passwords. Accounts with two-factor authentication enabled are rejected
by this password-only flow until an MFA challenge endpoint is implemented.

## Refresh Token Lifecycle

No new tables or migrations are required. Refresh tokens use the existing
`identity.UserTokens` table with login provider `BAVES.RefreshToken`:

- Each login creates a separate session family for multi-device use.
- A 256-bit random secret is returned to the client. Only the SHA-256 hash of the
  complete token is used as a database key; the raw token is never persisted.
- `Value` stores JSON metadata: security stamp, creation, expiry and revocation.
- An `:active` family marker permits efficient session checks for JWT requests.
- Refresh consumes the old token once using a conditional update and transaction.
  A replacement is returned; the original absolute refresh expiry is preserved.
- Reusing a consumed token revokes that whole family, including its access tokens.
  Logout does the same for the specified family, leaving other devices signed in.
- Each bearer request checks account status, lockout, security stamp, session
  activity and current active roles. Role removal or deactivation rejects stale
  role-bearing JWTs; refresh obtains the current roles.

The desktop client must serialize refresh requests, replace both tokens after a
successful refresh, and store them in protected OS credential storage. Do not
automatically retry the same refresh token after an uncertain network result;
re-authenticate, since the original request may already have consumed it. Expired
token metadata is rejected automatically; a maintenance job can remove expired
families when retention cleanup is introduced.

JWT validation follows [Microsoft's bearer authentication guidance](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-9.0).
Rotation and replay detection follow the principles in [RFC 9700, section 4.14.2](https://www.rfc-editor.org/rfc/rfc9700.html#section-4.14.2).

## Verification

```powershell
dotnet test tests/BAVES.Auth.Tests --no-restore -m:1 -nr:false
dotnet run --project tests/BAVES.ModelValidation --no-restore
```

Auth tests use the real controllers, middleware, Identity services and relational
repository through a local HTTP server and a separate temporary SQLite database
per test. No BA-VES SQL Server data is touched. SQL Server schema mapping is checked
separately by the model validator. Production SQL Server connectivity and provider-
specific concurrency behavior require integration testing against a real server.
