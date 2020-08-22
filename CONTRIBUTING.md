# Contributing

When contributing to this repository, please first discuss the change you wish to make via issue, Discord (MichalH#9982), or any other method with the owners of this repository before making a change.

## Pull request process

1. Make sure your code does not contain debug dumps (Such as `Console.WriteLine(...)`.).
2. Make sure your code does not contain TODO comments or code in comments.
3. If you are modifying the database, ensure that existing data is not corrupted. Otherwise, add a migration script or migration app (to PR comment).
4. Your PR must pass the build (and markdown lint).
5. Make sure your commits don't contains tokens, passwords or other vulnerability.
