using Microsoft.Extensions.Logging;
using WebApplication1.Controllers;
using WebApplication1.Interfaces;
using WebApplication1.Repository;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.TestHelpers;

public class AnalyzerTests
{
    private readonly IAnalyzerRepository _analyzerRepository;
    private readonly ILogger<AnalyzerController> _logger;
    private readonly AnalyzerController _controller;

    public AnalyzerTests()
    {
        _analyzerRepository = new AnalyzerRepository(); // Initialize repository
        _logger = GetLogger<AnalyzerController>(); // Initialize logger
        _controller = GetController(_analyzerRepository, _logger);
    }

    private AnalyzerController GetController(IAnalyzerRepository analyzerRepository, ILogger<AnalyzerController> logger)
    {
        return new AnalyzerController(logger, analyzerRepository);
    }

    private ILogger<T> GetLogger<T>()
    {
        return new LoggerFactory().CreateLogger<T>();
    }

    // Test Reversing
    [Fact]
    public void ReverseNumber_ShouldReturnOkResult()
    {
        // Arrange
        uint number = 123;

        // Act
        var result = _controller.ReverseNumber(number);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as ReverseNumberResponse;
        Assert.NotNull(response);
        Assert.Equal(321U, response.ReversedNumber);
    }

    // Test Remove Duplicate
    [Fact]
    public void RemoveDuplicate_ShouldReturnOkResult()
    {
        // Arrange
        string input = "aabbcc";

        // Act
        var result = _controller.RemoveDuplicate(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as RemoveDuplicateResponse;
        Assert.NotNull(response);
        Assert.Equal("abc", response.Message);
    }
}