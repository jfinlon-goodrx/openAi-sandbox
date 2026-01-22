# JWT Authentication Guide

Complete guide to implementing JWT authentication in OpenAI Platform projects.

## Overview

JWT (JSON Web Tokens) provides a stateless authentication mechanism that's ideal for APIs and microservices.

## Setup

### 1. Configuration

Add to `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "OpenAIPlatform",
    "Audience": "OpenAIPlatform",
    "ExpirationMinutes": "60"
  }
}
```

**Important**: In production, use a strong secret key (at least 32 characters) and store it securely (e.g., Azure Key Vault, environment variables).

### 2. Register Services

```csharp
using Shared.Common;

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<JwtTokenService>();
```

### 3. Configure Pipeline

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Usage

### Generate Token (Login)

```csharp
[HttpPost("login")]
[AllowAnonymous]
public IActionResult Login([FromBody] LoginRequest request)
{
    // Validate credentials (check against database)
    if (!ValidateCredentials(request.Email, request.Password))
    {
        return Unauthorized();
    }

    // Generate token
    var token = _tokenService.GenerateToken(
        userId: user.Id,
        email: user.Email,
        roles: user.Roles);

    return Ok(new { token, expiresIn = 3600 });
}
```

### Protect Endpoints

```csharp
[HttpGet("protected")]
[Authorize]
public IActionResult Protected()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    
    return Ok(new { userId, email });
}
```

### Role-Based Authorization

```csharp
[HttpGet("admin-only")]
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    return Ok("Admin access granted");
}
```

### Generate Token with Roles

```csharp
var token = _tokenService.GenerateToken(
    userId: "user-123",
    email: "user@example.com",
    roles: new List<string> { "User", "Admin" });
```

## Client Usage

### Login and Get Token

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### Use Token in Requests

```bash
curl -X GET http://localhost:5001/api/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### JavaScript Example

```javascript
// Login
const loginResponse = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'user@example.com', password: 'password' })
});

const { token } = await loginResponse.json();

// Use token in subsequent requests
const protectedResponse = await fetch('/api/protected', {
    headers: {
        'Authorization': `Bearer ${token}`
    }
});
```

## Token Structure

JWT tokens consist of three parts:
1. **Header**: Algorithm and token type
2. **Payload**: Claims (user ID, email, roles, expiration)
3. **Signature**: Ensures token integrity

Example payload:
```json
{
  "sub": "user-123",
  "email": "user@example.com",
  "role": ["User", "Admin"],
  "exp": 1234567890,
  "iss": "OpenAIPlatform",
  "aud": "OpenAIPlatform"
}
```

## Security Best Practices

1. **Strong Secret Key**: Use at least 32 random characters
2. **HTTPS Only**: Always use HTTPS in production
3. **Short Expiration**: Set reasonable expiration times (1 hour default)
4. **Refresh Tokens**: Implement refresh tokens for long-lived sessions
5. **Token Storage**: Store tokens securely on client (httpOnly cookies preferred)
6. **Validate Tokens**: Always validate tokens on the server

## Testing

### Generate Test Token

```csharp
var tokenService = new JwtTokenService(configuration);
var token = tokenService.GenerateToken("test-user", "test@example.com", new[] { "User" });
```

### Validate Token

```csharp
var principal = tokenService.ValidateToken(token);
if (principal != null)
{
    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
}
```

## Comparison: API Key vs JWT

| Feature | API Key | JWT |
|---------|---------|-----|
| **Stateless** | Yes | Yes |
| **User Info** | No | Yes (claims) |
| **Expiration** | Manual | Built-in |
| **Roles** | No | Yes |
| **Revocation** | Easy | Requires blacklist |
| **Complexity** | Simple | Moderate |

## Resources

- [JWT.io](https://jwt.io/) - JWT debugger and documentation
- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
