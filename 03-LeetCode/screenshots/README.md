# Part C — LeetCode: Binary Search

**My LeetCode profile:** https://leetcode.com/u/youssef_maurice/

## Problem 1 — 875. Koko Eating Bananas

- Accepted submission: https://leetcode.com/problems/koko-eating-bananas/submissions/2194962340

### Explanation

**What I binary-search over.**
I don't search the array. I search over the possible eating speeds `k`.
- `low = 1`, because Koko has to eat at least 1 banana per hour (speed 0 never finishes).
- `high = max(piles)`, because a speed higher than the biggest pile gives no benefit. Every pile already takes 1 hour, so the total can't get any smaller. The answer is therefore always somewhere in `[1, max(piles)]`.

**Why this is binary-searchable.**
As the speed goes up, the total hours needed never goes up. So the speeds form a pattern like `NO NO NO YES YES YES`, where "YES" means Koko can finish within `h` hours. I'm looking for the first YES, which is the minimum speed.

**The condition that decides which half to keep.**
For a candidate speed `mid`, I compute the total hours as the sum of `ceil(pile / mid)` over all piles. I use `(pile + mid - 1) / mid` for the ceiling so there's no floating point, and `long` for the sum to avoid overflow.
- If `hours <= h`, then `mid` works. It might not be the smallest, so I keep it and everything below it by moving `right = mid - 1`.
- If `hours > h`, then `mid` is too slow, and so is everything below it. I discard the lower half with `left = mid + 1`.

**Why the loop ends with the right answer.**
The loop runs while `left <= right`. Every speed less than `left` has been proven too slow, and every speed greater than `right` has been proven to work (or was already checked). When the loop ends, `left = right + 1`, so `left` is the smallest speed that works. That's why I return `left`. It also never ends up outside the valid range, because `max(piles)` always works as a speed.

**Complexity.**
- **Time:** `O(n log m)`, where `n` is the number of piles and `m = max(piles)`. The binary search takes `log m` steps, and each step scans all `n` piles once to count hours.
- **Space:** `O(1)`. It only uses a few variables.

### Trace of Example 1

**Input:** `piles = [3, 6, 7, 11]`, `h = 8`  **Expected output:** `4`

Initial: `low = 1`, `high = 11`

| Step | low | high | mid | Hours at `mid` (per pile) | Total hours | vs `h = 8` | Decision |
|------|-----|------|-----|---------------------------|-------------|------------|----------|
| 1 | 1 | 11 | 6 | 1 + 1 + 2 + 2 | 6 | 6 ≤ 8 ✅ | `mid` works, so `high = mid - 1 = 5` |
| 2 | 1 | 5 | 3 | 1 + 2 + 3 + 4 | 10 | 10 > 8 ❌ | `mid` too slow, so `low = mid + 1 = 4` |
| 3 | 4 | 5 | 4 | 1 + 2 + 2 + 3 | 8 | 8 ≤ 8 ✅ | `mid` works, so `high = mid - 1 = 3` |
| end | 4 | 3 | — | — | — | `low > high` | Stop, return `low = 4` ✅ |

The result is **4**, which matches the expected output.

---

## Problem 2 — 33. Search in Rotated Sorted Array

- Accepted submission: https://leetcode.com/problems/search-in-rotated-sorted-array/submissions/2195551608

### Explanation

**What I binary-search over.**
I binary-search over the **indices** of the array. `left = 0` and `right = nums.Length - 1` at the start, so the range covers the whole array. The target, if it exists, is always inside `[left, right]`.

**The key idea.**
The array was sorted and then rotated, so it isn't sorted as a whole. But when I cut it at `mid`, **at least one of the two halves is always fully sorted**. Only one side can contain the "break" where the large values wrap around to the small ones. So at every step I figure out which half is sorted, then check whether the target falls inside that sorted half.

**The condition that decides which half to keep.**
1. If `nums[mid] == target`, I'm done and return `mid`.
2. If `nums[left] <= nums[mid]`, the **left half** `[left..mid]` is sorted.
   - If `nums[left] <= target < nums[mid]`, the target must be in this sorted half, so `right = mid - 1`.
   - Otherwise it can only be in the other half, so `left = mid + 1`.
3. Else the **right half** `[mid..right]` is sorted.
   - If `nums[mid] < target <= nums[right]`, the target is in this sorted half, so `left = mid + 1`.
   - Otherwise `right = mid - 1`.

The `<=` in `nums[left] <= nums[mid]` matters when `left == mid`, which happens when the range has one or two elements. In that case the "left half" is a single element and counts as sorted.

**Why the loop ends with the right answer.**
Each step throws away a half that I have proven cannot contain the target, so the range `[left, right]` always contains the target if it's in the array. If the target is found, I return its index right away. If the range becomes empty (`left > right`), the target was never in the array, so I return `-1`.

**Complexity.**
- **Time:** `O(log n)`. The search range is cut in half every iteration and each iteration does only constant work.
- **Space:** `O(1)`. It only uses `left`, `right` and `mid`, with no extra data structures.

### Trace of Example 1

**Input:** `nums = [4, 5, 6, 7, 0, 1, 2]`, `target = 0`  **Expected output:** `4`

Indices: `0:4, 1:5, 2:6, 3:7, 4:0, 5:1, 6:2`

| Step | left | right | mid | `nums[mid]` | Sorted half | Check | Decision |
|------|------|-------|-----|-------------|-------------|-------|----------|
| 1 | 0 | 6 | 3 | 7 | Left (`nums[0]=4 ≤ 7`) | Is `4 ≤ 0 < 7`? No | Target not in left half, so `left = mid + 1 = 4` |
| 2 | 4 | 6 | 5 | 1 | Left (`nums[4]=0 ≤ 1`) | Is `0 ≤ 0 < 1`? Yes | Target is in left half, so `right = mid - 1 = 4` |
| 3 | 4 | 4 | 4 | 0 | — | `nums[4] == 0` ✅ | Found, return `4` |

The result is **4**, which matches the expected output.