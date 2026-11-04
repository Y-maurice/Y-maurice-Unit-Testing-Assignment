using DriveNow.Domain.Services;
using FluentAssertions;
using System.Globalization;

namespace DriveNow.UnitTests;

public class RentalMathTests
{
    private static DateTime ParseDate(string date) =>
        DateTime.ParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture
        );

    // BR-01: CalculateAge

    [Theory]
    [Trait("Rule", "BR-01")]

    [InlineData("2000-05-10", "2025-05-09", 24)] // Day before birthday
    [InlineData("2000-05-10", "2025-05-10", 25)] // Birthday
    [InlineData("2000-05-10", "2025-05-11", 25)] // Day after birthday
    [InlineData("2000-02-29", "2025-02-27", 24)] // Leap birthday: before Feb 28
    [InlineData("2000-02-29", "2025-02-28", 25)] // Leap birthday: Feb 28
    [InlineData("2000-02-29", "2024-02-28", 23)] // Leap year: before birthday
    [InlineData("2000-02-29", "2024-02-29", 24)]
    [InlineData("2025-11-04", "2026-11-03", 0)]
    [InlineData("2025-11-04", "2026-11-04", 1)]
    public void CalculateAge_AroundBirthdays_ReturnsFullYears(string dateOfBirth, string onDate, int expected)
    {
        var result = RentalMath.CalculateAge(
            ParseDate(dateOfBirth),
            ParseDate(onDate)
        );

        result.Should().Be(expected);
    }

    // BR-02: CalculateLicenseYears

    [Theory]
    [Trait("Rule", "BR-02")]
    [InlineData("2020-06-15", "2025-06-14", 4)] // Day before anniversary
    [InlineData("2020-06-15", "2025-06-15", 5)] // Anniversary
    [InlineData("2020-06-15", "2025-06-16", 5)] // Day after anniversary
    [InlineData("2020-02-29", "2025-02-27", 4)] // Leap anniversary: before
    [InlineData("2020-02-29", "2025-02-28", 5)] // Leap anniversary: Feb 28
    [InlineData("2026-11-03", "2026-11-04", 0)]
    [InlineData("2025-11-04", "2026-11-04", 1)]// exact first anniversary
    public void CalculateLicenseYears_AroundAnniversaries_ReturnsFullYears(
        string issueDate,
        string onDate,
        int expected)
    {
        var result = RentalMath.CalculateLicenseYears(
            ParseDate(issueDate),
            ParseDate(onDate)
        );

        result.Should().Be(expected);
    }

    // BR-03: CalculateRentalDays


    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(1499, 1)] // 24h 59m
    [InlineData(1500, 1)] // 25h 00m (exactly 60m remainder)
    [InlineData(1501, 2)] // 25h 01m (more than 60m remainder)
    [InlineData(180, 1)]  // 3h
    [InlineData(1440, 1)]
    [InlineData(2880, 2)]
    [InlineData(4320, 3)]
    [InlineData(1, 1)]
    public void CalculateRentalDays_AroundGraceHour_ReturnsExpectedDays(
        int rentalMinutes,
        int expected)
    {
        var pickup = ParseDate("2026-01-01");
        var returnAt = pickup.AddMinutes(rentalMinutes);

        var result = RentalMath.CalculateRentalDays(
            pickup,
            returnAt
        );

        result.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "BR-03")]

    [InlineData(0)]  // Return at pickup time
    [InlineData(-1)] // Return before pickup
    public void CalculateRentalDays_ReturnNotAfterPickup_ThrowsArgumentException(
        int minutesDifference)
    {
        var pickup = ParseDate("2026-01-01");
        var returnAt = pickup.AddMinutes(minutesDifference);

        Action act = () =>
            RentalMath.CalculateRentalDays(pickup, returnAt);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Return must be after pickup");
    }


    [Fact]
    [Trait("Rule", "BR-03")]
    public void CalculateRentalDays_ReturnOneTickAfterPickup_ReturnsOneDay()
    {
        var pickup = ParseDate("2026-01-01");
        var returnAt = pickup.AddTicks(1);

        var result = RentalMath.CalculateRentalDays(pickup, returnAt);

        result.Should().Be(1);
    }
}