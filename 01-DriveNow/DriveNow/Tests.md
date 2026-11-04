# TESTS.md

- Name: Youssef Maurice  
- Group:  Sunday & Wednesday

## Feature: RentalMath (Task 1)

### Step 1: My tests (commit `test(rentalmath): my tests`)

| Rule | Test | Boundaries tested |
| --- | --- | --- |
| BR-01 | `CalculateAge_AroundBirthdays_ReturnsFullYears` | Birthday 2000-05-10: day before (2025-05-09 → 24), the day itself (→ 25), day after (→ 25). Leap-day birthday 2000-02-29: 2025-02-27 → 24, 2025-02-28 → 25, and 2024-02-28 → 23 |
| BR-02 | `CalculateLicenseYears_AroundAnniversaries_ReturnsFullYears` | Issue date 2020-06-15: day before (2025-06-14 → 4), the day itself (→ 5), day after (→ 5). Leap-day issue date 2020-02-29: 2025-02-27 → 4, 2025-02-28 → 5 |
| BR-03 | `CalculateRentalDays_AroundGraceHour_ReturnsExpectedDays` | 1499 min (24h 59m) → 1, 1500 min (25h 00m) → 1, 1501 min (25h 01m) → 2, 180 min (3h) → 1 |
| BR-03 | `CalculateRentalDays_ReturnNotAfterPickup_ThrowsArgumentException` | Return equal to pickup (0 min) and before pickup (-1 min) both throw `ArgumentException("Return must be after pickup")` |

### Step 2: My own judgment

**What my tests cover**

- **BR-01 (`CalculateAge`):** I tested each side of a birthday: the day before (one year less), the birthday itself (the full year is complete) and the day after. I repeated the same pattern for a 29 February birthday: 27 February and 28 February in a non-leap year, and 28 February in a leap year (the day before the birthday).
- **BR-02 (`CalculateLicenseYears`):** same approach, with the issue date as the anniversary: the day before, the day itself and the day after, plus the 29 February issue date on 27 and 28 February.
- **BR-03 (`CalculateRentalDays`):** I focused on the grace hour. 1499 minutes (24h 59m) is 1 minute below the 60-minute threshold, so it stays at 1 day. 1500 minutes (25h 00m) leaves exactly 60 minutes, which per the rule does not add a day, so it is still 1 day. 1501 minutes (25h 01m) is the first value over the threshold, so it becomes 2 days. 180 minutes (3h) checks that a short rental is at least 1 day. I also tested that a return equal to or before the pickup throws `ArgumentException`.

**Result of running my tests:** 16 of 17 cases pass. The 1500-minute case fails ("Expected 1, but found 2"). My expectation follows BR-03 and the README's span table (25h 00m → 1), so I believe this is a bug in the code, not a mistake in my test. I have not changed production code. It is a bug candidate for `BUGS.md`.

**My verdict:** I think these are enough because I tested both sides of every boundary described in BR-01 to BR-03 (the day before, the day itself and the day after for birthdays and anniversaries, the 29 February cases, and the three values around the 60-minute grace threshold). I am not completely sure, though: the README's BR-03 table also lists 72h → 3 days, and I did not include that case, so I may be missing exact multiples of 24 hours.