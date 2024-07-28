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

namespace TestProject1_challenge.Controllers.MessagesTestController
{
    public class MessagesTests
    {
        //Create Fake Db For Testing
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
        //Import MessageController from project
        private MessagesController GetController(ApplicationDbContext context)
        {
            return new MessagesController(context);
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
            var controller = GetController(context);

            // Act
            var result = await controller.GetAllMessages();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllMessagesResponse>(okResult.Value);

            Assert.Equal("Messages Retrieved Successfuly", response.Message);
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

        // OK Senario for Create Message
        [Fact]
        public async Task CreateMessage_ShouldReturnCreatedAtAction()
        {
            var context = GetDbContext();
            var controller = GetController(context);
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
            Assert.NotNull(response.MessageInfo.DateCreated); // Check DateCreated is not null
            Assert.NotNull(response.MessageInfo.DateModified); // Check DateModified is not null

        }
         
        //OK Senario for Update Message
        [Fact]
        public async Task UpdateMessage_ShouldReturnOkResponse()
        {
            var context = GetDbContext();
            var controller = GetController(context);
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
            Assert.Equal("Message Updated Successfuly", response.Message);
            var updatedMessage = response.UpdatedMessage;
            Assert.Equal("update Content", updatedMessage.Content);

            Assert.NotNull(updatedMessage.DateModified);
            var timeDifference = DateTime.UtcNow - updatedMessage.DateModified.Value;
            Assert.True(timeDifference.TotalSeconds < 5, "DateModified should be within 5 seconds of the current time.");
        }

        // Ok Senario for Delete Message
        [Fact]
        public async Task DeleteMessageById_ShouldReturnOkResult()
        {
            var context = GetDbContext();
            var message = new MessageModel
            {
                Id = 1,
                Content = "Test For Delete"
               
            };
            
            context.Messages.Add(message);
            context.SaveChanges();
            var controller = GetController(context);
            var result = await controller.DeleteMessage(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as DeleteUserResponse;

            Assert.NotNull(response);
            Assert.Equal("Message Deleted Successfully", response.Message);

            var deletedMessage = await context.Users.FindAsync(1);
            Assert.Null(deletedMessage);
        }
        // OK Senario For GetUserMessages
        [Fact]
        public async Task GetUserMessage_ShouldReturnOkResult()
        {
            var context = GetDbContext();
          

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
            context.SaveChanges();

            var controller = GetController(context);
            var result = await controller.GetUserMessages(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetUserMessagesResponse>(okResult.Value);

            Assert.NotNull(response);
            Assert.Equal(2, response.UserMessages.Count);
            Assert.Contains(response.UserMessages, m => m.Content == "Message 1");
            Assert.Contains(response.UserMessages, m => m.Content == "Message 2");

        }
    }
    }
