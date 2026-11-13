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


## Feature: PricingService (Task 5)

### Step 1: My tests (commit `test(pricing): my tests`)

All the tests are in `UnitTests/PricingServiceTests.cs`. The helpers are in `UnitTests/Support/`: `TestClock` (a fixed "now", Thursday 2026-01-15, through `FakeTimeProvider`), `CustomerFaker` and `VehicleFaker` (Bogus, fixed seed), `FakePromoCodeRepository` (a hand-written fake with a list) and `PromoCodeCases` (the `[ClassData]` provider).

Every test starts from the same ordinary booking (an Economy car at 1,000 per day, a 30-year-old customer with no tier, Basic insurance, no additional drivers, no promo) and changes only what its rule is about, so a failure points at one rule.

| Rule | Test | Cases (tool) |
| --- | --- | --- |
| BR-30 | `Calculate_RentalDays_BaseIsDailyRateTimesDays` | 24 h, 72 h, 25 h 01 m, another daily rate (`[InlineData]`) |
| BR-30 | `Calculate_HalfCentRate_RoundsAwayFromZero` | a rate of 10.125 gives 10.13, not 10.12 |
| BR-31 | `Calculate_PickupWeekday_AddsWeekendSurcharge` | pickup on every weekday, 1 to 3 days, and pickups at 23:00 where the day is counted by when it starts (`[InlineData]`) |
| BR-32 | `Calculate_DriverAge_AddsYoungDriverSurcharge` | ages 20, 21, 24, 25, and birthdays one day before and one day after the pickup date |
| BR-33 | `Calculate_RentalLength_AppliesDurationDiscount` | 6, 7, 13, 14 and 20 days |
| BR-34 | `Calculate_LoyaltyTier_AppliesLoyaltyDiscount` | None, Silver, Gold, Platinum |
| BR-35 | `Calculate_PromoPercent_AppliesPromoDiscount` | 10%, 15%, 20%, 25% |
| BR-36 | `Calculate_PromoCases_ReturnsDiscountOrThrows` | 16 cases from `PromoCodeCases` (`[ClassData]`): no code, valid, case-insensitive, unknown, first and last valid day, expired, not valid yet, minimum days, category, used count, and two cases where the first failing check wins |
| BR-37 | `Calculate_DiscountsAboveCap_ReducesPromo` | totals of 30%, exactly 25%, and 20% with Silver, Gold and no tier |
| BR-38 | `Calculate_InsurancePlanAndCategory_AddsInsuranceCost` | three plans, Luxury doubled, Van and SUV not doubled |
| BR-39 | `Calculate_AdditionalDrivers_AddsDriverCost` | 0 to 3 drivers for every tier (Gold and Platinum get the first driver free) |
| BR-03 | `Calculate_ReturnNotAfterPickup_ThrowsArgumentException` | return equal to or before the pickup |
| BR-40 | `Calculate_FullScenarios_ReturnsExpectedBreakdown` | four whole breakdowns (`[MemberData]`): README examples A and B, and my own examples C and D |
| BR-40 | `Calculate_WorkedExampleB_ReportsEveryWrongField` | example B field by field inside one `AssertionScope` |
| BR-40 | `Calculate_WorkedExampleA_MatchesSnapshot`, `Calculate_WorkedExampleB_MatchesSnapshot` | Verify snapshots |

The expected values come from the README tables and its worked examples, never from the code. Examples C and D are my own, calculated by hand with the README formulas.

**Result of running my tests:** 114 tests in the project, 103 pass and 11 fail. The 1500-minute `RentalMath` case is the old failure from Task 1. The other 10 follow the README and expose three bugs in `PricingService`: (1) BR-33, a 7-day rental gets no duration discount (expected 700, found 0); (2) BR-37, the cap is applied to the loyalty discount and the promo is never cut (for example Gold plus a 10% promo on 10 days: expected promo 500, found 1000); (3) BR-39, Silver customers get a free additional driver (expected 200, found 0). Worked examples B and D fail because of these bugs: B's total is 12,675.00 instead of 12,850.00 (the three errors add up to exactly -175), and D's is 6,890.00 instead of 7,345.00 (the over-sized promo, 455). I have not changed any production code

### Step 2: My own judgment

**Gaps and unclear points**

| Rule | What I see |
| --- | --- |
| BR-37 | When Platinum (15%) is combined with 14 or more days (15%), the duration and loyalty discounts alone are already 30%. The README says to keep duration and loyalty and reduce only the promo, which would leave 30%, above the cap. The rule does not say what to do, so I did not test that combination. |
| BR-31 | <anything I did not cover, for example a rental that crosses the end of a month or a year> |
| BR-36 | <for example: an empty string as the promo code> |

**Why a fixed seed matters (R4).** The fakers use Bogus with a fixed seed, so every run builds the same names, e-mails, plates and ids. A test whose result does not depend on random data is only repeatable if the data cannot change between runs: with a random seed, a failure on one machine could not be reproduced on another, which breaks the "Repeatable" letter of FIRST. In `Calculate_RentalDays_BaseIsDailyRateTimesDays` random data is fine: the customer's name, e-mail and the vehicle's plate and model play no part in the result, and the values that matter (daily rate, number of days) are set explicitly. Random data is not fine for the customer's age in `Calculate_DriverAge_AddsYoungDriverSurcharge`: a faker that picked a random birth date could give a 20-year-old one day and a 25-year-old the next, and the surcharge would appear and disappear. That is why `CustomerFaker.WithAge(...)` computes the birth date from a given age and a given date, and why the other fields stay random but seeded.

**Snapshot or explicit assertions (R6).** A snapshot is better when the result is a large object whose every field matters and that nobody wants to retype, like the whole `PriceBreakdown` of examples A and B: one `Verify` call checks all ten fields, and the `.verified.txt` file shows the full breakdown in the review of the commit. Explicit assertions are better when the test is about one rule or one field (for example only the weekend surcharge): the expected number is written in the test next to its inputs, so a reader sees why it is right, and a failure message names the field. A snapshot also hides a danger: if it is accepted without checking, it records the current output, including a bug. For example B I checked the received file against the README before accepting it.


**My verdict:** I think these tests are enough to expose the main pricing bugs, and the run proved it: they found three disagreements with the README (BR-33, BR-37 and BR-39). Every rule from BR-30 to BR-40 has its own test with both sides of its boundaries (6/7/13/14 days, ages 20/21/24/25, exactly 25% and above the cap), the 16 promo cases, and four whole-breakdown scenarios whose expected values I calculated from the README and not from the code. They are not exhaustive. I know I am missing: (1) the BR-37 case where the duration and loyalty discounts alone already pass 25% (Platinum with 14 days or more), because the README does not say what to do; (2) an empty-string promo code; (3) the rounding of the percentage-based amounts (I only tested the rounding of the base, with a half-cent daily rate); and (4) more combinations of several surcharges and discounts in one booking than my four scenarios.



### Step 3: AI review



### Step 4: Decide, then act

| # | AI suggestion | Decision | Why |
| --- | --- | --- | --- |
| | | | |

Accepted cases are implemented in a separate commit: `test(pricing): AI-suggested cases`.

### Step 5: Judge the AI

### Step 3: AI review

Screenshot: `screenshots/ai-pricing.png` (the prompt from Section 01, with the BR rules and only my test names).

### Step 4: Decide, then act

| # | AI suggestion | Decision | Why |
| --- | --- | --- | --- |
| 1 | BR-03: exactly 60 minutes of leftover, one tick above and below | Rejected | These are the rental-days rule, tested in Task 1 (`RentalMathTests`: 1499, 1500 and 1501 minutes). In pricing, the days only have to feed the base (BR-30 test: 24 h, 72 h, 25 h 01 m, 96 h). |
| 2 | BR-31: Friday, Saturday and Sunday pickups, Thursday for several days, a rental that crosses a weekend without a Friday or Saturday pickup | Rejected | All in my weekday theory (10 rows). |
| 3 | BR-31: pickup late at night, the next day starts 24 hours later and not at midnight | Accepted | I had Thursday 23:00 for 2 days and Saturday 23:00, but not the "starts" rule from both sides. I add Thursday 23:00 for 1 day (0) and Friday 23:00 for 2 days (400). |
| 4 | BR-32: ages 22 and 23, birthday on, before and after the pickup date | Rejected | I test 20, 21, 24 and 25 and four birthday positions; 22 and 23 are in the same class as 21 and 24. |
| 5 | BR-32: young-driver surcharge together with the weekend surcharge | Rejected | Examples B and C combine both. |
| 6 | BR-33 thresholds, all loyalty tiers, loyalty combined with duration and promo | Rejected | Covered: 6, 7, 13, 14 and 20 days; all four tiers; the combinations are in the BR-37 theory and the four whole scenarios. |
| 7 | BR-35: a 0% promo, a decimal percentage, a promo combined with other discounts below the cap | Rejected | `DiscountPercent` is an `int`, so decimals do not exist. 0% is trivial. Below the cap is covered by the BR-37 rows (Silver with 5%, Gold with 5%). |
| 8 | BR-36: each failure alone, case-insensitive lookup, first and last valid day, exactly the minimum days, allowed and disallowed category, null categories, usage limit | Rejected | All are among my 16 `[ClassData]` cases. |
| 9 | BR-36: one day below the minimum | Accepted | My two minimum-days cases (3 of 3, and 3 of 7) did not make an adjacent pair. I add minimum 4 with 3 days. |
| 10 | BR-36: precedence between several failures | Accepted (3 of 5) | I had dates versus category, and minimum days versus used count, which are not neighbours in the order. I add the three missing neighbour pairs: dates versus minimum days, minimum days versus category, category versus used count. "Missing code plus other failures" cannot happen (no promo means nothing else to check), and "expired plus exhausted" is not a neighbour pair. |
| 11 | BR-37: promo alone above the cap, and the case where nothing is left for the promo | Accepted | New test: a 30% promo with no other discount is cut to 25%; 14 days with Gold (15% + 10%) cuts the promo to 0; 14 days with Silver leaves 5%. |
| 12 | BR-37: exact, below and above the cap, only the promo is reduced, total never above 25% | Rejected | Already in my theory (30%, exactly 25%, 20%) with the 25% assertion. |
| 13 | BR-37 table with a 20% duration discount (20% + 0% + 5%, 20% + 0% + 10%) | Rejected | Impossible: the duration discount is 10% or 15% (BR-33). See step 5. |
| 14 | BR-38 and BR-39 matrices (plan by category, tier by number of drivers) | Rejected | Fully tested: 3 plans by Economy and Luxury plus Van and SUV; 4 tiers by 0 to 3 drivers. |
| 15 | BR-40: the breakdown adds up exactly to the total | Accepted | New check on the four scenarios: the reported total equals the sum of the reported components. |
| 16 | Rounding: 10.124, 10.125 and 10.126 | Accepted | The half-cent test becomes a theory with the value below, at and above the

### Step 5: Judge the AI

- **It listed many things I already had as "missing".** It only saw my test names. I compared every suggestion with my data and rejected the ones already covered: the weekday combinations of BR-31, the ages of BR-32, the thresholds of BR-33, all loyalty tiers, all 16 promo cases, and the insurance and driver matrices (rows 2, 4 to 8, 12 and 14).
- **It made a factual mistake.** Its BR-37 table has a duration discount of 20% ("20% + 0% + 5%"), but BR-33 only allows 10% or 15%, so those rows cannot happen. I rejected them (row 13).
- **It suggested cases the README does not support:** a negative daily rate for rounding (-10.125), and a decimal promo percentage (the promo percent is an integer). It also suggested "missing code plus other failures", which cannot happen because with no promo there is nothing else to validate.
- **Its test names break my convention:** names like `Calculate_RentalDays_ExactlySixtyMinutesLeftover_ReturnsCurrentDays` have four parts, and one case per method goes against requirement R1 (`[Theory]` with `[InlineData]`). My own architecture test would fail them.
- **It said `ReportsEveryWrongField` might be redundant with the snapshot.** I kept both: the task requires `AssertionScope` (R5) and a snapshot (R6), and they fail in different ways.
- **It was right** about the neighbouring precedence pairs of BR-36, the case where nothing is left of the cap for the promo, the total equal to the sum of its parts, and the rounding values around the midpoint. I checked each against the README before accepting it.