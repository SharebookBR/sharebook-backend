using System;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using Xunit;

namespace ShareBook.Test.Unit;

public class BookReleasePolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 1, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UpcomingPhysicalBookCannotBeRequested()
    {
        Assert.False(BookReleasePolicy.CanRequest(BookStatus.Available, BookType.Printed, Now.AddSeconds(1), Now));
        Assert.True(BookReleasePolicy.IsUpcoming(BookStatus.Available, BookType.Printed, Now.AddSeconds(1), Now));
    }

    [Fact]
    public void PhysicalBookOpensAtExactReleaseInstant()
    {
        Assert.True(BookReleasePolicy.CanRequest(BookStatus.Available, BookType.Printed, Now, Now));
        Assert.False(BookReleasePolicy.IsUpcoming(BookStatus.Available, BookType.Printed, Now, Now));
    }

    [Fact]
    public void UnscheduledPhysicalBookPreservesExistingBehavior()
    {
        Assert.True(BookReleasePolicy.CanRequest(BookStatus.Available, BookType.Printed, null, Now));
    }

    [Fact]
    public void UnavailableBookCannotBeRequestedEvenAfterRelease()
    {
        Assert.False(BookReleasePolicy.CanRequest(BookStatus.WaitingApproval, BookType.Printed, Now.AddDays(-1), Now));
    }

    [Fact]
    public void ElectronicBookIsUnaffectedByPhysicalReleaseDate()
    {
        Assert.True(BookReleasePolicy.CanRequest(BookStatus.Available, BookType.Eletronic, Now.AddDays(1), Now));
    }

    [Fact]
    public void RejectsAmbiguousLocalTimes()
    {
        Assert.Throws<ArgumentException>(() =>
            BookReleasePolicy.CanRequest(BookStatus.Available, BookType.Printed, DateTime.SpecifyKind(Now, DateTimeKind.Unspecified), Now));
    }
}
