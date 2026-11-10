using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;

namespace DriveNow.Application.Services;

public class CancellationService
{
    readonly IBookingRepository _bookings;
    readonly IVehicleRepository _vehicles;
    readonly ICustomerRepository _customers;
    readonly IPromoCodeRepository _promos;
    readonly IPaymentGateway _payments;
    readonly ISmsSender _sms;
    readonly IEmailSender _email;
    readonly IAuditLog _audit;
    readonly TimeProvider _time;

    public CancellationService(
        TimeProvider time,
        IPaymentGateway payments,
        ISmsSender sms,
        IEmailSender email,
        IAuditLog audit,
        IBookingRepository bookings,
        IVehicleRepository vehicles,
        ICustomerRepository customers,
        IPromoCodeRepository promos)
    {
        _time = time;
        _payments = payments;
        _sms = sms;
        _email = email;
        _audit = audit;
        _bookings = bookings;
        _vehicles = vehicles;
        _customers = customers;
        _promos = promos;
    }

    public decimal Cancel(int bookingId)
    {
        // Read the clock once; same meaning as DateTime.Now (local time).
        var now = _time.GetLocalNow().DateTime;

        var booking = _bookings.GetById(bookingId)
            ?? throw new BookingNotFoundException($"Booking #{bookingId} not found");
        if (booking.Status != BookingStatus.Reserved)
            throw new InvalidBookingStateException("Only reserved bookings can be cancelled");
        if (now >= booking.PickupAt)
            throw new InvalidBookingStateException("Booking has already started");

        var vehicle = _vehicles.GetById(booking.VehicleId)
            ?? throw new VehicleNotFoundException($"Vehicle #{booking.VehicleId} not found");
        var customer = _customers.GetById(booking.CustomerId)
            ?? throw new CustomerNotFoundException($"Customer #{booking.CustomerId} not found");

        var hours = (booking.PickupAt - now).TotalHours;
        var fee = 0m;
        if (customer.LoyaltyTier != LoyaltyTier.Platinum)
        {
            if (hours > 48)
                fee = 0m;
            else if (hours >= 24)
                fee = Round(vehicle.DailyRate * 0.50m);
            else
                fee = Round(vehicle.DailyRate);
        }

        var refund = Math.Max(booking.Deposit - fee, 0m);
        if (refund > 0)
            _payments.Refund(customer.Id, refund, $"Deposit refund {booking.Reference}");
        if (fee > booking.Deposit)
            _payments.Charge(customer.Id, fee - booking.Deposit, $"Cancellation {booking.Reference}");

        booking.Status = BookingStatus.Cancelled;
        _bookings.Update(booking);

        if (!string.IsNullOrWhiteSpace(booking.PromoCode))
        {
            var promo = _promos.GetByCode(booking.PromoCode);
            if (promo is not null)
            {
                promo.UsedCount--;
                _promos.Update(promo);
            }
        }

        var note = $"Booking {booking.Reference} cancelled";
        _sms.Send(customer.Phone, note);
        _email.Send(customer.Email, note, note);
        _audit.Write(note);
        return fee;
    }

    static decimal Round(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}