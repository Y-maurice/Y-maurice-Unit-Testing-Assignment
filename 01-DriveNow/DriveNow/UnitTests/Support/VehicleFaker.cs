using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Support;

/// <summary>
/// Builds vehicles with Bogus data for the fields a test does not care about
/// (plate, model, mileage) and explicit values for category and daily rate.
/// </summary>
public class VehicleFaker : Faker<Vehicle>
{
    VehicleCategory _category = VehicleCategory.Economy;
    decimal _dailyRate = 500m;
    bool _isAvailable = true;

    public VehicleFaker()
    {
        UseSeed(5678);
        RuleFor(v => v.Id, f => f.Random.Int(1, 10_000));
        RuleFor(v => v.PlateNumber, f => f.Random.Replace("???-####").ToUpperInvariant());
        RuleFor(v => v.Model, f => f.Vehicle.Model());
        RuleFor(v => v.Category, _ => _category);
        RuleFor(v => v.DailyRate, _ => _dailyRate);
        RuleFor(v => v.Mileage, f => f.Random.Int(1_000, 90_000));
        RuleFor(v => v.IsAvailable, _ => _isAvailable);
    }

    public VehicleFaker WithCategory(VehicleCategory category)
    {
        _category = category;
        return this;
    }

    public VehicleFaker WithDailyRate(decimal dailyRate)
    {
        _dailyRate = dailyRate;
        return this;
    }

    public VehicleFaker Unavailable()
    {
        _isAvailable = false;
        return this;
    }

    public Vehicle Build() => Generate();
}