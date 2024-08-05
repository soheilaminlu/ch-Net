using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Interfaces;
using WebApplication1.Mapper;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
                var users = await _context.Users.ToListAsync();
                var userDtos = users.ToUserDtos().ToList();

                return userDtos;      
        }

        public async Task<UserModel?> GetUserByIdAsync(int id)
        {
                var user = await _context.Users
                    .Include(u => u.Messages)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                _logger.LogWarning("User with Id {UserId} not found.", id);
                return null;
                }

                return user;
           
        }
        public async Task<bool> GetUserByEmailAsync(string email)
        {    
                return await _context.Users.AnyAsync(u => u.Email == email);   

        }


        public async Task<UserDto?> CreateUserAsync(CreateUserDto userDto)
        {

            var user = new UserModel
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Age = userDto.Age,
                Website = userDto.Website
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDtoResult = user.ToUserDto();

            return userDtoResult;

        }

        public async Task<UserDto?> UpdateUserByIdAsync(int id, UpdateUserDto userDto)
        {
              var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return null;
                }
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.Age = userDto.Age;
                user.Website = userDto.Website;
            var userDtoResult = user.ToUserDto();
            return userDtoResult;
        }

        public async Task<UserModel?> DeleteUserByIdAsync(int id)
        {
          
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return null;
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return user;
            
        }
    }
}
