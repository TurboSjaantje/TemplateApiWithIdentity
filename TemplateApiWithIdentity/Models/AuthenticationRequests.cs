namespace TemplateApiWithIdentity.Models;

public record RegisterRequest(string Email, string Password, string Username);

public record LoginRequest(string Email, string Password);