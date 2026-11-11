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

        var baseFee = CalculateBaseFee(duration, session.Vehicle);

        if (IsNightRate(session.EntryTime, session.ExitTime))
        {
            var flatRate = GetNightFlatRate(session.Vehicle);
            return Math.Min(baseFee, flatRate);
        }

        return baseFee;
    }

    private bool IsNightRate(DateTime entryTime, DateTime exitTime)
    {
        var entryTimeOfDay = entryTime.TimeOfDay;
        var exitTimeOfDay = exitTime.TimeOfDay;

        // Entry at or after 22:00
        var isLateEntry = entryTimeOfDay >= new TimeSpan(22, 0, 0);

        // Exit at or before 08:00 the next morning
        var isEarlyExit = exitTimeOfDay <= new TimeSpan(8, 0, 0) && exitTime.Date > entryTime.Date;

        return isLateEntry && isEarlyExit;
    }

    private decimal GetNightFlatRate(VehicleType vehicle)
    {
        return vehicle switch
        {
            VehicleType.Car => 30m,
            VehicleType.Motorcycle => 15m,
            VehicleType.Truck => 75m,
            _ => throw new ArgumentOutOfRangeException(nameof(vehicle))
        };
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

        if (fullDays == 0 && fee > dailyCap)
        {
            fee = dailyCap;
        }

        return fee;
    }
}