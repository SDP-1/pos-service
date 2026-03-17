
<!-- 
# pos-service
dotnet ef migrations add message
dotnet ef database update
-->

### Technologies

| Aspect | Technology/Pattern Used |
|--------|------------------------|
| Framework | ASP.NET Core Web API |
| Database Access | Entity Framework Core (EF Core) |
| Mapping | AutoMapper (for Entity ↔ DTO projection) |
| Dependency Management | Inversion of Control (IoC) via built-in Dependency Injection |
| Authentication | JWT Bearer Tokens (Stateless, Token-Based Security) |
| Security | PBKDF2 Hashing via PasswordHasher for password storage |
