using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;
using System.Net;
using static WebApplication1.Utils.ApplyMessageUpdates;
using WebApplication1.TestHelpers;
using Microsoft.AspNetCore.Cors;
using WebApplication1.ErrorHandling;
using WebApplication1.Mapper;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        // DbConfiguration
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            try
            {
                var messages = await _context.Messages.ToListAsync();

                if (messages == null || !messages.Any())
                {
                    return NotFound(new NotFoundResponse { Message = "No messages found" });
                }

                var messagesDto = messages.ToMessagesDtos(); 

                var response = new GetAllMessagesResponse
                {
                    Message = "Messages Retrieved Successfuly",
                    MessagesInfo = messagesDto.ToList()
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
        //GetMessageById 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageById(int id)
        {
            try
            {
                var message = await _context.Messages.FirstOrDefaultAsync(x => x.Id == id);

                if (message == null)
                {
                    return NotFound(new NotFoundResponse { Message = "Message Not found" });
                }

                
                var messageDto = message.ToMessagesDto();

                return Ok(new
                {
                    message = "Message Retrived successfully",
                    MessageData = messageDto,
                   
                });
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
        //DeleteMessageById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                {
                    return NotFound(new NotFoundResponse { Message = "Message Not found" });
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync(); // Ensure async save

                var response = new DeleteMessageResponse
                {
                    Message = "Message Deleted Successfully"
                };
                return Ok(response);

            } catch (Exception ex)
            {
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
            if (createMessageDto == null)
            {
                return BadRequest(new BadRequestResponse { Message = "Invalid message data" });
            }

            var message = new MessageModel
            {
                Content = createMessageDto.Content,
                UserId = createMessageDto.UserId,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            };

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                var messageDto = message.ToMessagesDto();

                var response = new CreateMessageResponse
                {
                    Message = "Message successfully created.",
                    MessageInfo = messageDto
                    
                };

                return CreatedAtAction(nameof(GetAllMessages), new { id = message.Id }, response);
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

        // UpdateMessageById
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageDto updateMessageDto)
        {
            try
            {
                var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
                if (message == null)
                {
                    return NotFound(new NotFoundResponse { Message = " Message Not found" });
                }
                //definition in utils
                MessageUpdateHelper.ApplyMessagesUpdate(message, updateMessageDto);
                message.DateModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
               var messageDto =  message.ToMessagesDto();

                var response = new UpdateMessageResponse
                {
                    Message = "Message Updated Successfuly",
                    UpdatedMessage = messageDto
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
        // Get UserMessages With UserId
       
        [HttpGet("usermessages/{id}")]
        public async Task<IActionResult> GetUserMessages(int id)
        {
            try
            {
                // شامل کردن پیام‌های کاربر در بازیابی داده
                var user = await _context.Users.Include(u => u.Messages).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new NotFoundResponse { Message = "User Not found" });
                }

                // استفاده از MessageMapper برای تبدیل لیست پیام‌ها به DTO
                var userMessagesDto = user.Messages.ToMessagesDtos().ToList();

                if (userMessagesDto.Count == 0)
                {
                    return NotFound(new NotFoundResponse { Message = "Not Found Any Messages For this User" });
                }

                var response = new GetUserMessagesResponse
                {
                    Message = $"Retrived Messages from User {id}",
                    UserMessages = userMessagesDto
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
    }
}

