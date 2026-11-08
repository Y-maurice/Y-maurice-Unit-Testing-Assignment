# HIDDEN-INPUTS.md

- Name: Youssef Maurice 
- Group / ID: Sunday & Wednesday

A hidden input is anything a method depends on that is not passed to it: the clock, randomness, static state, the file system, the console, or a slow external call. I searched `src/` with `Select-String` for `DateTime.Now`, `DateTime.Today`, `Random`, `Thread.Sleep`, `File.`, `Path.`, `Directory.`, `Console.Write`, `static` and `new X()`.

| # | Class | Line | Hidden input | Why it hurts a test | How I removed it |
| --- | --- | --- | --- | --- | --- |
| 1 | `BookingService` | 56 | Clock: `CreatedAt = DateTime.Now` | The value changes on every run, so a test cannot assert BR-24 (created-at is now). | _to do (step 1: TimeProvider)_ |
| 2 | `BookingService` | 125 | Clock: `request.PickupAt < DateTime.Now.AddHours(2)` | The BR-18 boundary (exactly 2 hours from now) cannot be tested, because "now" moves while the test runs. | _to do (step 1)_ |
| 3 | `BookingService` | 89 | Clock: `DateTime.Now < booking.PickupAt.AddHours(-1)` in `StartRental` | The "too early to start" boundary cannot be hit exactly. | _to do (step 1)_ |
| 4 | `BookingService` | 58, 150-156 | Randomness: `RandomCode()` creates `new Random()` for the reference suffix | A test cannot predict the reference (BR-25), so snapshots and exact assertions are impossible. | _to do (step 2: reference generator)_ |
| 5 | `PricingService` | 77 | Clock: `DateTime.Today` for the promo validity dates (BR-36) | A promo that is valid today is expired tomorrow, so the same test fails on another day. | _to do (step 1)_ |
| 6 | `ReturnService` | 37 | Clock: `var now = DateTime.Now` as the actual return time | Late fees (BR-41) depend on how late "now" is, so a test cannot choose the lateness. | _to do (step 1)_ |
| 7 | `ReturnService` | 59-62 | Collaborators created inside the method: `new LineItemFactory()`, `new TaxCalculator()`, `new CurrencyFormatter()`, `new ReceiptFormatter()` | A test cannot replace or observe them. | _to do (step 4, injected receipt service)_ |
| 8 | `CancellationService` | 25, 33 | Clock: `DateTime.Now` for "already started" and the hours until pickup | The BR-51 fee tiers (24 h, 48 h) cannot be tested exactly. | _to do (step 1)_ |
| 9 | `BookingService`, `ReturnService`, `CancellationService`, `PricingService` | <constructors / fields> | Dependencies created by the services themselves (repositories, payment gateway, SMS and email senders, audit log) | There is no way to give the service a fake, so every test runs the real ones. | _to do (steps 4 and 5)_ |
| 10 | `PaymentGateway` | 20 | Slow call: `Thread.Sleep(1500)` | Every booking that charges a deposit takes 1.5 s. | _to do (step 4: IPaymentGateway)_ |
| 11 | `PaymentGateway` | 21 | Randomness: `new Random().Next(10) == 0` makes the payment fail about 1 time in 10 | A test that books with a deposit fails randomly. | _to do (step 4)_ |
| 12 | `PaymentGateway` | 9, 28-31 | Randomness: `RandomCode()` for the payment code | The payment reference cannot be predicted. | _to do (step 4)_ |
| 13 | `PaymentGateway` | 8, 15 | Console output (`[Payment] Charged ...`, `Refunded ...`) | Output inside tests, and nothing to assert on. | _to do (step 4)_ |
| 14 | `SmsSender` | 7, 8 | Slow call `Thread.Sleep(500)` and console output | 0.5 s per SMS, and console noise inside the tests. | _to do (step 4: ISmsSender)_ |
| 15 | `EmailSender` | 7-9, 11 | File system (`outbox` folder under `AppContext.BaseDirectory`, `File.WriteAllText`) and clock in the file name | Tests leave real files behind, and a file name that changes every millisecond. | _to do (step 4: IEmailSender)_ |
| 16 | `AuditLog` | 3, 7-8 | Static class that writes `audit.log` with `File.AppendAllText` and `DateTime.Now` | A static class cannot be replaced, and it writes to a real file during tests. | _to do (step 4: audit log interface and instance)_ |
| 17 | `InMemoryDatabase` | 9-13 | Static shared state: static lists and `NextBookingId` | Every test shares and changes the same data, so tests depend on each other and on run order. | _to do (step 5: instance)_ |
| 18 | `InMemoryDatabase` | 15 | Static constructor that seeds the data once per process | The seed cannot be recreated for a new test. | _to do (step 5)_ |
| 19 | `InMemoryDatabase` | 19-26, 49-83, 99-124 | Clock: `DateTime.Today` in every seeded date | The seed data changes from day to day, so expected values change with it. | _to do (step 5: seed from a given date)_ |
| 20 | `ConsoleApp` `Program` | 17-23 | Static fields that create every service with `new()` | Nothing can be passed in; this is the composition that I will wire by hand. | _to do (step 6)_ |

## Considered, not removed

- `RentalMath` and `LoyaltyService` are static but receive all their inputs as parameters, so they have no hidden input.