using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;
using System.Net;
using WebApplication1.TestHelpers;
using Microsoft.AspNetCore.Cors;
using WebApplication1.ErrorHandling;
using WebApplication1.Mapper;
using WebApplication1.Interfaces;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        // DbConfiguration
        private readonly ApplicationDbContext _context;
        private readonly IMessageRepository _messageRepo;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ApplicationDbContext context , ILogger<MessagesController> logger , IMessageRepository messageRepo)
        {
            _context = context;
            _logger = logger;
            _messageRepo = messageRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            try
            {
                // Call the repository method to get all messages
                var messagesDto = await _messageRepo.GetAllMessagesAsync();

                if (messagesDto == null || !messagesDto.Any())
                {
                    _logger.LogInformation("Not Found Any Message");
                    return NotFound(new NotFoundResponse { Message = "No messages found." });
                }

                var response = new GetAllMessagesResponse
                {
                    Message = "Messages Retrieved Successfully",
                    MessagesInfo = messagesDto
                };
                _logger.LogInformation(" Retrived {Count} Message Successfuly", messagesDto.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving messages.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
        //GetMessageById 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageById(int id)
        {
            try
            {
                var messageDto = await _messageRepo.GetMessageByIdAsync(id);

                if (messageDto == null)
                {
                    _logger.LogInformation("Message with Id {MessageId} not found.", id);
                    return NotFound(new NotFoundResponse { Message = $"Message with Id {id} not found." });
                }

                _logger.LogInformation("Retrived Message Successfuly with Id {MessageId}", id);

                return Ok(new
                {
                    Message = "Message retrieved successfully.",
                    MessageInfo = messageDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving message with Id {MessageId}.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
        //DeleteMessageById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var deleteMessage = await _messageRepo.DeleteMessageAsync(id);
                if (deleteMessage == null)
                {
                    _logger.LogWarning("Not Found Reponse For Message with Id {MessageId}" , id);
                    return NotFound(new NotFoundResponse { Message = "Message Not found" });
                }

                var response = new DeleteMessageResponse
                {
                    Message = "Message Deleted Successfully",
                    MessageModel = deleteMessage
                };
                _logger.LogInformation("Message Deleted Successfuly with Id {id}", id);
                return Ok(response);

            } catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occured while Deleting Message");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
          
        }

        //Create Message With UserID In Request body 
        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Data Validation Error");
                return BadRequest(new BadRequestResponse { Message = $"Invalid message data {ModelState}" });
            }

            try
            {
                var messageDto = await _messageRepo.CreateMessageAsync(createMessageDto);

                if (messageDto == null)
                {
                    _logger.LogWarning("Failed to CreateMessage");
                    return BadRequest(new BadRequestResponse { Message = "Failed to Create Message" });
                }

                _logger.LogInformation("Successfuly Created Message with Content : {createmessage}" , messageDto.Content);
                return CreatedAtAction(nameof(GetMessageById), new { id = messageDto.Id }, new CreateMessageResponse
                {
                    Message = "Message successfully created.",
                    MessageInfo = messageDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a message.");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }

        // UpdateMessageById
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageDto updateMessageDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Data Validation Error");
                return BadRequest(new BadRequestResponse { Message = $"Invalid message data {ModelState}" });
            }

            try
            {
                var updatedMessageDto = await _messageRepo.UpdateMessageAsync(id, updateMessageDto);

                if (updatedMessageDto == null)
                {
                    _logger.LogInformation("Message with Id {MessageId} not found for update.", id);
                    return NotFound(new NotFoundResponse { Message = $"Message with Id {id} not found." });
                }
                _logger.LogInformation("Successfuly Updated Message with Content {updatemessage}", updatedMessageDto.Content);

                return Ok(new UpdateMessageResponse
                {
                    Message = "Message updated successfully.",
                    UpdatedMessage = updatedMessageDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the message with Id {MessageId}.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
        // Get UserMessages With UserId

        [HttpGet("usermessages/{userId}")]
        public async Task<IActionResult> GetUserMessagesById(int userId)
        {
            try
            {
                var userMessagesDto = await _messageRepo.GetUserMessagesByIdAsync(userId);

                if (userMessagesDto == null || !userMessagesDto.Any())
                {
                    _logger.LogWarning("Not Found Any Message For This User");
                    return NotFound(new NotFoundResponse { Message = "No messages found for user" });
                }

                var response = new GetUserMessagesResponse
                {
                    Message = $"Messages for user with Id {userId} retrieved successfully.",
                    UserMessages = userMessagesDto
                };
                _logger.LogInformation("Messages Retrived Successfuly for user with Id {userId}", userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving messages for user with Id {UserId}.", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = "Internal Server Error",
                    Details = ex.Message
                });
            }
        }
    }
}

