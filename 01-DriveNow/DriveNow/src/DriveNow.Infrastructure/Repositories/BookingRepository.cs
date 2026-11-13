using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Infrastructure.Persistence;

namespace DriveNow.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    readonly InMemoryDatabase _database;

    public BookingRepository(InMemoryDatabase database) => _database = database;

    public Booking? GetById(int id) =>
        _database.Bookings.FirstOrDefault(booking => booking.Id == id);

    public List<Booking> GetAll() => _database.Bookings.ToList();

    public void Add(Booking booking)
    {
        booking.Id = _database.NextBookingId++;
        _database.Bookings.Add(booking);
    }

    public void Update(Booking booking)
    {
        var index = _database.Bookings.FindIndex(item => item.Id == booking.Id);
        if (index >= 0)
            _database.Bookings[index] = booking;
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId) =>
        _database.Bookings
            .Where(booking => booking.VehicleId == vehicleId
                && booking.Status is BookingStatus.Reserved or BookingStatus.Active)
            .ToList();

    public int CountOpenBookingsForCustomer(int customerId) =>
        _database.Bookings.Count(booking => booking.CustomerId == customerId
            && booking.Status is BookingStatus.Reserved or BookingStatus.Active);
}