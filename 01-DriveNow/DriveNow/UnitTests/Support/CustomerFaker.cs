using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Support;


public class CustomerFaker : Faker<Customer>
{
    DateTime _dateOfBirth = TestClock.Today.AddYears(-30);
    DateTime _licenseIssueDate = TestClock.Today.AddYears(-5);
    LoyaltyTier _tier = LoyaltyTier.None;
    bool _blacklisted;

    public CustomerFaker()
    {
        UseSeed(1234);
        RuleFor(c => c.Id, f => f.Random.Int(1, 10_000));
        RuleFor(c => c.FullName, f => f.Name.FullName());
        RuleFor(c => c.Email, f => f.Internet.Email());
        RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("0100#######"));
        RuleFor(c => c.DateOfBirth, _ => _dateOfBirth);
        RuleFor(c => c.LicenseIssueDate, _ => _licenseIssueDate);
        RuleFor(c => c.LoyaltyTier, _ => _tier);
        RuleFor(c => c.IsBlacklisted, _ => _blacklisted);
        RuleFor(c => c.CompletedRentals, _ => 0);
    }

    /// <summary>The customer is exactly this many full years old on the given date (default: today).</summary>
    public CustomerFaker WithAge(int years, DateTime? onDate = null)
    {
        _dateOfBirth = (onDate ?? TestClock.Today).AddYears(-years);
        return this;
    }

    /// <summary>The license is exactly this many full years old on the given date (default: today).</summary>
    public CustomerFaker WithLicenseYears(int years, DateTime? onDate = null)
    {
        _licenseIssueDate = (onDate ?? TestClock.Today).AddYears(-years);
        return this;
    }

    public CustomerFaker WithTier(LoyaltyTier tier)
    {
        _tier = tier;
        return this;
    }

    public CustomerFaker Blacklisted()
    {
        _blacklisted = true;
        return this;
    }

    public Customer Build() => Generate();
}