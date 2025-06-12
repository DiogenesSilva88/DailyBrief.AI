using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DailyBrief.AI.Configurations;
using DailyBrief.AI.Data;
using DailyBrief.AI.DTOs;
using DailyBrief.AI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DailyBrief.AI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UsuarioController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult CriarUsuario(CreateUsuarioDto dto)
    {
        if (_context.Usuarios.Any(u => u.Email == dto.Email))
            return BadRequest("E-mail já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = senhaHash
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        return CreatedAtAction(null, new { id = usuario.Id }, new { usuario.Id, usuario.Nome, usuario.Email });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginUsuarioDto dto)
    {
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email);
        if (usuario == null) return Unauthorized("Usuário ou senha inválidos");

        var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
        if (!senhaCorreta) return Unauthorized("Usuário ou senha inválidos");

        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email)
            },
            expires: DateTime.UtcNow.AddHours(jwtSettings.ExpiraEmHoras),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}
