﻿using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    [HttpPost("register")] // POST: api/accounts/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (string.IsNullOrEmpty(registerDto.Username))
        {
            return BadRequest("Username cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(registerDto.Password))
        {
            return BadRequest("Password cannot be null or empty.");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser 
        
        
        {
            Gender = "female",
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await _context.Users
        .Include(u => u.Photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

        if (user == null) return Unauthorized("Invalid Username");

        if (user.PasswordSalt == null)
        {
            return Unauthorized("Invalid user data. Password salt is missing.");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        if (string.IsNullOrEmpty(loginDto.Password))
        {
            return Unauthorized("Password cannot be null or empty.");
        }

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (user.PasswordHash == null)
        {
            return Unauthorized("Invalid user data. Password hash is missing.");
        }

        for(int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }
        
        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
    }
}
