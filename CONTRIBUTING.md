# Contributing

Thanks for helping. Bug reports, ideas and pull requests are all welcome.

## Before you start

- For a bug, open an issue with the smallest code that reproduces it.
- For a new feature or a change in behavior, open an issue first so the design can be agreed before you write it.
- Security problems go through [SECURITY.md](SECURITY.md), not public issues.

## Building and testing

You need the .NET SDK pinned in `global.json`.

```
dotnet build
dotnet test
```

The tests run on net8.0, net9.0 and net10.0; all three must pass.

## Pull requests

- Open pull requests against `beta`; `beta` is merged into `master` for releases.
- Keep one change per pull request, with tests for new behavior and for every bug fix.
- The build must have no warnings: StyleCop, Sonar and the public API analyzer run on every build.
- A change to the public API updates `PublicAPI.Unshipped.txt` in the affected project; the analyzer tells you
  the exact lines.
- Add an entry to `CHANGELOG.md` under a new version heading, and update `README.md` when usage changes.
- Breaking changes need a major version; describe them under "Breaking" with migration notes.
