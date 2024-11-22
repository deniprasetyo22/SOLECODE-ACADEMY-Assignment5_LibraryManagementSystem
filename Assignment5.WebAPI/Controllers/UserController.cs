using Asp.Versioning;
using Assignment5.Application.DTOs;
using Assignment5.Application.Interfaces.IService;
using Assignment5.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Assignment5.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<User>> AddUser([FromBody] User user)
        {
            try
            {
                var addedUser = await _userService.AddUser(user);
                return Ok("User Added Successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers([FromQuery] QueryObjectMember query)
        {
            try
            {
                var users = await _userService.GetAllUsers(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("noPages")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAllUsersNoPages()
        {
            try
            {
                var users = await _userService.GetAllUsersNoPages();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserById(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{userId}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] User user)
        {
            try
            {
                await _userService.UpdateUser(userId, user);
                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{userId}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var success = await _userService.DeleteUser(userId);
                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
