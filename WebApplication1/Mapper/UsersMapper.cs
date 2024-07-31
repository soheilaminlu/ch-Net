using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Mapper
{
    public static class UsersMapper
    {
        public static UserDto ToUserDto(this UserModel user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Age = user.Age,
                Website = user.Website
            };
        }
        public static IEnumerable<UserDto> ToUserDtos(this IEnumerable<UserModel> users)
        {
            return users.Select(u => u.ToUserDto());
        }
    }
}
