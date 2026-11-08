# ARCHITECTURE.md

- Name: Youssef Maurice 
- Group / ID: sunday & Wednesday

## Result of running the architecture tests on the starter

`dotnet test tests/DriveNow.ArchitectureTests`: 12 tests, 7 pass, 5 fail (screenshot: `screenshots/architecture-failures.png`). The failures are expected: the starter was not built with these rules, and they must all pass after Task 4.

| # | Rule · failing test | Offending type or reference | Why it matters for testing |
| --- | --- | --- | --- |
| 1 | R2: `Application` depends only on `Domain` · `Application_InfrastructureAndConsoleApp_AreNotReferenced` | Four classes import Infrastructure namespaces: `BookingService` (`External`, `Logging`, `Repositories`), `ReturnService` (`External`, `Logging`, `Repositories`), `CancellationService` (`External`, `Logging`, `Repositories`) and `PricingService` (`Repositories`) | The services use the real repositories, the payment gateway and senders (`External`) and the audit log (`Logging`) directly, so a unit test of the business logic cannot replace any of them. That is where the 2 s delay, the random payment failure and the files in `FIRST.md` came from. |
| 2 | R4: repositories end with `Repository` · `Repositories_TypeNames_EndWithRepository` | `Infrastructure.Repositories.CustomerRepo` | The convention is broken for one repository, and `ConsoleApp` uses the odd name (`static readonly CustomerRepo Customers`). Naming rules, doubles (`FakeCustomerRepository`) and the architecture tests cannot treat all repositories the same way until it is renamed. |
| 3 | R4: exceptions live in `Domain.Exceptions` · `Exceptions_Namespace_IsDomainExceptions` | `Infrastructure.External.PaymentFailedException` | BR-26 says a payment failure saves nothing, so a test of `BookingService` needs a fake gateway that throws this exception. If the exception stays in Infrastructure, the Application code and its tests can only know it by referencing Infrastructure, which R2 forbids. |
| 4 | R4: services live in a `Services` namespace · `Services_Namespace_IsServices` | `Application.Receipts.ReceiptService`, `Application.Bookings.CancellationService` | Services spread over `Receipts` and `Bookings` instead of `Services`, so one rule or one search cannot find them all, and it breaks the layout my `NAMING.md` promises. |
| 5 | R4 (extra check): classes in a `Services` namespace end with `Service` · `Services_TypeNames_EndWithService` | `Domain.Services.RentalMath` | <my decision: it is a static calculator, not a service. Either I rename or move it in Task 4, or I remove this test because `NAMING.md` only requires that classes named ...Service live in a Services namespace. Say which, and why.> |

## Rules that pass on the starter

- R1: `Domain` depends on no other DriveNow project.
- R3: `Infrastructure` does not depend on `ConsoleApp`.
- R5: both of my naming tests pass, so my unit tests follow `NAMING.md`.
- `Exceptions_TypeNames_EndWithException` and `Repositories_Namespace_IsRepositories` pass.
- `Interfaces_AllProductionTypes_StartWithI` passes on real data: the starter has four interfaces, all in `Application.Receipts` (`ICurrencyFormatter`, `ILineItemFactory`, `IReceiptFormatter`, `ITaxCalculator`), and all start with `I`. Nothing abstracts the repositories, the payment gateway, the senders, the audit log or the clock, which is what row 1 means in practice: the dependencies that make the code hard to test have no interface yet.