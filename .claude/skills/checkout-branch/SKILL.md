---
name: checkout-branch
description: Create and check out a feature branch when currently on master
---

You are currently on the master branch and should be working on a feature branch instead. Create and check out a new branch.

Follow these steps:

1. **Verify you are on master.** Run `git branch --show-current`. If you are not on master, tell the user and stop.

2. **Determine the branch name.**
   - If you know the GitHub issue number for the current work (from context in the conversation or from `$ARGUMENTS`), prefix the branch name with the issue number: `<number>-short-description`.
   - If no issue is associated, just use a descriptive name: `short-description`.
   - Use lowercase kebab-case. Keep it short but descriptive enough to understand the purpose at a glance.
   - **Never** use `/` in the branch name (no `feature/`, `fix/`, etc.).

3. **Present the branch name to the user**, then use the **ask user question tool** to confirm. Use two options: **"Sounds good!"** and **"Wait a minute"**. If the user picks "Wait a minute", wait for their feedback before proceeding.

4. **Create and check out the branch.** Run `git checkout -b <branch-name>`.

5. **Confirm.** Tell the user the new branch name and that you've switched to it.
