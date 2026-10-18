using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.DTOs;
using System;
using Xunit;
using Xunit.Abstractions;

namespace ShareBook.Test.Unit;

public class JsonDateOnlyProbeTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerSettings Mvc = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    [Fact]
    public void Probe()
    {
        var home = JsonConvert.SerializeObject(new HomeShowcaseBookDTO { Title = "x", ReleaseDate = new DateOnly(2026, 11, 15) }, Mvc);
        output.WriteLine("SERIALIZA: " + home);
        try
        {
            var vm = JsonConvert.DeserializeObject<ApproveBookVM>("{\"ChooseDate\":null,\"releaseDate\":\"2026-11-15\"}", Mvc);
            output.WriteLine("DESERIALIZA: " + vm!.ReleaseDate);
        }
        catch (Exception e) { output.WriteLine("DESERIALIZA FALHOU: " + e.Message); }
    }
}
