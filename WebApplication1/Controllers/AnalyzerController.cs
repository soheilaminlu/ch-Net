using Microsoft.AspNetCore.Mvc;
using WebApplication1.TestHelpers;
using Microsoft.AspNetCore.Cors;
using WebApplication1.Interfaces;
using WebApplication1.ErrorHandling;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class AnalyzerController : ControllerBase
    {
        private readonly ILogger<AnalyzerController> _logger;
        private readonly IAnalyzerRepository _analyzerRepo;
        public AnalyzerController(ILogger<AnalyzerController> logger, IAnalyzerRepository analyzerRepo)
        {
            _logger = logger;
            _analyzerRepo = analyzerRepo;
        }

        [HttpPost("reversenum")]
        public IActionResult ReverseNumber([FromBody] uint number)
        {
            try
            {
                var reversedNumber = _analyzerRepo.ReverseInteger(number);

                if (reversedNumber == 0)
                {
                    _logger.LogWarning("Zero Is Not Valid Number");
                    return BadRequest(new { message = "Zero is not a valid reversed number" });
                }

                var response = new ReverseNumberResponse
                {
                    ReversedNumber = reversedNumber
                };
                _logger.LogInformation("Number Reversed Successfuly");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing number");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPost("removeduplicate")]
        public IActionResult RemoveDuplicate([FromBody] string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    _logger.LogWarning("Input cannot be Empty or null");
                    return BadRequest(new { message = "Input cannot be null or empty." });
                }

                var result = _analyzerRepo.RemoveDuplicates(input);

                var response = new RemoveDuplicateResponse
                {
                    Message = result
                };
                _logger.LogInformation("Duplicates are Removed Succussfuly From String");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing duplicates");
                return StatusCode(500, new { message = "Internal Server Error" });
            }


        }
    }
}

   