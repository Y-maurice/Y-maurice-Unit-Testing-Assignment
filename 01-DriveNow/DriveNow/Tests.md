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


### Step 3: AI review

Screenshot: `screenshots/ai-rentalmath.png` (the prompt from Section 01, with the BR rules and only my test names)

### Step 4: Decide, then act

| # | AI suggestion | Decision | Why |
| --- | --- | --- | --- |
| 1 | BR-01: day before, day of and day after a birthday | Rejected | Already tested with the same three positions (born 2000-05-10 on 2025-05-09, 10 and 11). |
| 2 | BR-01: 29 Feb birthday on 27 and 28 Feb of a non-leap year, and on 28 Feb of a leap year (24, 25, 23) | Rejected | Already in my tests (2025-02-27, 2025-02-28, 2024-02-28). |
| 3 | BR-01: 29 Feb birthday on 29 Feb of a leap year (born 2000-02-29, on 2024-02-29 → 24) | Accepted | The leap-day birthday itself is not in my tests, and BR-01 treats leap and non-leap years differently. |
| 4 | BR-01: first birthday (born 2025-11-04 on 2026-11-03 → 0, on 2026-11-04 → 1) | Accepted | Checks the 0 to 1 boundary; my tests only used ages 23 to 25. |
| 5 | BR-02: day before, day of and day after an anniversary | Rejected | Already tested (issued 2020-06-15 on 2025-06-14, 15 and 16). |
| 6 | BR-02: license issued on 29 Feb, evaluated on 28 Feb of a non-leap year | Rejected | Already tested (2020-02-29 on 2025-02-27 and 2025-02-28). |
| 7 | BR-02: license issued recently (2026-11-03 on 2026-11-04 → 0) and exact first anniversary (2025-11-04 on 2026-11-04 → 1) | Accepted | The 0 to 1 year boundary matters for BR-13 (license at least 1 year old), and my tests never used it. |
| 8 | BR-03: 24h 59m, 25h 00m, 25h 01m | Rejected | Already tested (1499, 1500, 1501 minutes). |
| 9 | BR-03: exact multiples of 24h (24h → 1, 48h → 2, 72h → 3) | Accepted | 72h is a README example I missed, and exact multiples (no leftover time) were never tested. |
| 10 | BR-03: minimum of one day (1 minute → 1) | Accepted | "At least 1" for a very short rental; my shortest case was 3h. |
| 11 | BR-03: 1h, 30 min, 23h 59m → 1 | Rejected | Same result and behaviour as my 3h case and the new 1-minute case; no new boundary. |
| 12 | BR-03: return one tick after pickup → 1 day, no exception | Accepted | The valid side of the "return must be after pickup" boundary; my tests only cover the invalid side. |
| 13 | BR-03: return one minute after pickup → 1 | Rejected | Same input as the 1-minute case in row 10. |

Accepted cases are implemented in a separate commit: `test(rentalmath): AI-suggested cases`.

### Step 5: Judge the AI

I checked the AI's expected values against the README rules and found no wrong value. The problems I found were these:

- **It listed cases I already had as "missing".** The AI only saw my test names, not my data, so it suggested the day before / day of / day after a birthday, the 29 Feb cases on 27 and 28 Feb, the license 29 Feb case and the 24h 59m / 25h 00m / 25h 01m boundary as things to add. I compared each one with my `[InlineData]` and rejected them (rows 1, 2, 5, 6, 8).
- **It repeated itself.** The same scenario appears twice in its BR-01 table ("birthday is tomorrow" and "one day before birthday"), and "1 minute", "23h 59m" and the grace-hour cases appear in more than one of its tables. It also said none of my tests were redundant, while its own lists contain duplicates (rows 11 and 13).
- **It did not suggest checking the grace rule beyond the first day.** All its BR-03 suggestions stay within the first 24-hour block plus one extra hour; none checks that a second block gets the same treatment (for example 49h 00m and 49h 01m).
- **It was right about** the 72h README example, the exact multiples of 24h and the one-tick boundary, which I had missed.