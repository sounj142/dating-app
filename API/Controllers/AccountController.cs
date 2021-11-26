using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dbContext;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserTokenDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                PasswordHash = ComputePasswordHash(registerDto.Password, hmac),
                PasswordSalt = hmac.Key
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new UserTokenDto 
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }

        private static byte[] ComputePasswordHash(string password, HMACSHA512 hmac)
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDto>> Login(LoginDto loginDto)
        {
            var user = await FindUser(loginDto.UserName);
            if (user == null)
            {
                return Unauthorized("Username or password is incorrect");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var hashData = ComputePasswordHash(loginDto.Password, hmac);

            if (!Enumerable.SequenceEqual(hashData, user.PasswordHash))
            {
                return Unauthorized("Username or password is incorrect");
            }

            return new UserTokenDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return (await FindUser(userName)) != null;
        }

        private async Task<AppUser> FindUser(string userName)
        {
            var userNameLowerCase = userName.ToLower();
            return await _dbContext.Users.FirstOrDefaultAsync(user => user.UserName.ToLower() == userNameLowerCase);
        }
    }
}
