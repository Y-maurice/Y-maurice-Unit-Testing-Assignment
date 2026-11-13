# FIRST.md

- Name: Youssef Maurice  
- Group / ID: 20230514

## The naive tests

`BookingServiceTests` (in `Naive/`) has three tests for `BookingService.CreateBooking`, written against the starter code as it is: a happy path (customer 2), a Platinum booking (customer 5) and an overlap rejection (BR-21). They use `DateTime.Now` because the service reads the real clock itself, and `new BookingService()` takes no arguments, so I could not pass anything in.

## My runs

I ran the whole suite (35 tests) 5 times with `dotnet test -c Release --no-build`.

| Run | Duration | Failed tests |
| --- | --- | --- |
| 1 | 5.2 s | 1 (`RentalMath` 1500-minute case, known) |
| 2 | 5.0 s | 1 (same) |
| 3 | 4.8 s | 1 (same) |
| 4 | 4.9 s | 1 (same) |
| 5 | 4.9 s | 1 (same) |

Per-test times (`--logger "console;verbosity=detailed"`): Platinum booking 540 to 800 ms, valid request 2 s, overlap rejection 17 to 41 ms. I also ran the overlap test and the Platinum test alone: both passed with the same result as in the full run.

Screenshot: `screenshots/first-naive-runs.png`

## Fast

- **Verdict:** the naive tests break it.
- **Evidence:** the suite takes 4.8 to 5.2 s, and about 2.8 s of that is two booking tests. The delay is fixed in the production code: `PaymentGateway.cs:20` has `Thread.Sleep(1500)` and `SmsSender.cs:7` has `Thread.Sleep(500)`. A booking that charges a deposit costs 1.5 s + 0.5 s = 2 s (my valid-request test: 2 s). A Platinum booking skips the charge and only pays the SMS delay (540 to 800 ms). An overlap rejection stops before both (17 ms). Ten more tests like the happy path would already use the whole 10 s limit.
- **Production lines:** `PaymentGateway.cs:20`, `SmsSender.cs:7`.

## Independent

- **Verdict:** the naive tests do not break it yet, but only by luck of having three tests.
- **Evidence:** the overlap test and the Platinum test gave the same result alone as in the full run. But all state lives in static members of `InMemoryDatabase` (`Bookings` list, `NextBookingId`), so every booking a test creates stays for all the next tests, and my tests 1 and 3 both use customer 2. A third booking for customer 2 would hit BR-22 (maximum 2 open bookings), and test 3 only works because the seeded booking for vehicle 5 is still in the static list. xUnit also did not run the tests in the order I wrote them (the Platinum test ran first), so any order dependence would show up.
- **Production lines:** `InMemoryDatabase.cs:9-13` (static lists and `NextBookingId`), `InMemoryDatabase.cs:15` (static constructor that seeds them).

## Repeatable

- **Verdict:** the naive tests break it.
- **Evidence:** (1) the payment gateway fails at random: `PaymentGateway.cs:21` has `new Random().Next(10) == 0`, so any test that charges a deposit (my happy path) fails about 1 time in 10 with no change to the code or the test. It did not happen in my 5 full runs (the only failure each time was the known RentalMath case), but nothing prevents it. (2) The reference is random: the same booking got `DN-20261119-N91N`, `DN-20261119-1DR5` and `DN-20261119-V91K` on different runs, so a test cannot assert the reference. (3) The code reads the real clock, so the same test depends on the day it runs, and its dates come from `DateTime.Now`.
- **Production lines:** `PaymentGateway.cs:21` (random failure), `PaymentGateway.cs:9` and `:25-31` (random payment code), `BookingService.cs:58` and `:150-156` (random reference), `BookingService.cs:56` and `:125` (clock).

## Self-validating

- **Verdict:** the tests themselves are self-validating, but the code under test puts side effects around them.
- **Evidence:** each naive test decides pass or fail with FluentAssertions, without anyone reading output. But while they run, the production code prints to the console (`[SMS to 01000000005] Booking ... confirmed`, `[Payment] Charged 1,000.00 EGP to customer #2`) and writes real files (an `audit.log` and email files), which the rules forbid inside a test and which leaves files in the repo.
- **Production lines:** `PaymentGateway.cs:8` and `:15` and `SmsSender.cs:8` (console output), `AuditLog.cs:8` (writes `audit.log`), `EmailSender.cs:9-11` (writes a file per email).

## Timely

- **Verdict:** the naive tests break it.
- **Evidence:** the code was clearly not designed to be tested. `BookingService` creates its own collaborators, `AuditLog` and `InMemoryDatabase` are static classes, and the clock, randomness, delays, console and files are all hard-coded, so a test cannot replace any of them. I had to look up the seeded ids (customers 2 and 5, vehicles 1, 2 and 5), use real dates from `DateTime.Now`, and accept the delay and the random failure, since there was no way to pass a clock, a payment gateway or a database into `new BookingService()`.
- **Production lines:** `BookingService.cs:56`, `:58` and `:125` (hard-coded clock and randomness), `AuditLog.cs:3` and `InMemoryDatabase.cs:7` (static classes).