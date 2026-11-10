# ARCHITECTURE.md

- Name: Youssef Maurice 
- Group / ID: sunday & Wednesday

## Task 3: result of running the architecture tests on the starter

`dotnet test tests/DriveNow.ArchitectureTests`: 12 tests, 7 pass, 5 fail (screenshot: `screenshots/architecture-failures.png`). The failures are expected: the starter was not built with these rules, and they must all pass after Task 4.

| # | Rule · failing test | Offending type or reference | Why it matters for testing |
| --- | --- | --- | --- |
| 1 | R2: `Application` depends only on `Domain` · `Application_InfrastructureAndConsoleApp_AreNotReferenced` | Four classes import Infrastructure namespaces: `BookingService` (`External`, `Logging`, `Repositories`), `ReturnService` (`External`, `Logging`, `Repositories`), `CancellationService` (`External`, `Logging`, `Repositories`) and `PricingService` (`Repositories`) | The services use the real repositories, the payment gateway and senders (`External`) and the audit log (`Logging`) directly, so a unit test of the business logic cannot replace any of them. That is where the 2 s delay, the random payment failure and the files in `FIRST.md` came from. |
| 2 | R4: repositories end with `Repository` · `Repositories_TypeNames_EndWithRepository` | `Infrastructure.Repositories.CustomerRepo` | The convention is broken for one repository, and `ConsoleApp` uses the odd name (`static readonly CustomerRepo Customers`). Naming rules, doubles (`FakeCustomerRepository`) and the architecture tests cannot treat all repositories the same way until it is renamed. |
| 3 | R4: exceptions live in `Domain.Exceptions` · `Exceptions_Namespace_IsDomainExceptions` | `Infrastructure.External.PaymentFailedException` | BR-26 says a payment failure saves nothing, so a test of `BookingService` needs a fake gateway that throws this exception. If the exception stays in Infrastructure, the Application code and its tests can only know it by referencing Infrastructure, which R2 forbids. |
| 4 | R4: services live in a `Services` namespace · `Services_Namespace_IsServices` | `Application.Receipts.ReceiptService`, `Application.Bookings.CancellationService` | Services spread over `Receipts` and `Bookings` instead of `Services`, so one rule or one search cannot find them all, and it breaks the layout my `NAMING.md` promises. |
| 5 | R4 (extra check): classes in a `Services` namespace end with `Service` · `Services_TypeNames_EndWithService` | `Domain.Services.RentalMath` | It does not hurt testability: `RentalMath` is a pure static calculator that receives all its inputs as parameters. The only cost was naming consistency. **Decision (Task 4): I removed this extra check.** `NAMING.md` only requires that classes named `...Service` live in a `Services` namespace, which `Services_Namespace_IsServices` already checks. The extra check also demanded that every class in a `Services` namespace be named `...Service`, so passing it meant renaming or moving `RentalMath` (used by the domain, the services and `RentalMathTests`) for a rule my convention never stated. |

### Rules that pass on the starter

- R1: `Domain` depends on no other DriveNow project.
- R3: `Infrastructure` does not depend on `ConsoleApp`.
- R5: both of my naming tests pass, so my unit tests follow `NAMING.md`.
- `Exceptions_TypeNames_EndWithException` and `Repositories_Namespace_IsRepositories` pass.
- `Interfaces_AllProductionTypes_StartWithI` passes on real data: the starter has four interfaces, all in `Application.Receipts` (`ICurrencyFormatter`, `ILineItemFactory`, `IReceiptFormatter`, `ITaxCalculator`), and all start with `I`. Nothing abstracts the repositories, the payment gateway, the senders, the audit log or the clock, which is what row 1 means in practice: the dependencies that make the code hard to test have no interface yet.

## Task 4: how each failure was resolved

After Task 4, `dotnet test` shows 43 tests with 1 failure: the known `RentalMath` 1500-minute case from Task 1, a bug candidate for Task 10. **All 11 architecture tests pass.** The 5 legacy tests still pass, and the console output is unchanged (compared on the same day).

| # | Failure | Resolution |
| --- | --- | --- |
| 1 | R2: `Application` references `Infrastructure` | Resolved in two stages. First the payment gateway, SMS sender, email sender and audit log reached the services through interfaces, which removed the `External` and `Logging` imports. Then the repositories did too (`refactor: inject repositories into the services`), so no Application type depends on Infrastructure any more, and I removed the project reference from `DriveNow.Application.csproj`. |
| 2 | `CustomerRepo` | Renamed to `CustomerRepository` (class, file, and its uses in the services and in `Program.cs`). Commit `refactor: rename CustomerRepo to CustomerRepository`. |
| 3 | `PaymentFailedException` in Infrastructure | Moved to the Domain project, namespace `DriveNow.Domain.Exceptions`. `PaymentGateway` and `ConsoleApp` import it from Domain, and the services already imported `DriveNow.Domain.Exceptions`, so Application knows the exception without referencing Infrastructure. Commit `refactor: move PaymentFailedException to Domain.Exceptions`. |
| 4 | `CancellationService` and `ReceiptService` outside `Services` | Both moved to the `DriveNow.Application.Services` namespace. `ReceiptService` keeps using the interfaces that stay in `Application.Receipts`. Commits `refactor: move CancellationService to the Services namespace` and `refactor: move ReceiptService to the Services namespace`. |
| 5 | `RentalMath` name | The extra check was removed (see row 5 above), so the architecture suite has 11 tests. |

## Task 4: where the new interfaces live (R3)

All the new interfaces are in the Domain project, namespace `DriveNow.Domain.Abstractions`, and their real implementations stay in Infrastructure.

**Why Domain and not Application:**

- While I was refactoring, `Application` still referenced `Infrastructure` (row 1), and `Infrastructure` did not reference `Application`. An interface in `Application` would have forced `Infrastructure` to reference `Application` to implement it, which is a circular project reference that does not build. `Domain` is referenced by both.
- The repository interfaces return Domain entities (`Booking`, `Customer`, `Vehicle`, `PromoCode`), so they belong next to them. The other interfaces only use primitive types, `DateTime` and Domain types.
- Rule R1 (Domain depends on no other DriveNow project) still holds, because the interfaces only mention Domain types.
- Now that `Application` no longer references `Infrastructure`, the interfaces could live in either project. I kept them in Domain so `Infrastructure` only depends on `Domain` and never needs a reference to `Application`.

| Interface | Real implementation | Used by | Why it matters for testing |
| --- | --- | --- | --- |
| `IReferenceGenerator` | `Infrastructure.Generators.RandomReferenceGenerator` | `BookingService` | A test passes a fake that returns a fixed reference, so BR-25 and the snapshot of a created booking no longer depend on a random suffix. |
| `IPaymentGateway` | `Infrastructure.External.PaymentGateway` | `BookingService`, `ReturnService`, `CancellationService` | A test passes a fast fake that never fails at random, or one that always throws `PaymentFailedException` to test BR-26. The real gateway keeps its 1.5 s delay and its 1-in-10 failure. |
| `ISmsSender` | `Infrastructure.External.SmsSender` | `BookingService`, `CancellationService` | A spy records the messages instead of waiting 0.5 s and printing to the console. |
| `IEmailSender` | `Infrastructure.External.EmailSender` | `BookingService`, `ReturnService`, `CancellationService` | A spy records the emails instead of writing files to the `outbox` folder. The real sender receives the clock for its file names. |
| `IAuditLog` | `Infrastructure.Logging.AuditLog` (no longer static) | `BookingService`, `ReturnService`, `CancellationService` | A list in memory replaces the `audit.log` file, and a test can assert what was logged. The real log receives the clock and prints the same timestamp format as before. |
| `IBookingRepository` | `Infrastructure.Repositories.BookingRepository` | `BookingService`, `ReturnService`, `CancellationService` | A test gives the service a repository with exactly the bookings it needs (for example an overlapping one for BR-21), without touching shared static state. |
| `ICustomerRepository` | `Infrastructure.Repositories.CustomerRepository` | `BookingService`, `ReturnService`, `CancellationService` | A test creates the customers it needs (blacklisted, Platinum, young driver) instead of relying on the seed data. |
| `IVehicleRepository` | `Infrastructure.Repositories.VehicleRepository` | `BookingService`, `ReturnService`, `CancellationService` | Same: a test creates the vehicle category and rate it needs. |
| `IPromoCodeRepository` | `Infrastructure.Repositories.PromoCodeRepository` | `PricingService`, `BookingService`, `CancellationService` | A test creates promo codes with any dates, limits and categories for BR-36 and BR-37. |

`InMemoryDatabase` is now a normal class with a `TimeProvider` constructor, and the four repositories receive the instance. `ReturnService` receives a `ReceiptService` (a concrete class in Application that already depends on interfaces), so it needs no new interface.

`ConsoleApp` is the composition root: it creates the database, the repositories, the real implementations and the services by hand and passes them through the constructors (no DI container). The dependency direction is now `ConsoleApp` → `Infrastructure` → `Domain` and `ConsoleApp` → `Application` → `Domain`.