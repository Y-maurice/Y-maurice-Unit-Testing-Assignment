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
}