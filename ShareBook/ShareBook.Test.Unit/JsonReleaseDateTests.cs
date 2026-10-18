using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.DTOs;
using System;
using Xunit;

namespace ShareBook.Test.Unit;

/// <summary>
/// A API usa .AddNewtonsoftJson(), que é quem faz model binding e resposta no MVC.
/// ReleaseDate é DateOnly e precisa trafegar como "yyyy-MM-dd": o frontend e o script
/// de produção dependem desse formato.
/// </summary>
public class JsonReleaseDateTests
{
    private static readonly JsonSerializerSettings Mvc = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    [Fact]
    public void ReleaseDate_IsSentAsPlainIsoDate()
    {
        var json = JsonConvert.SerializeObject(new HomeShowcaseBookDTO { Title = "x", ReleaseDate = new DateOnly(2026, 11, 15) }, Mvc);

        Assert.Contains("\"releaseDate\":\"2026-11-15\"", json);
    }

    [Fact]
    public void ReleaseDate_IsNullForBooksWithoutSchedule()
    {
        var json = JsonConvert.SerializeObject(new HomeShowcaseBookDTO { Title = "x" }, Mvc);

        Assert.Contains("\"releaseDate\":null", json);
    }

    [Fact]
    public void ApproveBody_AcceptsReleaseDateAsPlainIsoDate()
    {
        var vm = JsonConvert.DeserializeObject<ApproveBookVM>("{\"ChooseDate\":null,\"releaseDate\":\"2026-11-15\"}", Mvc);

        Assert.Equal(new DateOnly(2026, 11, 15), vm!.ReleaseDate);
        Assert.Null(vm.ChooseDate);
    }

    [Fact]
    public void ApproveBody_WithoutReleaseDate_KeepsTheOldBehavior()
    {
        var vm = JsonConvert.DeserializeObject<ApproveBookVM>("{\"ChooseDate\":null}", Mvc);

        Assert.Null(vm!.ReleaseDate);
    }
}
