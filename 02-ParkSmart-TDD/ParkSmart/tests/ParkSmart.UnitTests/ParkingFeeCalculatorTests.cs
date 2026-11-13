using ParkSmart.Application;
using ParkSmart.Domain;

namespace ParkSmart.UnitTests;

public class ParkingFeeCalculatorTests
{
    private readonly ParkingFeeCalculator _calculator = new();

    [Fact]
    public void Calculate_ParkingForExactlyFifteenMinutes_IsFree()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 10, 15, 0),
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(0m);
    }

    [Fact]
    public void Calculate_ParkingForFifteenMinutesAndOneSecond_IsNotFree()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 10, 15, 1),
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(10m);
    }

    [Fact]
    public void Calculate_ExactlySixtyMinutes_ChargesTen()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 11, 0, 0),
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(10m);
    }

    [Fact]
    public void Calculate_SixtyOneMinutes_ChargesTwenty()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 11, 1, 0),
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(20m);
    }
    [Fact]
    public void Calculate_ExitTimeBeforeEntryTime_ThrowsArgumentException()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 09, 59, 0), // 1 min before
            Vehicle = VehicleType.Car
        };

        // Act
        Action act = () => _calculator.Calculate(session);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Exit time must be after entry time.");
    }

    [Fact]
    public void Calculate_TenHours_AppliesDailyCapOfEighty()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 20, 0, 0), // 10 hours
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(80m); // Normally 100, capped at 80
    }

    [Fact]
    public void Calculate_ThirtyHours_ChargesEightyForFirstBlockAndSixtyForRemainder()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 2, 16, 0, 0), // 30 hours
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(140m); // 80 (first 24h) + 60 (6 hours)
    }

    [Fact]
    public void Calculate_FortyEightHoursAndTenMinutes_ChargesOneHundredSeventy()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 3, 10, 10, 0), // 48h 10m
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(170m); // 80 + 80 + 10
    }

    [Theory]
    [InlineData(3, VehicleType.Motorcycle, 15)] // 3 * 5
    [InlineData(10, VehicleType.Truck, 200)]     // 10 * 25 = 250, capped at 200
    [InlineData(30, VehicleType.Truck, 350)]     // 24h cap (200) + 6h (150)
    public void Calculate_VehicleRates_ReturnCorrectFee(int hours, VehicleType vehicle, decimal expectedFee)
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 10, 0, 0).AddHours(hours),
            Vehicle = vehicle
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Theory]
    [InlineData(VehicleType.Car, 30)]
    [InlineData(VehicleType.Motorcycle, 15)]
    [InlineData(VehicleType.Truck, 75)]
    public void Calculate_NightEntryAndExit_ReturnsLowerFlatRate(VehicleType vehicle, decimal expectedFee)
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 23, 0, 0), // 23:00
            ExitTime = new DateTime(2024, 1, 2, 07, 00, 0), // 07:00 next day (8 hours total)
            Vehicle = vehicle
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Theory]
    [InlineData(VehicleType.Car, 5, 150)]        // 5 mins normally free, lost ticket = 150 min
    [InlineData(VehicleType.Motorcycle, 5, 75)]  // 5 mins normally free, lost ticket = 75 min
    [InlineData(VehicleType.Truck, 30, 350)]     // 30 hours normal = 350, min is 300 Returns 350
    public void Calculate_LostTicket_ReturnsMinimumOrNormalFeeWhicheverIsHigher(VehicleType vehicle, int hours, decimal expectedFee)
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 10, 0, 0).AddHours(hours),
            Vehicle = vehicle,
            TicketLost = true
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Theory]
    [InlineData(2024, 1, 5, 2, 0, 0)] // Friday 2 hours exactly -> 0
    [InlineData(2024, 1, 5, 2, 1, 30)] // Friday 2 hours 1 min -> 30 (3 started hours)
    [InlineData(2024, 1, 4, 0, 20, 10)] // Thursday 20 mins -> 10 (not free)
    public void Calculate_FridayEntry_AppliesTwoHourFreePeriod(int year, int month, int day, int hours, int minutes, decimal expectedFee)
    {
        // Arrange
        var entryTime = new DateTime(year, month, day, 10, 0, 0); // 10:00 AM
        var session = new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = entryTime.AddHours(hours).AddMinutes(minutes),
            Vehicle = VehicleType.Car
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Theory]
    // Free conditions
    [InlineData("2024-01-08", 9, 0, "2024-01-08", 17, 0, false, 0)] // Monday 09:00-17:00 -> 0
    // Not free, apply 50%
    [InlineData("2024-01-08", 18, 0, "2024-01-08", 20, 0, false, 10)] // Monday 18:00-20:00 -> 20 -> 10
    [InlineData("2024-01-06", 10, 0, "2024-01-06", 13, 0, false, 15)] // Saturday 3h -> 30 -> 15
    // Lost ticket minimum after discount
    [InlineData("2024-01-08", 10, 0, "2024-01-08", 11, 0, true, 150)] // Monday 1h -> 10 -> 5 -> max(5, 150) = 150
    public void Calculate_Subscriber_AppliesCorrectDiscountOrFreePeriod(
    string entryDate, int entryHour, int entryMin,
    string exitDate, int exitHour, int exitMin,
    bool ticketLost, decimal expectedFee)
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = DateTime.Parse(entryDate).AddHours(entryHour).AddMinutes(entryMin),
            ExitTime = DateTime.Parse(exitDate).AddHours(exitHour).AddMinutes(exitMin),
            Vehicle = VehicleType.Car,
            IsSubscriber = true,
            TicketLost = ticketLost
        };

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Theory]
    [InlineData(2, 10, false, 55)] // 2h (20) + 10kWh (35) = 55
    [InlineData(0, 4, true, 14)]   // Subscriber free (0) + 4kWh (14) = 14
    public void Calculate_EvCharging_AddsCostAfterEverythingElse(int hours, int kwh, bool isSubscriber, decimal expectedFee)
    {
        // Arrange
        var entryTime = new DateTime(2024, 1, 8, 9, 0, 0); // Monday 09:00
        var session = new ParkingSession
        {
            EntryTime = entryTime,
            ExitTime = entryTime.AddHours(hours), // For subscriber free window test 0 hours duration is good
            Vehicle = VehicleType.Car,
            IsSubscriber = isSubscriber,
            ChargedKwh = kwh
        };

        // If duration is 0, need to make sure it doesn't hit the 15 min free rule to test the EV addition properly.
        // if hours = 0, duration is 0, 0 <= 15, so it returns 0. Then EV adds

        // Act
        var fee = _calculator.Calculate(session);

        // Assert
        fee.Should().Be(expectedFee);
    }

    [Fact]
    public void Calculate_NegativeKwh_ThrowsArgumentException()
    {
        // Arrange
        var session = new ParkingSession
        {
            EntryTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ExitTime = new DateTime(2024, 1, 1, 11, 0, 0),
            Vehicle = VehicleType.Car,
            ChargedKwh = -1
        };

        // Act
        Action act = () => _calculator.Calculate(session);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Charged kWh cannot be negative*");
    }
}