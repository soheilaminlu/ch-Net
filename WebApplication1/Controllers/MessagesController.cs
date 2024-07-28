using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;
using System.Net;
using static WebApplication1.Utils.ApplyMessageUpdates;
using WebApplication1.TestHelpers;
using Microsoft.AspNetCore.Cors;

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

        // GetAllMessages 
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            try
            {
                var messages = await _context.Messages
           .Select(m => new MessagesDto
           {
               Id = m.Id,
               Content = m.Content,
               DateCreated = m.DateCreated,
               DateModified = m.DateModified,
               views = m.views,
               published = m.published,
               UserId = m.UserId
           })
            .ToListAsync();
                if (messages == null || messages.Count == 0)
                {
                    return NotFound(new { message = "No messages found" });
                }
                var response = new GetAllMessagesResponse
                {
                    Message = "Messages Retrieved Successfuly",
                    MessagesInfo = messages
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
                    return NotFound(new { message = "Message not found" });
                }
                return Ok(new { Message = message, log = "Message found successfully" });
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
                var message = await _context.Messages.FirstOrDefaultAsync(x => x.Id == id);
                if (message == null)
                {
                    return NotFound(new { message = "Message not found" });
                }
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                var response = new DeleteMessageResponse
                {
                    Message = "Message Deleted Successfully"
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
        //Create Message With UserID In Request body 
        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            if (createMessageDto == null)
            {
                return BadRequest(new { message = "Invalid message data" });
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

                var response = new CreateMessageResponse
                {
                    Message = "Message successfully created.",
                    MessageInfo = message
                    
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
                    return NotFound(new { message = "Message Not Found" });
                }
                //definition in utils
                MessageUpdateHelper.ApplyMessagesUpdate(message, updateMessageDto);
                message.DateModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                var response = new UpdateMessageResponse
                {
                    Message = "Message Updated Successfuly",
                    UpdatedMessage = message
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
                var user = await _context.Users.Include(u => u.Messages).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { message = "User Not Found" });
                }
                var response = new GetUserMessagesResponse
                {
                    UserMessages = user.Messages.Select(m => new MessagesDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        DateCreated = m.DateCreated,
                        DateModified = m.DateModified,
                        views = m.views,
                        published = m.published,
                        UserId = m.UserId
                    }).ToList()
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

