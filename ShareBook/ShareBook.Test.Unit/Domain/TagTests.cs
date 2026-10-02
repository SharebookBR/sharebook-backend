using ShareBook.Domain;
using ShareBook.Domain.Enums;
using Xunit;

namespace ShareBook.Test.Unit.Domain;

public class TagTests
{
    [Fact]
    public void TagShouldBeActiveAndPublicByDefault()
    {
        var tag = new Tag();

        Assert.Equal(TagStatus.Active, tag.Status);
        Assert.True(tag.IsPublic);
    }

    [Theory]
    [InlineData("kubernetes")]
    [InlineData("KUBERNETES")]
    [InlineData("k8s")]
    [InlineData("K8S")]
    public void MatchesIdOrAliasShouldAcceptCanonicalIdAndAliases(string value)
    {
        var tag = new Tag
        {
            Id = "kubernetes",
            Aliases = ["k8s"]
        };

        Assert.True(tag.MatchesIdOrAlias(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("docker")]
    public void MatchesIdOrAliasShouldRejectEmptyOrUnrelatedValues(string value)
    {
        var tag = new Tag
        {
            Id = "kubernetes",
            Aliases = ["k8s"]
        };

        Assert.False(tag.MatchesIdOrAlias(value));
    }
}
