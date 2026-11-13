# ParkSmart

- Name: Youssef Maurice 
- Group: Sunday & Wednesday

## Business Rules (BR)
- BR-01: Parking for 15 minutes or less is free.
- BR-02: After that, 10 EGP per started hour, counted from entry.
- BR-03: Exit before entry throws ArgumentException with message "Exit time must be after entry time."
- BR-04: Daily cap: each 24-hour block counted from entry costs at most 80. The free 15 minutes apply to the whole session only.
- BR-05: Motorcycles: 5 per hour, cap 40. Trucks: 25 per hour, cap 200.
- BR-06: Night flat rate: entry at or after 22:00 and exit at or before 08:00 the next morning -> the fee is the lower of the normal fee and the flat rate: car 30, motorcycle 15, truck 75.
- BR-07: Lost ticket: the fee is at least 150 (motorcycle 75, truck 300). The free 15 minutes do not apply.
- BR-08: Friday: if entry is on a Friday, the free period is 2 hours instead of 15 minutes. After it, hours still count from entry.
- BR-09: Subscribers park free when entry and exit are on the same day, Sunday to Thursday, between 07:00 and 19:00. Otherwise they get 50% off the fee. The lost-ticket minimum (BR-07) is applied after the discount.
- BR-10: EV charging: add 3.50 per kWh after everything else. It is never discounted or capped. Negative kWh throws ArgumentException.