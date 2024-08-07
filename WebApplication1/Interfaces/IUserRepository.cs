using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserModel?> GetUserByIdAsync(int id);

        Task<bool> GetUserByEmailAsync(string email, int? excludeUserId = null);

        Task<UserDto?> CreateUserAsync(CreateUserDto userDto);

        Task<UserDto?> UpdateUserByIdAsync(int id , UpdateUserDto userDto);

        Task<UserModel?> DeleteUserByIdAsync(int id);
    }
}
