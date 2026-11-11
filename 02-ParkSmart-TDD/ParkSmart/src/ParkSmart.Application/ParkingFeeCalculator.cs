using ParkSmart.Domain;

namespace ParkSmart.Application;

public class ParkingFeeCalculator
{
    public decimal Calculate(ParkingSession session)
    {
        if (session.ExitTime < session.EntryTime)
        {
            throw new ArgumentException("Exit time must be after entry time.");
        }

        var duration = session.ExitTime - session.EntryTime;

        if (duration.TotalMinutes <= 15)
        {
            return 0m;
        }

        return CalculateBaseFee(duration, session.Vehicle);
    }

    private decimal CalculateBaseFee(TimeSpan duration, VehicleType vehicle)
    {
        var hourlyRate = vehicle switch
        {
            VehicleType.Car => 10m,
            VehicleType.Motorcycle => 5m,
            VehicleType.Truck => 25m,
            _ => throw new ArgumentOutOfRangeException(nameof(vehicle))
        };

        var dailyCap = vehicle switch
        {
            VehicleType.Car => 80m,
            VehicleType.Motorcycle => 40m,
            VehicleType.Truck => 200m,
            _ => throw new ArgumentOutOfRangeException(nameof(vehicle))
        };

        var totalHours = (int)Math.Ceiling(duration.TotalHours);

        var fullDays = totalHours / 24;
        var remainingHours = totalHours % 24;

        var fee = (fullDays * dailyCap) + (remainingHours * hourlyRate);

        // Apply cap if it's within the first 24 hours
        if (fullDays == 0 && fee > dailyCap)
        {
            fee = dailyCap;
        }

        return fee;
    }
}