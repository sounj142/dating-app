using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IList<UserDto>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();
            return _mapper.Map<IList<UserDto>>(users);
        }

        [HttpGet("{userName}")]
        public async Task<UserDto> GetById(string userName)
        {
            var user = await _userRepository.GetUserByUserNameAsync(userName);
            return _mapper.Map<UserDto>(user);
        }
    }
}
