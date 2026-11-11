using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using DriveNow.UnitTests.Support;
using FluentAssertions;
using FluentAssertions.Execution;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{
    // =====================================================================
    // Helpers. Every test starts from the same ordinary booking and changes
    // only what its rule is about: an Economy car at 1,000 per day, a 30-year-old
    // customer with no tier, Basic insurance, no additional drivers, no promo.
    // =====================================================================

    static DateTime NextWeekday(DayOfWeek day, int hour = 10, int minute = 0)
    {
        var date = TestClock.Today.AddDays(1);
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        return date.AddHours(hour).AddMinutes(minute);
    }

    static Customer CustomerAged(int years, DateTime pickup, LoyaltyTier tier = LoyaltyTier.None) =>
        new CustomerFaker().WithAge(years, pickup.Date).WithTier(tier).Build();

    static Vehicle Car(decimal dailyRate = 1000m, VehicleCategory category = VehicleCategory.Economy) =>
        new VehicleFaker().WithDailyRate(dailyRate).WithCategory(category).Build();

    static PromoCode Promo(int percent, string code = "PROMO") => new()
    {
        Code = code,
        DiscountPercent = percent,
        ValidFrom = TestClock.Today.AddDays(-10),
        ValidUntil = TestClock.Today.AddDays(60),
        MinDays = 1,
        AllowedCategories = null,
        MaxUses = 100,
        UsedCount = 0
    };

    static PriceBreakdown Calculate(
        Customer customer,
        Vehicle vehicle,
        DateTime pickup,
        DateTime returnAt,
        InsurancePlan insurance = InsurancePlan.Basic,
        int additionalDrivers = 0,
        PromoCode? promo = null,
        string? promoCode = null)
    {
        var promos = promo is null ? new FakePromoCodeRepository() : new FakePromoCodeRepository(promo);
        var service = new PricingService(TestClock.Create(), promos);
        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = returnAt,
            InsurancePlan = insurance,
            AdditionalDrivers = additionalDrivers,
            PromoCode = promoCode
        };

        return service.Calculate(customer, vehicle, request);
    }

    // =====================================================================
    // BR-30: base = daily rate x rental days (the days come from BR-03)
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-30")]
    [InlineData(1000, 1440, 1, 1000)] // exactly 24 h
    [InlineData(1000, 4320, 3, 3000)] // 72 h
    [InlineData(1000, 1501, 2, 2000)] // 25 h 01 m: one extra day
    [InlineData(650, 5760, 4, 2600)]  // another daily rate, 96 h
    public void Calculate_RentalDays_BaseIsDailyRateTimesDays(
        int dailyRate, int rentalMinutes, int expectedDays, int expectedBase)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(CustomerAged(30, pickup), Car(dailyRate), pickup, pickup.AddMinutes(rentalMinutes));

        price.RentalDays.Should().Be(expectedDays);
        price.BaseAmount.Should().Be(expectedBase);
    }

    // =====================================================================
    // BR-31: 20% of the daily rate for every rental day that STARTS on Friday or Saturday.
    // Day k starts at pickup + k x 24 hours.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-31")]
    [InlineData(DayOfWeek.Monday, 10, 3, 0)]     // Mon, Tue, Wed
    [InlineData(DayOfWeek.Thursday, 10, 1, 0)]   // Thu
    [InlineData(DayOfWeek.Friday, 10, 1, 200)]   // Fri
    [InlineData(DayOfWeek.Saturday, 10, 1, 200)] // Sat
    [InlineData(DayOfWeek.Sunday, 10, 1, 0)]     // Sun
    [InlineData(DayOfWeek.Thursday, 10, 3, 400)] // Thu, Fri, Sat
    [InlineData(DayOfWeek.Friday, 10, 3, 400)]   // Fri, Sat, Sun
    [InlineData(DayOfWeek.Saturday, 10, 3, 200)] // Sat, Sun, Mon
    [InlineData(DayOfWeek.Thursday, 23, 2, 200)] // day 2 starts Friday 23:00
    [InlineData(DayOfWeek.Saturday, 23, 2, 200)] // day 1 starts Saturday 23:00, day 2 Sunday 23:00
    public void Calculate_PickupWeekday_AddsWeekendSurcharge(
        DayOfWeek pickupDay, int pickupHour, int days, int expectedSurcharge)
    {
        var pickup = NextWeekday(pickupDay, pickupHour);

        var price = Calculate(CustomerAged(30, pickup), Car(1000m), pickup, pickup.AddDays(days));

        price.WeekendSurcharge.Should().Be(expectedSurcharge);
    }

    // =====================================================================
    // BR-32: 15% of the base when the driver is 21 to 24 on the pickup date.
    // birthdayInDays moves the birthday relative to the pickup date:
    // 0 = the birthday is the pickup day, 1 = tomorrow, -1 = yesterday.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-32")]
    [InlineData(20, 0, 0)]    // 20 today
    [InlineData(21, 0, 150)]  // 21 today (lower edge)
    [InlineData(24, 0, 150)]  // 24 today (upper edge)
    [InlineData(25, 0, 0)]    // 25 today
    [InlineData(25, 1, 150)]  // turns 25 tomorrow: still 24 on the pickup date
    [InlineData(21, 1, 0)]    // turns 21 tomorrow: still 20 on the pickup date
    [InlineData(21, -1, 150)] // turned 21 yesterday
    [InlineData(25, -1, 0)]   // turned 25 yesterday
    public void Calculate_DriverAge_AddsYoungDriverSurcharge(
        int birthdayAge, int birthdayInDays, int expectedSurcharge)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);
        var customer = new CustomerFaker().WithAge(birthdayAge, pickup.Date.AddDays(birthdayInDays)).Build();

        var price = Calculate(customer, Car(1000m), pickup, pickup.AddDays(1)); // base = 1,000

        price.YoungDriverSurcharge.Should().Be(expectedSurcharge);
    }

    // =====================================================================
    // BR-33: duration discount of the base: 10% for 7 to 13 days, 15% for 14 days or more.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-33")]
    [InlineData(6, 0)]      // below the first band
    [InlineData(7, 700)]    // first day of the 10% band
    [InlineData(13, 1300)]  // last day of the 10% band
    [InlineData(14, 2100)]  // first day of the 15% band
    [InlineData(20, 3000)]
    public void Calculate_RentalLength_AppliesDurationDiscount(int days, int expectedDiscount)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(CustomerAged(30, pickup), Car(1000m), pickup, pickup.AddDays(days));

        price.DurationDiscount.Should().Be(expectedDiscount);
    }

    // =====================================================================
    // BR-34: loyalty discount of the base: Silver 5%, Gold 10%, Platinum 15%.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-34")]
    [InlineData(LoyaltyTier.None, 0)]
    [InlineData(LoyaltyTier.Silver, 150)]
    [InlineData(LoyaltyTier.Gold, 300)]
    [InlineData(LoyaltyTier.Platinum, 450)]
    public void Calculate_LoyaltyTier_AppliesLoyaltyDiscount(LoyaltyTier tier, int expectedDiscount)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(CustomerAged(30, pickup, tier), Car(1000m), pickup, pickup.AddDays(3)); // base = 3,000

        price.LoyaltyDiscount.Should().Be(expectedDiscount);
    }

    // =====================================================================
    // BR-35: promo discount = the promo percent of the base.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-35")]
    [InlineData(10, 300)]
    [InlineData(15, 450)]
    [InlineData(20, 600)]
    [InlineData(25, 750)] // exactly the cap, nothing is reduced
    public void Calculate_PromoPercent_AppliesPromoDiscount(int percent, int expectedDiscount)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(
            CustomerAged(30, pickup), Car(1000m), pickup, pickup.AddDays(3), // base = 3,000
            promo: Promo(percent), promoCode: "PROMO");

        price.PromoDiscount.Should().Be(expectedDiscount);
    }

    // =====================================================================
    // BR-36: promo validation. The first failing check wins:
    // missing code, dates (inclusive), minimum days, category, used count.
    // The cases and their exact messages are in Support/PromoCodeCases.cs.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-36")]
    [ClassData(typeof(PromoCodeCases))]
    public void Calculate_PromoCases_ReturnsDiscountOrThrows(PromoCase promoCase)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);
        var customer = CustomerAged(30, pickup);
        var vehicle = Car(1000m, promoCase.Category);
        var returnAt = pickup.AddDays(promoCase.RentalDays);

        if (promoCase.ExpectedError is not null)
        {
            var act = () => Calculate(customer, vehicle, pickup, returnAt,
                promo: promoCase.Stored, promoCode: promoCase.TypedCode);

            act.Should().Throw<InvalidPromoCodeException>().WithMessage(promoCase.ExpectedError);
        }
        else
        {
            var price = Calculate(customer, vehicle, pickup, returnAt,
                promo: promoCase.Stored, promoCode: promoCase.TypedCode);

            price.PromoDiscount.Should().Be(promoCase.ExpectedDiscount);
        }
    }

    // =====================================================================
    // BR-37: duration + loyalty + promo never exceed 25% of the base.
    // When the cap applies, duration and loyalty stay and the PROMO is reduced.
    // 10 days (not 7) so this test does not depend on the 7-day edge of BR-33.
    // Base = 10,000, duration discount = 10% = 1,000.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-37")]
    [InlineData(LoyaltyTier.Silver, 15, 500, 1000)]  // 10+5+15 = 30%: promo cut to 10%
    [InlineData(LoyaltyTier.Gold, 5, 1000, 500)]     // 10+10+5 = 25%: exactly the cap, no cut
    [InlineData(LoyaltyTier.Gold, 10, 1000, 500)]    // 10+10+10 = 30%: promo cut to 5%
    [InlineData(LoyaltyTier.None, 20, 0, 1500)]      // 10+0+20 = 30%: promo cut to 15%
    [InlineData(LoyaltyTier.Silver, 5, 500, 500)]    // 10+5+5 = 20%: below the cap, no cut
    public void Calculate_DiscountsAboveCap_ReducesPromo(
        LoyaltyTier tier, int promoPercent, int expectedLoyalty, int expectedPromo)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(
            CustomerAged(30, pickup, tier), Car(1000m), pickup, pickup.AddDays(10),
            promo: Promo(promoPercent), promoCode: "PROMO");

        price.DurationDiscount.Should().Be(1000);
        price.LoyaltyDiscount.Should().Be(expectedLoyalty);
        price.PromoDiscount.Should().Be(expectedPromo);
        (price.DurationDiscount + price.LoyaltyDiscount + price.PromoDiscount)
            .Should().BeLessThanOrEqualTo(2500m);
    }

    // =====================================================================
    // BR-38: insurance per day: Basic 0, Standard 150, Premium 300; Luxury pays double.
    // 2 days.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-38")]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Basic, 0)]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Standard, 300)]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Premium, 600)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Basic, 0)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Standard, 600)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Premium, 1200)]
    [InlineData(VehicleCategory.Van, InsurancePlan.Premium, 600)] // only Luxury doubles
    [InlineData(VehicleCategory.SUV, InsurancePlan.Standard, 300)]
    public void Calculate_InsurancePlanAndCategory_AddsInsuranceCost(
        VehicleCategory category, InsurancePlan plan, int expectedCost)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(
            CustomerAged(30, pickup), Car(1000m, category), pickup, pickup.AddDays(2), insurance: plan);

        price.InsuranceCost.Should().Be(expectedCost);
    }

    // =====================================================================
    // BR-39: 100 per additional driver per day; Gold and Platinum get the first driver free.
    // 2 days.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-39")]
    [InlineData(LoyaltyTier.None, 0, 0)]
    [InlineData(LoyaltyTier.Silver, 0, 0)]
    [InlineData(LoyaltyTier.Gold, 0, 0)]
    [InlineData(LoyaltyTier.Platinum, 0, 0)]
    [InlineData(LoyaltyTier.None, 1, 200)]
    [InlineData(LoyaltyTier.Silver, 1, 200)]   // Silver does NOT get a free driver
    [InlineData(LoyaltyTier.Gold, 1, 0)]       // the first driver is free
    [InlineData(LoyaltyTier.Platinum, 1, 0)]
    [InlineData(LoyaltyTier.None, 2, 400)]
    [InlineData(LoyaltyTier.Silver, 2, 400)]
    [InlineData(LoyaltyTier.Gold, 2, 200)]
    [InlineData(LoyaltyTier.Platinum, 2, 200)]
    [InlineData(LoyaltyTier.Gold, 3, 400)]
    public void Calculate_AdditionalDrivers_AddsDriverCost(
        LoyaltyTier tier, int additionalDrivers, int expectedCost)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(
            CustomerAged(30, pickup, tier), Car(1000m), pickup, pickup.AddDays(2),
            additionalDrivers: additionalDrivers);

        price.AdditionalDriversCost.Should().Be(expectedCost);
    }

    // =====================================================================
    // Rounding: every money value is rounded half away from zero to 2 decimals.
    // 10.125 is exactly half a cent: away from zero gives 10.13 (banker's rounding would give 10.12).
    // =====================================================================

    [Fact]
    [Trait("Rule", "BR-30")]
    public void Calculate_HalfCentRate_RoundsAwayFromZero()
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var price = Calculate(CustomerAged(30, pickup), Car(10.125m), pickup, pickup.AddDays(1));

        price.BaseAmount.Should().Be(10.13m);
        price.Total.Should().Be(10.13m);
    }

    // =====================================================================
    // BR-03 seen from pricing: the rental days come from RentalMath,
    // so a return that is not after the pickup is rejected.
    // =====================================================================

    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(0)]  // return at pickup time
    [InlineData(-1)] // return before pickup
    public void Calculate_ReturnNotAfterPickup_ThrowsArgumentException(int minutesDifference)
    {
        var pickup = NextWeekday(DayOfWeek.Monday);

        var act = () => Calculate(
            CustomerAged(30, pickup), Car(1000m), pickup, pickup.AddMinutes(minutesDifference));

        act.Should().Throw<ArgumentException>().WithMessage("Return must be after pickup");
    }

    // =====================================================================
    // BR-40 and the worked examples: the whole breakdown at once.
    // A and B are the README's own examples (Total 2,070.00 and 12,850.00).
    // C and D are my own combinations, calculated by hand from the README formulas.
    // =====================================================================

    public sealed record PricingScenario(
        string Name,
        decimal DailyRate,
        VehicleCategory Category,
        DateTime Pickup,
        DateTime Return,
        LoyaltyTier Tier,
        int Age,
        InsurancePlan Insurance,
        int Drivers,
        int? PromoPercent,
        PriceBreakdown Expected)
    {
        // Shown as the test case name in the test runner.
        public override string ToString() => Name;
    }

    static PriceBreakdown Expected(
        int days, decimal baseAmount, decimal weekend, decimal young, decimal duration,
        decimal loyalty, decimal promo, decimal insurance, decimal drivers, decimal total) => new()
        {
            RentalDays = days,
            BaseAmount = baseAmount,
            WeekendSurcharge = weekend,
            YoungDriverSurcharge = young,
            DurationDiscount = duration,
            LoyaltyDiscount = loyalty,
            PromoDiscount = promo,
            InsuranceCost = insurance,
            AdditionalDriversCost = drivers,
            Total = total
        };

    // README example A: 600 per day, Sunday 10:00 to Wednesday 10:00, Gold, age 34, Standard, no drivers.
    static PricingScenario ExampleA()
    {
        var pickup = NextWeekday(DayOfWeek.Sunday, 10);
        return new PricingScenario(
            "Example A (README): Gold, 3 days, Standard insurance",
            600m, VehicleCategory.Economy, pickup, pickup.AddDays(3),
            LoyaltyTier.Gold, 34, InsurancePlan.Standard, 0, null,
            Expected(3, 1800m, 0m, 0m, 0m, 180m, 0m, 450m, 0m, 2070m));
    }

    // README example B: SUV 1,500 per day, Thursday 09:00 to the next Thursday 09:30, Silver, age 23,
    // promo SAVE15, Premium insurance, 1 additional driver. The promo is capped (1,575 -> 1,050).
    static PricingScenario ExampleB()
    {
        var pickup = NextWeekday(DayOfWeek.Thursday, 9);
        return new PricingScenario(
            "Example B (README): Silver, 7 days, SAVE15 capped, Premium, 1 driver",
            1500m, VehicleCategory.SUV, pickup, pickup.AddDays(7).AddMinutes(30),
            LoyaltyTier.Silver, 23, InsurancePlan.Premium, 1, 15,
            Expected(7, 10500m, 600m, 1575m, 1050m, 525m, 1050m, 2100m, 700m, 12850m));
    }

    // Mine: Luxury 4,000 per day, Friday 10:00 to Monday 10:00, Platinum, age 22, Premium, 2 drivers.
    static PricingScenario ExampleC()
    {
        var pickup = NextWeekday(DayOfWeek.Friday, 10);
        return new PricingScenario(
            "Example C: Luxury, Platinum, young driver, weekend, 2 drivers",
            4000m, VehicleCategory.Luxury, pickup, pickup.AddDays(3),
            LoyaltyTier.Platinum, 22, InsurancePlan.Premium, 2, null,
            Expected(3, 12000m, 1600m, 1800m, 0m, 1800m, 0m, 1800m, 300m, 15700m));
    }

    // Mine: 650 per day, 14 days from a Wednesday, Silver, age 40, Basic, a 10% promo.
    // 15% duration + 5% loyalty + 10% promo = 30%, so the promo is cut to 5%.
    static PricingScenario ExampleD()
    {
        var pickup = NextWeekday(DayOfWeek.Wednesday, 9);
        return new PricingScenario(
            "Example D: Silver, 14 days, 10% promo cut by the cap",
            650m, VehicleCategory.Economy, pickup, pickup.AddDays(14),
            LoyaltyTier.Silver, 40, InsurancePlan.Basic, 0, 10,
            Expected(14, 9100m, 520m, 0m, 1365m, 455m, 455m, 0m, 0m, 7345m));
    }

    public static IEnumerable<object[]> Scenarios =>
        new[]
        {
            new object[] { ExampleA() },
            new object[] { ExampleB() },
            new object[] { ExampleC() },
            new object[] { ExampleD() }
        };

    static PriceBreakdown Run(PricingScenario scenario)
    {
        var customer = new CustomerFaker()
            .WithAge(scenario.Age, scenario.Pickup.Date)
            .WithTier(scenario.Tier)
            .Build();
        var vehicle = new VehicleFaker()
            .WithDailyRate(scenario.DailyRate)
            .WithCategory(scenario.Category)
            .Build();
        var promo = scenario.PromoPercent is { } percent ? Promo(percent) : null;

        return Calculate(
            customer, vehicle, scenario.Pickup, scenario.Return,
            scenario.Insurance, scenario.Drivers, promo, promo?.Code);
    }

    [Theory]
    [Trait("Rule", "BR-40")]
    [MemberData(nameof(Scenarios))]
    public void Calculate_FullScenarios_ReturnsExpectedBreakdown(PricingScenario scenario)
    {
        var price = Run(scenario);

        price.Should().BeEquivalentTo(scenario.Expected);
    }

    // One AssertionScope: a single run reports EVERY wrong field of example B, not just the first.
    [Fact]
    [Trait("Rule", "BR-40")]
    public void Calculate_WorkedExampleB_ReportsEveryWrongField()
    {
        var price = Run(ExampleB());

        using var scope = new AssertionScope();
        price.RentalDays.Should().Be(7);
        price.BaseAmount.Should().Be(10500m);
        price.WeekendSurcharge.Should().Be(600m);
        price.YoungDriverSurcharge.Should().Be(1575m);
        price.DurationDiscount.Should().Be(1050m);
        price.LoyaltyDiscount.Should().Be(525m);
        price.PromoDiscount.Should().Be(1050m);
        price.InsuranceCost.Should().Be(2100m);
        price.AdditionalDriversCost.Should().Be(700m);
        price.Total.Should().Be(12850m);
    }

    // =====================================================================
    // Snapshots (Verify): the whole breakdown is stored in a .verified.txt file.
    // =====================================================================

    [Fact]
    [Trait("Rule", "BR-40")]
    public Task Calculate_WorkedExampleA_MatchesSnapshot() =>
        Verifier.Verify(Run(ExampleA()));

    [Fact]
    [Trait("Rule", "BR-40")]
    public Task Calculate_WorkedExampleB_MatchesSnapshot() =>
        Verifier.Verify(Run(ExampleB()));
}