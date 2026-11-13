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
        decimal finalFee = 0m;

        //  Subscriber Free Check
        if (session.IsSubscriber && IsSubscriberFree(session))
        {
            finalFee = 0m;
        }
        else
        {
            var freePeriodMinutes = session.EntryTime.DayOfWeek == DayOfWeek.Friday ? 120 : 15;

            if (duration.TotalMinutes <= freePeriodMinutes && !session.TicketLost)
            {
                return 0m; // Safe to return here 
            }

            var baseFee = CalculateBaseFee(duration, session.Vehicle);
            finalFee = baseFee;

            if (IsNightRate(session.EntryTime, session.ExitTime))
            {
                var flatRate = GetNightFlatRate(session.Vehicle);
                finalFee = Math.Min(baseFee, flatRate);
            }

            //  Subscriber 50% Discount
            if (session.IsSubscriber)
            {
                finalFee = finalFee * 0.5m;
            }
        }

        //  Lost Ticket Minimum (applied after discounts)
        if (session.TicketLost)
        {
            var lostTicketMinimum = session.Vehicle switch
            {
                VehicleType.Car => 150m,
                VehicleType.Motorcycle => 75m,
                VehicleType.Truck => 300m,
                _ => throw new ArgumentOutOfRangeException(nameof(session.Vehicle))
            };
            finalFee = Math.Max(finalFee, lostTicketMinimum);
        }

        return finalFee;
    }
    private bool IsSubscriberFree(ParkingSession session)
    {
        if (session.EntryTime.Date != session.ExitTime.Date) return false;
        if (session.EntryTime.DayOfWeek == DayOfWeek.Friday || session.EntryTime.DayOfWeek == DayOfWeek.Saturday) return false;

        var entryTimeOfDay = session.EntryTime.TimeOfDay;
        var exitTimeOfDay = session.ExitTime.TimeOfDay;

        return entryTimeOfDay >= new TimeSpan(7, 0, 0) && exitTimeOfDay <= new TimeSpan(19, 0, 0);
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