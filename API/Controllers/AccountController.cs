using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(IUserRepository userRepository, ITokenService tokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserTokenDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is taken");
            }

            using var hmac = new HMACSHA512();

            var user = _mapper.Map<AppUser>(registerDto);
            user.PasswordHash = hmac.ComputePasswordHash(registerDto.Password);
            user.PasswordSalt = hmac.Key;

            _userRepository.Add(user);
            await _userRepository.SaveAllAsync();

            return new UserTokenDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = null,
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDto>> Login(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUserNameAsync(loginDto.UserName);
            if (user == null)
            {
                return Unauthorized("Username or password is incorrect");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var hashData = hmac.ComputePasswordHash(loginDto.Password);

            if (!Enumerable.SequenceEqual(hashData, user.PasswordHash))
            {
                return Unauthorized("Username or password is incorrect");
            }

            return new UserTokenDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return (await _userRepository.GetUserByUserNameAsync(userName)) != null;
        }
    }
}
