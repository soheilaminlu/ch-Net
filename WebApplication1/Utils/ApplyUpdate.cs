using WebApplication1.Dto;
using WebApplication1.Models;



// Check Inputs for Update Users
namespace WebApplication1.Utils
{
    public class ApplyUpdate
    {
        public static class UserUpdateHelper
        {
            public static void ApplyUpdates(UserModel user, UpdateUserDto updateUserDto)
            {
                if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                {
                    user.FirstName = updateUserDto.FirstName;
                }

                if (!string.IsNullOrEmpty(updateUserDto.LastName))
                {
                    user.LastName = updateUserDto.LastName;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    user.Email = updateUserDto.Email;
                }

                if (updateUserDto.Age.HasValue)
                {
                    user.Age = updateUserDto.Age;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Website))
                {
                    user.Website = updateUserDto.Website;
                }
            }
        }
    }
}
