using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;
using WebApplication1.TestHelpers;

namespace TestProject1_challenge.Controllers.UsersTestController
{

    
    public class UsersTests
    {
         // Create Fake Db For Testing
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(databaseName: "TestDatabase")
               .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted(); // Ensure the database is clean before each test
            context.Database.EnsureCreated();

            return context;
        }
        // import and user Controller From Project
        private UsersController GetController(ApplicationDbContext context)
        {
            return new UsersController(context);
        }

        //OK Senario for GetAll Users
        [Fact]
        public async Task GetAllUsers_ReturnsOkResult()
        {
            // Arrange
            var context = GetDbContext();
            context.Users.AddRange(
                new UserModel { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Age = 30, Website = "www.johndoe.com" },
                new UserModel { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", Age = 25, Website = "www.janedoe.com" }
            );
            context.SaveChanges();

            var controller = GetController(context);

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsType<List<UserDto>>(okResult.Value);
            
            Assert.Equal(2, users.Count);

            var firstUser = users.First();
            Assert.Equal(1, firstUser.Id);
            Assert.Equal("John", firstUser.FirstName);
            Assert.Equal("Doe", firstUser.LastName);
            Assert.Equal("john.doe@example.com", firstUser.Email);
            Assert.Equal(30, firstUser.Age);
            Assert.Equal("www.johndoe.com", firstUser.Website);

            var secondUser = users.Last();
            Assert.Equal(2, secondUser.Id);
            Assert.Equal("Jane", secondUser.FirstName);
            Assert.Equal("Doe", secondUser.LastName);
            Assert.Equal("jane.doe@example.com", secondUser.Email);
            Assert.Equal(25, secondUser.Age);
            Assert.Equal("www.janedoe.com", secondUser.Website);
        }
        // OK Senario for CreateUser 
        [Fact]
        public async Task CreateUser_Returns_reatedActionResult()
        {
            var context = GetDbContext();
            var controller = GetController(context);

            var createUserDto = new CreateUserDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };

            var result = await controller.CreateUser(createUserDto);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<CreateUserResponse>(createdAtActionResult.Value);
            var user = response.User;

            Assert.Equal("User successfully created.", response.Message);
            Assert.NotNull(user);
            Assert.Equal("Alice", user.FirstName);
            Assert.Equal("Smith", user.LastName);
            Assert.Equal("alice.smith@example.com", user.Email);
            Assert.Equal(28, user.Age);
            Assert.Equal("www.alicesmith.com", user.Website);
    }
        // Ok Senario For GetUser By Id
        [Fact]
        public async Task GetUserById_ShouldReturnOkResult()
        {
            var context = GetDbContext();  
            var user = new UserModel
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };
            context.Users.Add(user);
            context.Messages.Add(new MessageModel { Content = "Hello, Alice", UserId = 1 });
            context.SaveChanges();
            var controller = GetController(context);
            var result = await controller.GetUserById(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as GetUserByIdResponse;

            Assert.Equal("User Found", response.Message);
            Assert.Equal("Alice", response.Firstname);
            Assert.Equal("Smith", response.Lastname);
            Assert.Equal(28, response.Age);
            Assert.Equal("alice.smith@example.com", response.Email);
            Assert.Equal("www.alicesmith.com", response.Website);

            var messages = response.UserMessages;
            Assert.NotNull(messages);
            Assert.Single(messages);
            Assert.Equal("Hello, Alice", messages[0].Content);

        }

        // OK Senario for UpdateUser By Id 
        [Fact]
        public async Task UpdateUserById_ShouldReturnOkResult()
        {
            var context = GetDbContext();
            var user = new UserModel
            {
                Id = 1 , 
                FirstName = "ali",
                LastName = "alilast",
                Age = 28,
                Email = "ali@ex.com",
                Website = "www.ali.com"
            };
            context.Add(user);
            context.SaveChanges();
            var controller = GetController(context);
            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedAlice",
                LastName = "UpdatedSmith",
                Email = "updated.alice.smith@example.com",
                Age = 30,
                Website = "www.updatedalicesmith.com"
            };
            var result = await controller.UpdateUser(1, updateUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as UpdateUserResponse;

            Assert.NotNull(response);
            Assert.Equal("User successfully updated.", response.Message);
            var updatedUser = response.User;
            Assert.Equal("UpdatedAlice", updatedUser.FirstName);
            Assert.Equal("UpdatedSmith", updatedUser.LastName);
            Assert.Equal("updated.alice.smith@example.com", updatedUser.Email);
            Assert.Equal(30, updatedUser.Age);
            Assert.Equal("www.updatedalicesmith.com", updatedUser.Website);

        }

        // OK Senario For DeleteUser By ID
        [Fact]
        public async Task DeleteUserById_ShouldReturnOk()
        {
            var context = GetDbContext();
            var user = new UserModel
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };
            context.Add(user);
            context.Messages.Add(new MessageModel { Content = "Hello Alice", UserId = 1 });
            context.SaveChanges();
            var controller = GetController(context);     
            var result = await controller.DeleteUser(1);
           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as DeleteUserResponse;

            Assert.NotNull(response);
            Assert.Equal("User and their messages successfully deleted.", response.Message);

            var deletedUser = await context.Users.FindAsync(1);
            Assert.Null(deletedUser);

            var userMessages = await context.Messages.Where(m => m.UserId == 1).ToListAsync();
            Assert.Empty(userMessages);
        }
    }
}
