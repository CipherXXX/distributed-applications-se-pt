using SkillForge.Application.Common;
using Xunit;

namespace SkillForge.Tests.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var result = new PagedResult<string>
        {
            TotalCount = 25,
            PageSize = 10,
            Page = 1
        };
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_WhenPageGreaterThanOne()
    {
        var result = new PagedResult<string> { Page = 2, TotalCount = 100, PageSize = 10 };
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_WhenPageLessThanTotalPages()
    {
        var result = new PagedResult<string> { Page = 1, TotalCount = 25, PageSize = 10 };
        Assert.True(result.HasNextPage);
    }
}
