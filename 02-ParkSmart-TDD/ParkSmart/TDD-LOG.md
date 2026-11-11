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