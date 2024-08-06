
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using System.Net;
using WebApplication1.Dto;
using WebApplication1.TestHelpers;
using WebApplication1.Models;
using static WebApplication1.Utils.ApplyUpdate;
using Microsoft.AspNetCore.Cors;
using WebApplication1.Mapper;
using WebApplication1.ErrorHandling;
using WebApplication1.Interfaces;
using Microsoft.Extensions.Logging;
using WebApplication1.Repository;
namespace WebApplication1.Controllers


{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        //Repository Configuration For Dependency Injection
        IUserRepository _userRepo;

        //Ilogger Configuration
        private readonly ILogger<UsersController> _logger;
        // Db Configuration
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger , IUserRepository userRepo)
        {
            _context = context;
            _logger = logger;
            _userRepo = userRepo;        
        }
        // All Users without UserMessages
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepo.GetAllUsersAsync();

                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning("No users found.");
                    return NotFound(new NotFoundResponse { Message = "No users found." });
                }
                _logger.LogInformation("Successfully retrieved {Count} users", users.Count);
                return Ok(new GetAllUsersResponse
                {
                    Message = "Users Retrieved Successfully",
                    Users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all users.");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        Message = "Internal Server Error.",
                        Details = ex.Message
                    });
            }
        }

        // GetUserById with UserMessages
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userRepo.GetUserByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with Id {UserId} not found.", id);
                    return NotFound(new NotFoundResponse { Message = "User Not found" });
                }

               
                var userMessages = user.Messages.Select(m => new UserMessageDto
                {
                    Content = m.Content
                }).ToList();

                var response = new GetUserByIdResponse
                {
                    Message = $"Successfully retrieved user with Id {id}.",
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    Age = user.Age,
                    Email = user.Email,
                    Website = user.Website,
                    UserMessages = userMessages
                };
                _logger.LogInformation("Successfully retrieved user with Id {UserId}.", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user with Id {UserId}.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }

        // Create User without jwt authentication and bcrypt passwordHasher 
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Data Validation Error for Creating User");
                return BadRequest(new BadRequestResponse { Message = $"Invalid Data : {ModelState}" });
            }

            try
            {
                // Check if user already exists
                if (await _userRepo.GetUserByEmailAsync(createUserDto.Email))
                {
                    _logger.LogWarning("Attempt to create a user with an existing email: {UserEmail}.", createUserDto.Email);
                    return Conflict(new ConflictResponse { Message = "A user with this email already exists." });
                }

                // Create a new user
                var newUserDto = await _userRepo.CreateUserAsync(createUserDto);

                _logger.LogInformation("User with email {UserEmail} successfully created.", createUserDto.Email);
                return CreatedAtAction(nameof(GetAllUsers), new CreateUserResponse
                {
                    Message = "User successfully created.",
                    User = newUserDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user with email {UserEmail}.", createUserDto.Email);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }


        // Patch request for update users 
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserById(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {

                _logger.LogWarning("Data Validation Error for Creating User");
                return BadRequest(new BadRequestResponse { Message = $"Invalid Data: {ModelState}" });
            }

            try
            {

                var updatedUser = await _userRepo.UpdateUserByIdAsync(id, updateUserDto);

                if (updatedUser == null)
                {
                    _logger.LogWarning("Not Found User With this Id {UserId}", id);
                    return NotFound(new NotFoundResponse { Message = "User not found." });
                }


                if (await _userRepo.GetUserByEmailAsync(updatedUser.Email))
                {
                    _logger.LogWarning("Attempt to Update a user with an existing email: {UserEmail}.", updatedUser.Email);
                    return Conflict(new ConflictResponse { Message = "A user with this email already exists." });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfuly Updated User With Id {UserId}", id);
                return Ok(new UpdateUserResponse
                {
                    Message = "User updated successfully.",
                    User = updatedUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user with Id {UserId}.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }

        //DeleteUserById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            try
            {
                var deletedUser = await _userRepo.DeleteUserByIdAsync(id);

                if (deletedUser == null)
                {
                    _logger.LogWarning("Not Found User With this Id {UserId}", id);
                    return NotFound(new NotFoundResponse { Message = $"User with Id {id} not found." });
                }
                _logger.LogInformation("Successfuly Deleted User With Id {UserId}", id);
                return Ok(new DeleteUserResponse
                {
                    Message = "User and their messages successfully deleted.",
                    User = deletedUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with Id {UserId}.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
    }
}
