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
}