using ShareBook.Service;
using System.Linq;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class BookTagRuleEngineTests
{
    [Theory]
    [InlineData("C++", "cplusplus")]
    [InlineData("C#", "csharp")]
    [InlineData(".NET", "dotnet")]
    [InlineData("Café", "cafe")]
    [InlineData("Ação!", "acao")]
    public void NormalizeText_ShouldExpandTokensAndStripAccents(string input, string expected)
    {
        Assert.Equal(expected, BookTagRuleEngine.NormalizeText(input));
    }

    [Fact]
    public void SuggestTagIds_ShouldReturnPythonForPythonTitle()
    {
        var tags = BookTagRuleEngine.SuggestTagIds("Python Crash Course", "Um guia prático de Python.");

        Assert.Contains("python", tags);
    }

    [Fact]
    public void SuggestTagIds_ShouldMatchCleanCode()
    {
        var tags = BookTagRuleEngine.SuggestTagIds("Clean Code", "Qualidade de código.");

        Assert.Contains("clean-code", tags);
    }

    [Fact]
    public void SuggestTagIds_ShouldMatchKafkaAsEventDriven()
    {
        var tags = BookTagRuleEngine.SuggestTagIds("Kafka, The Definitive Guide", "Event streaming.");

        Assert.Contains("event-driven", tags);
    }

    [Fact]
    public void SuggestTagIds_ShouldNotFireOnSynopsisOnly()
    {
        // A regra exige correspondência no título (sinopse sozinha vale +1, abaixo do piso).
        var tags = BookTagRuleEngine.SuggestTagIds("Um livro qualquer", "fala bastante sobre Python e Docker.");

        Assert.Empty(tags);
    }

    [Fact]
    public void SuggestTagIds_ShouldReturnAtMostThreeTags()
    {
        var tags = BookTagRuleEngine.SuggestTagIds(
            "Python Machine Learning with Docker and Kubernetes",
            "Deep learning, data science, databases, algorithms.");

        Assert.True(tags.Count <= BookTagRuleEngine.MaxTagsPerBook);
    }

    [Fact]
    public void SuggestTagIds_ShouldDeduplicateTags()
    {
        var tags = BookTagRuleEngine.SuggestTagIds("Clean Code", "Clean Code quality.");

        Assert.Equal(1, tags.Count(tag => tag == "clean-code"));
    }

    [Fact]
    public void SuggestTagIds_ShouldNotSuggestGitForSubversion()
    {
        var tags = BookTagRuleEngine.SuggestTagIds("Version Control with Subversion", "svn e versionamento.");

        Assert.DoesNotContain("git", tags);
        Assert.Contains("versionamento-de-codigo", tags);
    }
}
