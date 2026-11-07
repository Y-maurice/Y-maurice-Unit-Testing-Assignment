
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.Persistence;

using FluentAssertions;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTests
{
    // Test 1: Happy path
    [Fact]
    [Trait("Rule", "BR-24")]

    public void CreateBooking_ValidRequest_ReturnsBooking()
    {
        var service = new BookingService();

        var pickup = DateTime.Now.AddDays(10);
        var request = new BookingRequest
        {
            CustomerId = 2, // Mariam Adel
            VehicleId = 1,  // Hyundai Accent (Economy)
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        var result = service.CreateBooking(request);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(2);
        result.VehicleId.Should().Be(1);
        result.Status.Should().Be(BookingStatus.Reserved);
        result.PickupAt.Should().Be(request.PickupAt);
        result.ReturnAt.Should().Be(request.ReturnAt);
    }

    // Test 2: Platinum booking
    [Fact]
    [Trait("Rule", "BR-26")]

    public void CreateBooking_PlatinumCustomer_ReturnsZeroDeposit()
    {
        var service = new BookingService();

        var pickup = DateTime.Now.AddDays(12);
        var request = new BookingRequest
        {
            CustomerId = 5, // Tamer Nabil (Platinum)
            VehicleId = 2,  // Kia Rio (Economy)
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(2),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        var result = service.CreateBooking(request);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(5);
        result.VehicleId.Should().Be(2);
        result.Deposit.Should().Be(0m);
        result.Status.Should().Be(BookingStatus.Reserved);
    }

    // Test 3: Overlapping booking rejection
    [Fact]
    [Trait("Rule", "BR-21")]
    public void CreateBooking_OverlappingDates_ThrowsBookingRejectedException()
    {
        var service = new BookingService();

        var existingBooking = InMemoryDatabase.Bookings
            .First(booking => booking.VehicleId == 5 && booking.Status == BookingStatus.Reserved);

        var request = new BookingRequest
        {
            CustomerId = 2, // Mariam Adel
            VehicleId = 5,  // Kia Sportage
            PickupAt = existingBooking.PickupAt.AddHours(1),
            ReturnAt = existingBooking.ReturnAt.AddHours(1),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        Action act = () => service.CreateBooking(request);

        act.Should()
            .Throw<BookingRejectedException>()
            .WithMessage("Vehicle is already booked for these dates");
    }
}
