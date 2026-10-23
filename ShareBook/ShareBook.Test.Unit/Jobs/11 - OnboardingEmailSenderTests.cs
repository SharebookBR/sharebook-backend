using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Onboarding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Jobs;

public class OnboardingEmailSenderTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 23, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SendsTheFiveFixedStepsOnlyOnceAndCompletesOnDaySeven()
    {
        await using var context = NewContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "password",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        context.OnboardingJourneys.Add(new OnboardingJourney
        {
            UserId = user.Id,
            NextStep = 0,
            NextEmailAtUtc = Start.UtcDateTime
        });
        await context.SaveChangesAsync();

        foreach (var now in new[]
        {
            Start,
            Start.AddDays(1),
            Start.AddDays(3),
            Start.AddDays(5),
            Start.AddDays(7)
        })
        {
            var result = await CreateJob(context, now).WorkAsync();
            Assert.Contains("Etapas colocadas na fila: 1", result.Details);
        }

        var journey = await context.OnboardingJourneys.SingleAsync();
        Assert.Equal(5, journey.NextStep);
        Assert.Equal(Start.AddDays(7).UtcDateTime, journey.CompletedAtUtc);
        Assert.Equal("sequence_complete", journey.CompletionReason);

        var retry = await CreateJob(context, Start.AddDays(7)).WorkAsync();
        Assert.Contains("Etapas colocadas na fila: 0", retry.Details);
    }

    private static OnboardingEmailSender CreateJob(ApplicationDbContext context, DateTimeOffset now)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AwsSqsSettings:IsActive"] = "true",
                ["ServerSettings:FrontendUrl"] = "https://www.sharebook.com.br",
                ["ServerSettings:BackendUrl"] = "https://api.sharebook.com.br"
            })
            .Build();
        var template = new Mock<IEmailTemplate>();
        template.Setup(item => item.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("<html></html>");
        var email = new Mock<IEmailService>();
        email.Setup(item => item.IsBounceAsync(It.IsAny<string>())).ReturnsAsync(false);
        var users = new Mock<IUserService>();
        users.Setup(item => item.GenerateUnsubscribeToken(It.IsAny<Guid>())).Returns("token");

        // A fila desativada permite testar a transição persistida sem acessar AWS.
        var queue = new MailSenderLowPriorityQueue(Options.Create(new AwsSqsSettings { IsActive = false }));
        return new OnboardingEmailSender(
            new Mock<IJobHistoryRepository>().Object,
            new Mock<ILoggerFactory>().Object,
            new FixedTimeProvider(now),
            context,
            queue,
            template.Object,
            email.Object,
            users.Object,
            configuration,
            Options.Create(new OnboardingSettings { CommunityUrl = "https://chat.whatsapp.com/invite", MaxJourneysPerRun = 20 }));
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
