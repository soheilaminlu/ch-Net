using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;
using WebApplication1.TestHelpers;
using WebApplication1.ErrorHandling;
using Microsoft.Extensions.Logging;
using WebApplication1.Interfaces;
using WebApplication1.Repository;

namespace TestProject1_challenge.Controllers.MessagesTestController
{
    public class MessagesTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly MessagesController _controller;
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<MessagesController> _logger;

        public MessagesTests()
        {
            _context = GetDbContext();
            _messageRepository = GetMessageRepository(_context);
            _logger = GetLogger<MessagesController>();
            _controller = GetController(_context, _messageRepository, _logger);
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

        private IMessageRepository GetMessageRepository(ApplicationDbContext context)
        {
            var logger = GetLogger<MessageRepository>();
            return new MessageRepository(context, logger);
        }

        private MessagesController GetController(ApplicationDbContext context, IMessageRepository messageRepo, ILogger<MessagesController> logger)
        {
            return new MessagesController(context, logger, messageRepo);
        }

        private ILogger<T> GetLogger<T>()
        {
            return new LoggerFactory().CreateLogger<T>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        // Ok Senario for GetAll Messages
        [Fact]
        public async Task GetAllMessages_ReturnsOkResult()
        {
            var context = GetDbContext();
            context.Messages.AddRange(
     new MessageModel
     {
         Id = 1,
         Content = "First message",
         DateCreated = DateTime.UtcNow,
         DateModified = DateTime.UtcNow,
         views = 5,
         published = true,
         UserId = 1
     },
     new MessageModel
     {
         Id = 2,
         Content = "Second message",
         DateCreated = DateTime.UtcNow,
         DateModified = DateTime.UtcNow,
         views = 3,
         published = false,
         UserId = 2
     }
 );
            context.SaveChanges();
            var messageRepository = GetMessageRepository(context);
            var logger = GetLogger<MessagesController>();

            var controller = GetController(context, messageRepository, logger);

            // Act
            var result = await controller.GetAllMessages();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllMessagesResponse>(okResult.Value);

            Assert.Equal("Messages Retrieved Successfully", response.Message);
            Assert.NotNull(response.MessagesInfo);
            Assert.Equal(2, response.MessagesInfo.Count);

            var firstMessage = response.MessagesInfo.First();
            Assert.Equal("First message", firstMessage.Content);
            Assert.Equal(5, firstMessage.views);
            Assert.True(firstMessage.published);

            var secondMessage = response.MessagesInfo.Last();
            Assert.Equal("Second message", secondMessage.Content);
            Assert.Equal(3, secondMessage.views);
            Assert.False(secondMessage.published);
        }
        // Not Found Senario for GetAllMessages
        [Fact]
        public async Task GetAllMessages_ReturnsNotFound_WhenNoMessagesExist()
        {
            // Act
            var result = await _controller.GetAllMessages();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("No messages found.", response.Message);
        }

        // OK Senario for Create Message
        [Fact]
        public async Task CreateMessage_ShouldReturnCreatedAtAction()
        {
            var context = GetDbContext();
            var messageRepository = GetMessageRepository(context);
            var logger = GetLogger<MessagesController>();

            var controller = GetController(context, messageRepository, logger);
            var createMessageDto = new CreateMessageDto
            {
               Content = "Hello From Alice",
                UserId = 1
            };
            var result = await controller.CreateMessage(createMessageDto);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<CreateMessageResponse>(createdAtActionResult.Value);
            Assert.Equal("Message successfully created.", response.Message);
            Assert.NotNull(response.MessageInfo);
            Assert.Equal(createMessageDto.Content, response.MessageInfo.Content);
            Assert.Equal(createMessageDto.UserId, response.MessageInfo.UserId);
            Assert.NotNull(response.MessageInfo.DateCreated); 
            Assert.NotNull(response.MessageInfo.DateModified); 

        }
        // BadRequest Senario For Create Message

        [Fact]
        public async Task CreateMessage_ReturnsBadRequest_WhenMessageDataIsInvalid()
        {
            // Arrange
            var invalidMessageDto = (CreateMessageDto)null; // Test invalid message data

            // Act
            var result = await _controller.CreateMessage(invalidMessageDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<BadRequestResponse>(badRequestResult.Value);

            Assert.Equal("Failed to Create Message", response.Message);
        }

        //OK Senario for Update Message
        [Fact]
        public async Task UpdateMessage_ShouldReturnOkResponse()
        {
            var context = GetDbContext();
            var messageRepository = GetMessageRepository(context);
            var logger = GetLogger<MessagesController>();

            var controller = GetController(context, messageRepository, logger);
            var message = new MessageModel
            {
                Id = 1,
                Content = "Initial Content",
                UserId = 1,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            context.Add(message);
            context.SaveChanges();
            var updateMessageDto = new UpdateMessageDto
            {
                Content = "update Content"
            };
            var result = await controller.UpdateMessage(1, updateMessageDto);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UpdateMessageResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal("Message updated successfully.", response.Message);
            var updatedMessage = response.UpdatedMessage;
            Assert.Equal("update Content", updatedMessage.Content);

            Assert.NotNull(updatedMessage.DateModified);
            var timeDifference = DateTime.UtcNow - updatedMessage.DateModified.Value;
            Assert.True(timeDifference.TotalSeconds < 5, "DateModified should be within 5 seconds of the current time.");
        }
        // Not Found Senario For Update Message
        public async Task UpdateMessage_ReturnsNotFound_WhenMessageDoesNotExist()
        {
            // Arrange
            var nonExistentMessageId = 999; // A message ID that does not exist
            var updateMessageDto = new UpdateMessageDto
            {
                Content = "Updated content"
            };

            // Act
            var result = await _controller.UpdateMessage(nonExistentMessageId, updateMessageDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal(" Message Not found", response.Message);
        }


        // OK Senario For GetUserMessages
        [Fact]
        public async Task GetUserMessage_ShouldReturnOkResult()
        {
            // Arrange
            using (var context = GetDbContext())
            {
                var user = new UserModel
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Smith",
                    Age = 30,
                    Email = "alice.smith@example.com",
                    Website = "www.alicesmith.com"
                };

                var message1 = new MessageModel
                {
                    Id = 1,
                    Content = "Message 1",
                    UserId = user.Id,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                var message2 = new MessageModel
                {
                    Id = 2,
                    Content = "Message 2",
                    UserId = user.Id,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow
                };

                context.Users.Add(user);
                context.Messages.AddRange(message1, message2);
                await context.SaveChangesAsync();

                var messageRepository = GetMessageRepository(context);
                var logger = GetLogger<MessagesController>();

                var controller = GetController(context, messageRepository, logger);

                // Act
                var result = await controller.GetUserMessagesById(1);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var response = Assert.IsType<GetUserMessagesResponse>(okResult.Value);

                Assert.NotNull(response);
                Assert.Contains(response.UserMessages, m => m.Content == "Message 1");
                Assert.Contains(response.UserMessages, m => m.Content == "Message 2");
            }
        }

        // Not Found Senario for GetUserMessages
        [Fact]
        public async Task GetUserMessages_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = 999; 

            // Act
            var result = await _controller.GetUserMessagesById(nonExistentUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("No messages found for user", response.Message);
        }

        // Ok Senario for Delete Message
        [Fact]
        public async Task DeleteMessageById_ShouldReturnOkResult()
        {
            var context = GetDbContext();
            var message = new MessageModel
            {
                Id = 1,
                Content = "Test For Delete",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
                views = 0,
                published = false,
                UserId = 1
            };
            context.Messages.Add(message);
            context.SaveChanges();
            var messageRepository = GetMessageRepository(context);
            var logger = GetLogger<MessagesController>();

            var controller = GetController(context, messageRepository, logger);
            var result = await controller.DeleteMessage(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DeleteMessageResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal("Message Deleted Successfully", response.Message);

            var deletedMessage = await context.Messages.FindAsync(1);
            Assert.Null(deletedMessage);
        }

        [Fact]
        public async Task DeleteMessageById_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            var context = GetDbContext();
            var messageRepository = GetMessageRepository(context);
            var logger = GetLogger<MessagesController>();

            var controller = GetController(context, messageRepository, logger);

            // Act
            var result = await controller.DeleteMessage(999); // Id that does not exist

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<NotFoundResponse>(notFoundResult.Value);

            Assert.Equal("Message Not found", response.Message);
        }
    }
    }
