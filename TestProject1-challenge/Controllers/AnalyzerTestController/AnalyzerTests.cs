using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebApplication1.TestHelpers;
using System.Threading.Tasks;
using WebApplication1.Controllers;

namespace TestProject1_challenge.Controllers.AnalyzerTestController
{
    // Import and Use Controller for Test Analyzer
    public class AnalyzerTests
    {
        private AnalyzerController GetController()
        {
            return new AnalyzerController();
        }
        // Test Reversing
        [Fact]
        public void ReverseNumber_ShouldReturnOkResult()
        {
            // Arrange
            var controller = GetController();
            int number = 123;

            // Act
            var result = controller.ReverseNumber(number);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ReverseNumberResponse;
            Assert.NotNull(response);
            Assert.Equal(321U,response.ReversedNumber);
        }
        // Test Remove Duplicate
        [Fact]
        public void RemoveDuplicate_ShouldReturnOkResult()
        {
            // Arrange
            var controller = GetController();
            string input = "aabbcc";

            // Act
            var result = controller.RemoveDuplicate(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as RemoveDuplicateResponse;
            Assert.NotNull(response);
            Assert.Equal("abc", response.Message);
        }
    }
}
