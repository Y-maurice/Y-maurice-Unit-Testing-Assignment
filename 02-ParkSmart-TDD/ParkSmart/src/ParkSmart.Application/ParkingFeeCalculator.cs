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

        var hours = Math.Ceiling(duration.TotalHours);
        return (decimal)hours * 10m;
    }
}