---
name: worktree-from-issue
description: Create a new worktree and branch from a GitHub issue
disable-model-invocation: true
---

The user wants to create a new worktree with a new branch based on a GitHub issue. The issue number (e.g. `#72` or `72`) is provided as the argument: `$ARGUMENTS`.

If no issue number argument was provided, ask the user which issue they want.

Follow these steps:

1. **Fetch the GitHub issue** using `gh issue view <number>` to get the issue title and body.

2. **Look at existing branch names** by running `git branch -r --sort=-committerdate` to understand the repo's naming conventions.

3. **Think of a good branch name** that:
   - Starts with the issue number (e.g. `72-descriptive-name`)
   - Uses kebab-case
   - Is concise but clearly describes what the issue is about
   - Matches the style of existing branch names in the repo
   - Has no "/" prefix

4. **Verify that the branch and worktree path (`../ordning-<branch-name>`) aren't already in use** by checking `git branch -r` and `git worktree list`. If either already exists, pick a different name and repeat untill you find one that is free.

5. **Pick the branch name and proceed directly** — do not ask the user to confirm it. Just state the name you chose and continue. For example:
   ```
   Issue #72: "Add export functionality for datasets"
   Branch: 72-dataset-export
   ```

6. **Assign the issue to the user** (if not already assigned to them) by running `gh issue edit <number> --add-assignee @me`. This is idempotent — running it when the user is already an assignee is harmless.

7. **Fetch latest from remote** by running `git fetch origin`.

8. **Create the worktree** from master:
   ```
   git worktree add ../ordning-<branch-name> -b <branch-name> origin/master
   ```

9. **Copy gitignored files to the new worktree** by running the `wi` CLI tool:
   ```
   wi init <absolute-path-to-this-repo> <absolute-path-to-new-worktree>
   ```
   This copies build outputs, `.env` files, and other gitignored files from the current repo to the new worktree so it's ready to build and run. Use absolute paths for both arguments. Run `wi help` for more details on usage.

10. **Tell the user how to start working there.** Print a message like:
   ```
   Worktree ready! To start Claude in the worktree, run:
   cd ../ordning-<branch-name> && claude
   ```
