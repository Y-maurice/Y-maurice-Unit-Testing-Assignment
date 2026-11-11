# TDD Log

- Name: Youssef Maurice 
- Group: Sunday & Wednesday

| Commit | Test name | Why it failed (real failure line) | What you wrote | Refactor |
| --- | --- | --- | --- | --- |
| red(S1) / green(S1) | Calculate_ParkingForFifteenMinutesAndOneSecond_IsNotFree | Expected fee to be 10M, but found 0M. | Added if statement to check if duration <= 15 minutes, return 0, else return 10. | None |
| red(S2) / green(S2) | Calculate_SixtyOneMinutes_ChargesTwenty | Expected fee to be 20M, but found 10M. | Implemented Math.Ceiling for hours. | None |