# COVERAGE.md (Task 9)

Name: Youssef Maurice | Group / ID: Sunday & Wednesday

## Tools and commands

- Collector: `coverlet.collector` (added to each test project)
- Report: ReportGenerator 5.5.12.0

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:"Html;TextSummary"
```

`TestResults/` and `coveragereport/` are not committed (they are in `.gitignore`).
Only the screenshots below are committed.

## Progress table

Every number is read from the ReportGenerator HTML report (the Coverage table, one row per assembly).
Format: line % (covered/coverable) · branch % (covered/total).

| Step | Date | Domain | Application | Infrastructure | Overall | Screenshot |
| --- | --- | --- | --- | --- | --- | --- |
| Baseline (5 legacy tests only, before Task 1) | 2026-11-02 | 3.9% (4/101) · 0% (0/18) | 7.8% (27/346) · 3.2% (6/186) | 0% (0/200) · 0% (0/20) | 4.7% (31/647) · 2.6% (6/224) | `screenshots/coverage-baseline.png` |
| After Task 1 | | | | | | |
| After Task 5 | | | | | | |
| After Task 6 | | | | | | |
| After Task 7 | | | | | | |
| Final (ConsoleApp excluded) | | | | | | |

## Baseline notes

- The baseline was measured with only `DriveNow.LegacyTests`: 5 tests, all passing.
- The report contains 3 assemblies (Domain, Application, Infrastructure). `DriveNow.ConsoleApp` does not appear because no test project loads it. The exclusion rule in R3 is therefore not needed yet.
- The only well-covered class is `ReceiptService` (100% line, 100% branch). Everything else in Application, Domain and Infrastructure is at or near 0%.

## R3: Exclusions

_To fill in when I apply an exclusion (`.runsettings` or `[ExcludeFromCodeCoverage]`), with the justification._

## R4: Target

Domain and Application: at least 90% line and 85% branch coverage.

_Final numbers to fill in at the end._

## R5: Coverage is not correctness

_One line that was 100% covered while a bug in it was still there (before the fix), and why coverage did not catch it._

## R6: A gap I chose

_One uncovered branch I decided not to test, and why._