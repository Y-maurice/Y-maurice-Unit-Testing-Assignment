# TDD Log

- Name: Youssef Maurice 
- Group: Sunday & Wednesday

| Commit | Test name | Why it failed (real failure line) | What you wrote | Refactor |
| --- | --- | --- | --- | --- |
| red(S1) / green(S1) | Calculate_ParkingForFifteenMinutesAndOneSecond_IsNotFree | Expected fee to be 10M, but found 0M. | Added if statement to check if duration <= 15 minutes, return 0, else return 10. | None |
| red(S2) / green(S2) | Calculate_SixtyOneMinutes_ChargesTwenty | Expected fee to be 20M, but found 10M. | Implemented Math.Ceiling for hours. | None |
| red(S3) / green(S3) | Calculate_ExitTimeBeforeEntryTime_ThrowsArgumentException | Expected an ArgumentException, but no exception was thrown. | Added validation check at the start of Calculate. | None |
| red(S4) / green(S4) | Calculate_TenHours_AppliesDailyCapOfEighty | Expected fee to be 80M, but found 100M. | Implemented daily cap logic using full 24h blocks. | Extracted hour calculation logic for readability. |
| refactor(S4) | All S1-S4 tests | N/A | Extracted `CalculateBaseFee` private method to separate validation from math. | Extracted `CalculateBaseFee`. |
| red(S5) / green(S5) | Calculate_VehicleRates_ReturnCorrectFee | Expected fee to be 15M, 200M, 350M, but found 30M, 200M, 300M. | Introduced vehicle-specific hourly rates and daily caps. | None |
| red(S6) / green(S6) | Calculate_NightEntryAndExit_ReturnsLowerFlatRate | Expected fee to be 30M, 15M, 75M, but found 80M, 40M, 200M. | Added night flat rate logic and Math.Min comparison. | None |
| red(S7) / green(S7) | Calculate_LostTicket_ReturnsMinimumOrNormalFeeWhicheverIsHigher | Expected fee to be 150M, but found 0M. | Skip 15-min free rule if TicketLost, apply Math.Max for lost ticket minimum. | None |
| red(S8) / green(S8) | Calculate_FridayEntry_AppliesTwoHourFreePeriod | Expected fee to be 0M, but found 10M. | Added logic to check DayOfWeek for Friday and set free period to 120 mins. | None |
| red(S9) / green(S9) | Calculate_Subscriber_AppliesCorrectDiscountOrFreePeriod | Expected fee to be 0M, but found 10M. | Added subscriber logic to check days/times for free parking, otherwise 50% discount. | None |
| red(S10) / green(S10) | Calculate_EvCharging_AddsCostAfterEverythingElse & Calculate_NegativeKwh_ThrowsArgumentException | Expected fee to be 55M, but found 20M. Expected ArgumentException, but none thrown. | Added validation for negative kWh and added EV charge calculation at the very end. | None |


## Reflection

**Which stage forced the biggest change to earlier code, and what that taught you about designing up front?**

Stage 9 (Subscribers) and Stage 10 (EV Charging) forced the biggest changes to earlier code. In S10, I realized I couldn't just `return 0m` early for free parking because EV charging had to be applied after everything else, even if the parking itself was free. I had to refactor the method to use an `isFree` flag and calculate the final fee at the end. This taught me that designing the order of operations (especially for discounts, minimums, and add-ons) is crucial up front. If I had just rushed to make S1 pass with early returns, the later requirements would have required a complete rewrite of the method.