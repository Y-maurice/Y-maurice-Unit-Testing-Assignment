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

        return CalculateBaseFee(duration);
    }

    private decimal CalculateBaseFee(TimeSpan duration)
    {
        var totalHours = (int)Math.Ceiling(duration.TotalHours);

        var fullDays = totalHours / 24;
        var remainingHours = totalHours % 24;

        var fee = (fullDays * 80m) + (remainingHours * 10m);

        if (fullDays == 0 && fee > 80m)
        {
            fee = 80m;
        }

        return fee;
    }
}