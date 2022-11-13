# Design

## Performance Optimizations

1. Avoid use of foreach because it allocates an enumerator.

   A. Technically, this is critical only when searching positions.

   B. However, to eliminate the need to reason about code (to determine if a function ever is called inside search loop), avoid all use of foreach.

2. Speed integer division by calculating percentages from 128 instead of 100.

   A. The compiler can leverage bit shifts because x / 128 = x >> 7.

   B. Similar to "foreach" above, to eliminate the need to reason about code, calculate percentages from 128 even if integer division isn't used.

3. Allocate delegates at program startup so they aren't allocated repeatedly via lambda syntax inside search loop.
