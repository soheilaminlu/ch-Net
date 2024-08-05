using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.ErrorHandling;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Repository;
using WebApplication1.TestHelpers;

namespace TestProject1_challenge.Controllers.UsersTestController
{

    public class UsersTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UsersController _controller;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersTests()
        {
            _context = GetDbContext();
            _userRepository = GetUserRepository(_context);
            _logger = GetLogger<UsersController>();
            _controller = GetController(_context , _userRepository, _logger);
        }

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        private IUserRepository GetUserRepository(ApplicationDbContext context)
        {
            var logger = GetLogger<UserRepository>();
            return new UserRepository(context, logger);
        }

        private UsersController GetController(ApplicationDbContext context, IUserRepository userRepo, ILogger<UsersController> logger)
        {
            return new UsersController(context, logger, userRepo);
        }

        private ILogger<T> GetLogger<T>()
        {
            return new LoggerFactory().CreateLogger<T>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        [Fact]
        public async Task GetAllUsers_ReturnsOkResult()
        {
            // Arrange
            _context.Users.AddRange(
                new UserModel { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Age = 30, Website = "www.johndoe.com" },
                new UserModel { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", Age = 25, Website = "www.janedoe.com" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllUsersResponse>(okResult.Value);

            // Assert message
            Assert.Equal("Users Retrieved Successfully", response.Message);

            // Assert user list
            var users = response.Users;

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
        [Fact]
        public async Task GetAllUsers_ReturnsNotFound_WhenNoUsersExist()
        {
            // Arrange
            var context = GetDbContext();
            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);

            // Ensure the database is empty
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetAllUsers();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("No users found.", response.Message);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedActionResult()
        {
            var createUserDto = new CreateUserDto
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };

            var result = await _controller.CreateUser(createUserDto);
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
        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var context = GetDbContext();
            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);

            // Create an invalid DTO (e.g., missing required fields)
            var invalidDto = new CreateUserDto
            {
                // Assuming FirstName is required but not provided
                LastName = "Doe",
                Email = "john.doe@example.com",
                Age = 30,
                Website = "www.johndoe.com"
            };

            controller.ModelState.AddModelError("FirstName", "The FirstName field is required.");

            // Act
            var result = await controller.CreateUser(invalidDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<BadRequestResponse>(badRequestResult.Value);

            // The message should contain the ModelState errors
            Assert.Contains("Invalid Data", response.Message);
        }
        [Fact]
        public async Task CreateUser_ReturnsConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var context = GetDbContext();
            var existingUser = new UserModel
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Age = 30,
                Website = "www.johndoe.com"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);
            var newUserDto = new CreateUserDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "john.doe@example.com", // Email already exists
                Age = 25,
                Website = "www.janedoe.com"
            };

            // Act
            var result = await controller.CreateUser(newUserDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<ConflictResponse>(conflictResult.Value); // Conflict response type is dynamic

            Assert.Equal("A user with this email already exists.", response.Message);
        }
        [Fact]
        public async Task GetUserById_ShouldReturnOkResult()
        {
            var user = new UserModel
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };
            _context.Users.Add(user);
            _context.Messages.Add(new MessageModel { Content = "Hello, Alice", UserId = 1 });
            await _context.SaveChangesAsync();

            var result = await _controller.GetUserById(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetUserByIdResponse>(okResult.Value);

            Assert.Equal($"Successfully retrieved user with Id {user.Id}.", response.Message);
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

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var context = GetDbContext();
            var userRepository = GetUserRepository(context);
            var id = 999;
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);

            // Act
            var result = await controller.GetUserById(id); // Assuming ID 999 does not exist

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("User Not found", response.Message);
        }



        [Fact]
        public async Task UpdateUserById_ShouldReturnOkResult()
        {
            var user = new UserModel
            {
                Id = 1,
                FirstName = "ali",
                LastName = "alilast",
                Age = 28,
                Email = "ali@ex.com",
                Website = "www.ali.com"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateUserDto = new UpdateUserDto
            {
                FirstName = "UpdatedAlice",
                LastName = "UpdatedSmith",
                Email = "updated.alice.smith@example.com",
                Age = 30,
                Website = "www.updatedalicesmith.com"
            };
            var result = await _controller.UpdateUserById(1, updateUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UpdateUserResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal("User updated successfully.", response.Message);
            var updatedUser = response.User;
            Assert.Equal("UpdatedAlice", updatedUser.FirstName);
            Assert.Equal("UpdatedSmith", updatedUser.LastName);
            Assert.Equal("updated.alice.smith@example.com", updatedUser.Email);
            Assert.Equal(30, updatedUser.Age);
            Assert.Equal("www.updatedalicesmith.com", updatedUser.Website);
        }
        [Fact]
        public async Task UpdateUser_ReturnsConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var context = GetDbContext();
            // Add a user with a specific email
            context.Users.Add(new UserModel
            {
                Id = 1,
                Email = "existing@example.com"
            });
            await context.SaveChangesAsync();

            // Create another user with a different ID
            var userToUpdate = new UserModel
            {
                Id = 2,
                Email = "new@example.com" // Email to be updated
            };
            context.Users.Add(userToUpdate);
            await context.SaveChangesAsync();

            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);
            var updateUserDto = new UpdateUserDto
            {
                Email = "existing@example.com" // Attempt to update with an email that already exists
            };

            // Act
            var result = await controller.UpdateUserById(2, updateUserDto); // Update user with ID 2

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<ConflictResponse>(conflictResult.Value);

            Assert.Equal("A user with this email already exists.", response.Message);
        }
        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var context = GetDbContext();
            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);
            var updateUserDto = new UpdateUserDto
            {
                Email = "new@example.com"
            };

            // Act
            var result = await controller.UpdateUserById(999, updateUserDto); // Assuming user id 1 does not exist

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("User not found.", response.Message);
        }

        [Fact]
        public async Task DeleteUserById_ShouldReturnOk()
        {
            var user = new UserModel
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                Age = 28,
                Website = "www.alicesmith.com"
            };
            _context.Users.Add(user);
            _context.Messages.Add(new MessageModel { Content = "Hello Alice", UserId = 1 });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteUserById(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DeleteUserResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal("User and their messages successfully deleted.", response.Message);

            var deletedUser = await _context.Users.FindAsync(1);
            Assert.Null(deletedUser);

            var userMessages = await _context.Messages.Where(m => m.UserId == 1).ToListAsync();
            Assert.Empty(userMessages);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            var context = GetDbContext();
            var id = 999;
            var userRepository = GetUserRepository(context);
            var logger = GetLogger<UsersController>();
            var controller = GetController(context, userRepository, logger);

            // Act
           
            var result = await controller.DeleteUserById(id); // Id that does not exist

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal($"User with Id {id} not found.", response.Message);
        }
    }
}