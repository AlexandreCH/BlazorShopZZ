That's excellent news! Using a separate temporary branch (`dev-tmp`) for syncing and conflict resolution is the **safest and most professional way** to handle complex merges.

Here is a summary of the main Git commands you used and the underlying concepts, formatted as a clear Markdown reference file.

---

# 📚 Git Fork Synchronization Reference

This document summarizes the core Git commands used to safely sync a fork with its root (upstream) repository, resolve conflicts, and integrate the changes back into the main development branch.

## 🔗 Phase 1: Setting up the Upstream Remote

The **upstream remote** is a temporary pointer to the original repository.

| Command | Purpose |
| :--- | :--- |
| `git remote -v` | Checks all current remote connections (should show `origin`). |
| `git remote add upstream <URL>` | **Adds** the root repository URL as a new remote named `upstream`. |
| `git fetch upstream` | Downloads the latest commits (e.g., the 26 new ones) from the root repo to your local machine, saving them under the remote-tracking branch (`upstream/master`), **without changing your local branches**. |

---

## 🛡️ Phase 2: Safe Merge and Conflict Resolution

This phase uses a temporary branch (`dev-tmp`) to isolate the synchronization work, preventing direct damage to your primary `master` branch.

| Command | Purpose |
| :--- | :--- |
| `git checkout master` | Ensures you start from the latest state of your primary branch. |
| `git checkout -b dev-tmp` | **Creates** a new branch (`dev-tmp`) from your current location (`master`) and **switches** to it immediately. |
| `git merge upstream/master --no-commit --no-ff` | **Initiates the merge** of the upstream changes into the current branch (`dev-tmp`), but pauses before committing to allow conflict resolution and inspection. The `--no-ff` ensures a merge commit is created. |
| `git status` | Lists all files with **merge conflicts** (listed as "unmerged paths"). |
| `(Resolve Conflicts)` | Manually edit conflicted files, removing the `<<<<<<<`, `=======`, and `>>>>>>>` markers. |
| `git add .` | Stages the resolved files, marking the conflicts as fixed. |
| `git commit -m "Merge upstream changes into dev-tmp"` | **Finalizes the merge commit** on the `dev-tmp` branch. Your fork's history is now safely combined with the root's history. |

---

## 🔄 Phase 3: Final Integration and Cleanup

The final steps integrate the successful sync into your main branch and remove the temporary upstream pointer.

| Command | Purpose |
| :--- | :--- |
| `git checkout master` | Switches back to your primary development branch. |
| `git merge dev-tmp` | **Performs the final, clean merge.** Since the merge and conflict resolution was already done on `dev-tmp`, this will be a fast-forward merge (or a simple, non-conflicting merge commit) into `master`. |
| `git branch -d dev-tmp` | **Deletes the temporary branch** now that its job is complete. (Use `-D` to force deletion if the branch hasn't been fully merged). |
| `git remote remove upstream` | **Removes the pointer** to the root repository, preventing accidental pushes and cleaning up your local config. |
| `git push origin master` | Pushes the newly synchronized history (your 7 commits + 26 upstream commits) up to **your GitHub fork**. |

