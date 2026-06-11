---
name: evaluate-reviews
description: Evaluate review comments from the conversation and determine which are valid
---

The conversation already contains one or more sets of review comments from previous review steps (e.g. `/review-changes`, `/review-style`). Your job is to evaluate all of them and determine which points are actually valid.

For each review point raised in the conversation:
1. Look at the actual code (not just the diff) to determine whether the point is truly valid
2. Discard any points that are false positives — but keep nitpicks, since even small improvements matter

Remember: we want the best and cleanest possible code long term. We do not want to introduce any technical debt. If something could be improved, it should be improved now — not deferred for later.

Present your evaluation as a list. For each original review point, state whether it is valid or not, and briefly explain why. Do not make any changes to the code.
