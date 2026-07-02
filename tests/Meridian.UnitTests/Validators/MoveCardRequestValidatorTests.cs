using Meridian.Bll.Dtos;
using Meridian.Bll.Validators;

namespace Meridian.UnitTests.Validators;

public class MoveCardRequestValidatorTests
{
    private readonly MoveCardRequestValidator _sut = new();

    [Theory]
    [InlineData("ToDo")]
    [InlineData("InProgress")]
    [InlineData("Done")]
    [InlineData("done")]
    public void ValidStatus_Passes(string status)
    {
        var result = _sut.Validate(new MoveCardRequestDto { Status = status });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Shipped")]
    [InlineData("42")]
    public void InvalidStatus_Fails(string status)
    {
        var result = _sut.Validate(new MoveCardRequestDto { Status = status });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(MoveCardRequestDto.Status));
    }
}
