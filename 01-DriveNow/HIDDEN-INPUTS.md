# HIDDEN-INPUTS.md

- Name: Youssef Maurice 
- Group / ID: Sunday & Wednesday

A hidden input is anything a method depends on that is not passed to it: the clock, randomness, static state, the file system, the console, or a slow external call.

I searched `src/` with `Select-String` for `DateTime.Now`, `DateTime.Today`, `Random`, `Thread.Sleep`, `Task.Delay`, `File.`, `Path.`, `Directory.`, `Console.Write`, `Environment.`, `static` and `new X()`, and then opened every hit in its file.

## Table of hidden inputs

| # | Class | Line | Hidden input | Why it hurts a test | How I removed it |
| --- | --- | --- | --- | --- | --- |
| 1 | `BookingService` | 56 | Clock: `CreatedAt = DateTime.Now` | The value changes on every run, so a test cannot assert BR-24 (created-at is now). | `TimeProvider` is injected through the constructor and the code reads `_time.GetLocalNow()` (local time, the same meaning as `DateTime.Now`). `ConsoleApp` passes `TimeProvider.System`; a test passes a fake clock. Commit `refactor: inject TimeProvider into PricingService and BookingService`. |
| 2 | `BookingService` | 125 | Clock: `request.PickupAt < DateTime.Now.AddHours(2)` | The BR-18 boundary (exactly 2 hours from now) cannot be tested, because "now" moves while the test runs. | Same change as row 1: `RejectVehicle` is no longer static and reads the injected clock. |
| 3 | `BookingService` | 89 | Clock: `DateTime.Now < booking.PickupAt.AddHours(-1)` in `StartRental` | The "too early to start" boundary cannot be hit exactly. | Same change as row 1. |
| 4 | `BookingService` | 58, 150-156 | Randomness: `RandomCode()` creates `new Random()` for the reference suffix | A test cannot predict the reference (BR-25), so exact assertions and snapshots are impossible. | `IReferenceGenerator` (in `DriveNow.Domain.Abstractions`) is injected into `BookingService`. The real `RandomReferenceGenerator` (Infrastructure) keeps the same alphabet, length and `DN-yyyyMMdd-XXXX` format. A test passes a fake that returns a fixed reference. The static `RandomCode()` method is gone. Commit `refactor: inject reference generator into BookingService`. |
| 5 | `PricingService` | 77 | Clock: `DateTime.Today` for the promo validity dates (BR-36) | A promo that is valid today is expired tomorrow, so the same test fails on another day. | `TimeProvider` injected into `PricingService`; the code reads `_time.GetLocalNow().Date`. `BookingService` passes its own clock to the `PricingService` it creates. |
| 6 | `ReturnService` | 37 | Clock: `var now = DateTime.Now` as the actual return time | Late fees (BR-41) depend on how late "now" is, so a test cannot choose the lateness. | `TimeProvider` injected through the constructor; `now` is read from `_time.GetLocalNow().DateTime`. Commit `refactor: inject TimeProvider into ReturnService`. |
| 7 | `ReturnService` | 59-62 | Collaborators created inside the method: `new LineItemFactory()`, `new TaxCalculator()`, `new CurrencyFormatter()`, `new ReceiptFormatter()` | A test cannot replace or observe them. | _to do: the receipt service will be injected_ |
| 8 | `CancellationService` | 25, 33 | Clock: `DateTime.Now` for "already started" and the hours until pickup | The BR-51 fee tiers (24 h, 48 h) cannot be tested exactly, and the two reads of the clock can disagree. | `TimeProvider` injected through the constructor; the clock is read once at the top of `Cancel` and used for both checks. Commit `refactor: inject TimeProvider into CancellationService`. |
| 9 | `BookingService`, `ReturnService`, `CancellationService`, `PricingService` | field initializers at the top of each class | Dependencies created by the services themselves: `CustomerRepo`, `VehicleRepository`, `BookingRepository`, `PromoCodeRepository`, `PaymentGateway`, `SmsSender`, `EmailSender` (`BookingService` also `PricingService`, now built with the injected clock; `ReturnService` has 5 of them, `CancellationService` 7, `PricingService` 1). `AuditLog` is called as a static class. | There is no way to give a service a fake, so every test runs the real ones. | _to do: interfaces and constructor injection_ |
| 10 | `PaymentGateway` | 20 | Slow call: `Thread.Sleep(1500)` | Every booking that charges a deposit takes 1.5 s. | _to do: `IPaymentGateway`_ |
| 11 | `PaymentGateway` | 21 | Randomness: `new Random().Next(10) == 0` makes the payment fail about 1 time in 10 | A test that books with a deposit fails randomly. | _to do: `IPaymentGateway`_ |
| 12 | `PaymentGateway` | 9, 28-31 | Randomness: `RandomCode()` for the payment code | The payment reference cannot be predicted. | _to do: `IPaymentGateway`_ |
| 13 | `PaymentGateway` | 8, 15 | Console output (`[Payment] Charged ...`, `Refunded ...`) | Output inside tests, and nothing to assert on. | _to do: `IPaymentGateway`_ |
| 14 | `SmsSender` | 7, 8 | Slow call `Thread.Sleep(500)` and console output | 0.5 s per SMS, and console noise inside the tests. | _to do: `ISmsSender`_ |
| 15 | `EmailSender` | 7-9, 11 | File system (an `outbox` folder under `AppContext.BaseDirectory`, `Directory.CreateDirectory`, `File.WriteAllText`) and the clock in the file name | Tests leave real files behind, and the file name changes every millisecond. | _to do: `IEmailSender`_ |
| 16 | `AuditLog` | 3, 7-8 | Static class that writes `audit.log` with `File.AppendAllText` and `DateTime.Now` | A static class cannot be replaced, and it writes a real file during tests. | _to do: an audit log interface and an instance_ |
| 17 | `InMemoryDatabase` | 9-13 | Static shared state: static lists and `NextBookingId` | Every test shares and changes the same data, so tests depend on each other and on run order. | _to do: make it an instance_ |
| 18 | `InMemoryDatabase` | 15 | Static constructor that seeds the data once per process | The seed cannot be recreated for a new test. | _to do: seed in an instance constructor_ |
| 19 | `InMemoryDatabase` | 19-26, 49-83, 99-124 | Clock: `DateTime.Today` in every seeded date (customers, promo codes, the two bookings) | The seed data changes from day to day, so expected values change with it (see the evidence below). | _to do: seed from a given date_ |
| 20 | `ConsoleApp` `Program` | 17-23 | Static fields that create every service with `new(...)` | Nothing can be passed in; this is the composition I will wire by hand. | Partly done: the four services now receive `TimeProvider.System`, and `BookingService` also receives `new RandomReferenceGenerator()`. The rest is _to do_. |

## Evidence from my own runs

- **The seed data depends on the date.** I printed the booking list of the console on two different days. On Sunday 2026-11-08 `DN-SEED-0001` cost 4,800.00 and `DN-SEED-0002` cost 12,700.00; on Monday 2026-11-09 the same seed bookings cost 5,100.00 and 11,900.00. Nothing in the code changed between the runs: the seed dates are relative to `DateTime.Today`, and the weekend surcharge (BR-31) counts Fridays and Saturdays, so a different weekday gives a different price (row 19).
- **The refactor keeps the behaviour.** After each step I built the solution, ran the 5 legacy tests (all pass) and compared the console output with the output of the previous commit taken on the same day.

## Decision: the real adapters keep their behaviour

Task 4 says behaviour must not change, so the real `PaymentGateway`, `SmsSender`, `EmailSender` and audit log keep their delays, random failure, console output and files. What changes is that the services only see interfaces, so a test can use a double instead. These rows are marked _to do_ until the interfaces exist.

## Considered, not removed

- `RentalMath` and `LoyaltyService` are static but receive all their inputs as parameters, so they have no hidden input.