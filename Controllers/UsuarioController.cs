using System;
using DailyBrief.AI.Data;
using DailyBrief.AI.DTOs;
using DailyBrief.AI.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DailyBrief.AI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuarioController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult CriarUsuario(CreateUsuarioDto dto)
    {
        if (_context.Usuarios.Any(u => u.Email == dto.Email))
            return BadRequest("E-mail já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha); // Precisa adicionar o pacote

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
}
