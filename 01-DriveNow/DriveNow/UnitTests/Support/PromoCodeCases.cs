using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using System.Collections;

namespace DriveNow.UnitTests.Support;

/// <summary>One promo scenario: the stored promo, what the customer typed, and what must happen (BR-36).</summary>
public sealed class PromoCase
{
    public required string Name { get; init; }

    /// <summary>The promo stored in the repository (null = the repository is empty).</summary>
    public PromoCode? Stored { get; init; }

    /// <summary>The code the customer typed (null = no code).</summary>
    public required string? TypedCode { get; init; }

    public int RentalDays { get; init; } = 3;

    public VehicleCategory Category { get; init; } = VehicleCategory.Economy;

    /// <summary>The exact exception message, or null when the promo is valid.</summary>
    public string? ExpectedError { get; init; }

    /// <summary>The expected promo discount when the promo is valid (the test vehicle costs 1,000 per day).</summary>
    public decimal ExpectedDiscount { get; init; }

    // Shown as the test case name in the test runner.
    public override string ToString() => Name;
}

/// <summary>
/// Used with [ClassData]: one row per case. Dates are relative to <see cref="TestClock.Today"/>.
/// With a vehicle that costs 1,000 per day, the base amount is 1,000 x RentalDays.
/// BR-36: the first failing check wins, in this order: missing code, dates, minimum days,
/// category, used count.
/// </summary>
public class PromoCodeCases : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var promoCase in All())
            yield return new object[] { promoCase };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    static PromoCode Promo(
        string code = "SAVE15",
        int percent = 15,
        int fromDays = -10,
        int untilDays = 60,
        int minDays = 1,
        VehicleCategory[]? allowed = null,
        int maxUses = 100,
        int used = 0) => new()
        {
            Code = code,
            DiscountPercent = percent,
            ValidFrom = TestClock.Today.AddDays(fromDays),
            ValidUntil = TestClock.Today.AddDays(untilDays),
            MinDays = minDays,
            AllowedCategories = allowed?.ToList(),
            MaxUses = maxUses,
            UsedCount = used
        };

    static IEnumerable<PromoCase> All()
    {
        // No code, or a valid one
        yield return new PromoCase
        {
            Name = "No code typed: no discount",
            Stored = Promo(),
            TypedCode = null,
            ExpectedDiscount = 0m
        };
        yield return new PromoCase
        {
            Name = "Valid code: 15% of the base",
            Stored = Promo(),
            TypedCode = "SAVE15",
            ExpectedDiscount = 450m
        };
        yield return new PromoCase
        {
            Name = "Lookup is case-insensitive",
            Stored = Promo(),
            TypedCode = "save15",
            ExpectedDiscount = 450m
        };

        // Missing code
        yield return new PromoCase
        {
            Name = "Unknown code",
            Stored = null,
            TypedCode = "NOPE",
            ExpectedError = "Promo code 'NOPE' does not exist"
        };

        // Valid dates (inclusive on both ends)
        yield return new PromoCase
        {
            Name = "Valid on its first day",
            Stored = Promo(fromDays: 0),
            TypedCode = "SAVE15",
            ExpectedDiscount = 450m
        };
        yield return new PromoCase
        {
            Name = "Valid on its last day",
            Stored = Promo(untilDays: 0),
            TypedCode = "SAVE15",
            ExpectedDiscount = 450m
        };
        yield return new PromoCase
        {
            Name = "Expired yesterday",
            Stored = Promo(fromDays: -30, untilDays: -1),
            TypedCode = "SAVE15",
            ExpectedError = "Promo code 'SAVE15' has expired"
        };
        yield return new PromoCase
        {
            Name = "Not valid yet (starts tomorrow)",
            Stored = Promo(fromDays: 1, untilDays: 30),
            TypedCode = "SAVE15",
            ExpectedError = "Promo code 'SAVE15' has expired"
        };

        // Minimum days
        yield return new PromoCase
        {
            Name = "Rental shorter than the minimum",
            Stored = Promo(minDays: 7),
            TypedCode = "SAVE15",
            RentalDays = 3,
            ExpectedError = "Promo code 'SAVE15' requires at least 7 days"
        };
        yield return new PromoCase
        {
            Name = "Rental exactly the minimum",
            Stored = Promo(minDays: 3),
            TypedCode = "SAVE15",
            RentalDays = 3,
            ExpectedDiscount = 450m
        };

        // Category
        yield return new PromoCase
        {
            Name = "Category not allowed",
            Stored = Promo(percent: 10, allowed: [VehicleCategory.Luxury]),
            TypedCode = "SAVE15",
            Category = VehicleCategory.Economy,
            ExpectedError = "Promo code 'SAVE15' is not valid for Economy"
        };
        yield return new PromoCase
        {
            Name = "Category allowed: 10% of the base",
            Stored = Promo(percent: 10, allowed: [VehicleCategory.Luxury]),
            TypedCode = "SAVE15",
            Category = VehicleCategory.Luxury,
            ExpectedDiscount = 300m
        };

        // Used count
        yield return new PromoCase
        {
            Name = "Fully used (used count equals the maximum)",
            Stored = Promo(maxUses: 20, used: 20),
            TypedCode = "SAVE15",
            ExpectedError = "Promo code 'SAVE15' has been fully used"
        };
        yield return new PromoCase
        {
            Name = "One use left",
            Stored = Promo(maxUses: 20, used: 19),
            TypedCode = "SAVE15",
            ExpectedDiscount = 450m
        };

        // The first failure wins
        yield return new PromoCase
        {
            Name = "Expired and wrong category: the date check wins",
            Stored = Promo(untilDays: -1, allowed: [VehicleCategory.Luxury]),
            TypedCode = "SAVE15",
            Category = VehicleCategory.Economy,
            ExpectedError = "Promo code 'SAVE15' has expired"
        };
        yield return new PromoCase
        {
            Name = "Too few days and fully used: the minimum-days check wins",
            Stored = Promo(minDays: 7, maxUses: 20, used: 20),
            TypedCode = "SAVE15",
            RentalDays = 3,
            ExpectedError = "Promo code 'SAVE15' requires at least 7 days"
        };
    }
}