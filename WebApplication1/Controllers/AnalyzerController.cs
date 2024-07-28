using Microsoft.AspNetCore.Mvc;
using static WebApplication1.Utils.ReverseNumber;
using static WebApplication1.Utils.RemoveDuplicate;
using WebApplication1.TestHelpers;
using Microsoft.AspNetCore.Cors;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigins")]
    [ApiController]
    public class AnalyzerController : ControllerBase
    {
        [HttpPost("reversenum")]
        public IActionResult ReverseNumber([FromBody] int number)
        {
            if (number < 0)
            {
                return BadRequest(new { message = "Number must be non-negative." });
            }

            // Definition in Utils
            uint reversedNumber = ReverseInteger((uint)number);


            var response = new ReverseNumberResponse
            {
                ReversedNumber = reversedNumber
            };
            return Ok(response);
        }

        [HttpPost("removeduplicate")]
        public IActionResult RemoveDuplicate([FromBody] string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest(new { message = "Input cannot be null or empty." });

            }
            // Deifinition in Utils
            string result = RemoveDup(input);
            var response = new RemoveDuplicateResponse
            {
                Message = result
            };
            return Ok(response);

        }
    }
}
   