
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using System.Net;
using WebApplication1.Dto;
using WebApplication1.TestHelpers;
using WebApplication1.Models;
using static WebApplication1.Utils.ApplyUpdate;
using Microsoft.AspNetCore.Cors;
using WebApplication1.ErrorHandling;
namespace WebApplication1.Controllers

{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        // Db Configuration
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        // All Users without UserMessages
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Age = u.Age,
                    Website = u.Website,
                   }).ToListAsync();

                if (users.Count == 0)
                {
                    return NotFound(new NotFoundResponse { Message = "No users found." });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
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
                var user = await _context.Users.Include(u => u.Messages).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new NotFoundResponse { Message = "User Not found" });
                }
                var userMessages = user.Messages.Select(m => new UserMessageDto
                {
                    Content = m.Content

                }).ToList();

                var response = new GetUserByIdResponse
                {
                    Message = "User Found",
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    Age = user.Age,
                    Email = user.Email,
                    Website = user.Website,
                    UserMessages = userMessages
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
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
            if(!ModelState.IsValid)
            {
                return BadRequest(new BadRequest { Message = $"Invalid Data : {ModelState}" });
            }


            var existingUser = await _context.Users
                 .FirstOrDefaultAsync(u => u.Email == createUserDto.Email);

            if (existingUser != null)
            {
                return Conflict(new Conflict { Message = "A user with this email already exists." });
            }
            
            try
            {
                var newUser = new UserModel
                {
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    Age = createUserDto.Age,
                    Website = createUserDto.Website
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var response = new CreateUserResponse
                {
                    Message = "User successfully created.",
                    User = newUser
                };

                return CreatedAtAction(nameof(GetAllUsers), new { id = newUser.Id }, response);

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }

        // Patch request for update users 
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
           if(!ModelState.IsValid)
            {
                return BadRequest(new BadRequest { Message = $"Invalid Data : {ModelState}"});
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new NotFoundResponse { Message = "User Not found." });
                }
                //check email unique 
                var existingUserWithEmail = await _context.Users
          .Where(u => u.Email == updateUserDto.Email && u.Id != id)
          .FirstOrDefaultAsync();

                if (existingUserWithEmail != null)
                {
                    return Conflict(new Conflict { Message = "A user with this email already exists." });
                }
                //definition in utils
                UserUpdateHelper.ApplyUpdates(user, updateUserDto);
                await _context.SaveChangesAsync();
               var response = (new UpdateUserResponse
                {
                    Message = "User successfully updated.",
                    User = user
                });
              return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }

        //DeleteUserById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Messages).FirstOrDefaultAsync( u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new NotFoundResponse { Message = "User Not found." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                var response = new DeleteUserResponse
                {
                    Message = "User and their messages successfully deleted."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
               
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    message = "Internal Server Error",
                    details = ex.Message
                });
            }
        }
    }
}
