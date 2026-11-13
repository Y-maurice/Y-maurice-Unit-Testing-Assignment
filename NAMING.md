# NAMING.md (Task 0)

Name: Youssef Maurice | Group / ID: Sunday & Wednesday

This is my naming convention for both parts (DriveNow and ParkSmart).
My architecture tests (Task 3) enforce it.

## R1: Test projects, classes and files

- Test projects are named `<Product>.<Kind>Tests`: `DriveNow.UnitTests`, `DriveNow.ArchitectureTests`, `DriveNow.LegacyTests`.
- There is one test class per class under test, named `<ClassUnderTest>Tests`: `RentalMathTests`, `PricingServiceTests`.
- The file name equals the class name: `RentalMathTests.cs`.

## R2: Test method style

Style: `Method_Scenario_ExpectedResult`

Why I chose it:

- It reads left to right as "what is tested, under which condition, what happens".
- It is easy to scan in the test runner, because tests of one method group together.
- It can be checked by a regular expression: `^[A-Za-z0-9]+_[A-Za-z0-9]+_[A-Za-z0-9]+$`.

Three examples from DriveNow:

1. `CalculateAge_BirthdayNotYetThisYear_ReturnsOneYearLess`
2. `CalculateRentalDays_LeftoverExactlyOneHour_DoesNotAddDay`
3. `CreateBooking_BlacklistedCustomer_IsRejected`

Every DriveNow rule test also carries `[Trait("Rule", "BR-xx")]`.

## R3: Test doubles, fakers and customizations

- Hand-written doubles are named `<Kind><Thing>`, where the prefix says the kind of double: `DummyEmailSender`, `StubClock`, `SpySmsSender`, `FakeBookingRepository`.
- Moq mocks are variables named `<thing>Mock` (for example `paymentGatewayMock`).
- Bogus fakers are named `<Entity>Faker`: `CustomerFaker`, `VehicleFaker`.
- AutoFixture customizations are named `<Thing>Customization`: `ValidAdultCustomerCustomization`.
- AutoFixture attributes are named `<Name>DataAttribute`: `AutoMoqDataAttribute`.
- All of these live in a `Support/` folder inside the test project.

## R4: Production code

| Kind | Name rule | Namespace rule |
| --- | --- | --- |
| Interface | starts with `I` | the namespace of the project that owns it |
| Repository class | ends with `Repository` | `...Repositories` |
| Service class | ends with `Service` | `...Services` |
| Exception class | ends with `Exception` | `DriveNow.Domain.Exceptions` |