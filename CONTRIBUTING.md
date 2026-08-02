# Contributing to Twelve Ranks

Thank you for considering a contribution to Twelve Ranks. The project welcomes
focused contributions from visual designers, developers, testers, translators,
and responsible technology partners.

## Before You Start

1. Read [README.md](README.md), [DEVELOPERS_README.md](DEVELOPERS_README.md),
   [SECURITY.md](SECURITY.md), and [ASSET_LICENSES.md](ASSET_LICENSES.md).
2. Search existing issues before opening a new proposal.
3. Use the relevant issue form before beginning substantial work.
4. Wait for maintainer feedback before investing in a large redesign,
   dependency change, online service, or architectural change.

Opening an issue or submitting work does not create employment, sponsorship,
payment, exclusivity, or an obligation to accept the contribution.

## Visual Design and Artwork

Visual design is the project's highest-priority contribution area. Artwork
proposals should cover a coherent board and piece system rather than an
isolated replacement that cannot be evaluated in context.

Submissions must:

- be original work, or use third-party material that permits modification and
  redistribution in this repository and the Microsoft Store application;
- identify every source, reference, font, tool, and third-party license;
- include editable source files, export instructions, and representative
  raster previews;
- cover light and dark backgrounds, high-DPI scaling, clear silhouettes,
  selection and capture states, and accessibility contrast;
- include the Minister, Chancellor, and Royal Guard treatment alongside the
  standard chess pieces; and
- avoid trademarks, copied game artwork, or material found online without
  documented permission.

Current asset licensing and provenance are documented in
[ASSET_LICENSES.md](ASSET_LICENSES.md). Unless a separate written agreement is
accepted and recorded, contributed project-owned artwork is submitted under
the repository's Apache License 2.0 contribution terms.

## Code, Tests, and Localization

Keep changes focused and preserve the application's offline-first behavior.

- Do not add an AI model, AI service, online multiplayer service, telemetry, or
  a new network dependency without an approved design proposal.
- Explain every new NuGet package and why the existing platform or dependency
  graph cannot meet the requirement.
- Preserve the distinction between the local conventional computer opponent
  and any possible future AI-service opponent.
- Add or update automated tests for game rules and deterministic behavior.
- Keep all 18 localization files aligned when adding or changing user-facing
  strings.
- Preserve save compatibility, accessibility, package identity, and Store
  claims unless an approved change explicitly requires otherwise.
- Never commit credentials, certificates, signing keys, tokens, private
  configuration, generated packages, WACK output, `bin`, or `obj` content.

## Build and Test

From the repository root:

```powershell
dotnet restore .\TwelveRanks.slnx
dotnet build .\TwelveRanks.slnx --configuration Release -p:Platform=x64
dotnet test .\TwelveRanks.Tests\TwelveRanks.Tests.csproj --configuration Release -p:Platform=x64
```

The pull request should state the exact commands run and their results. Do not
claim runtime, packaging, accessibility, or WACK verification unless those
checks were actually completed against the submitted commit.

## Fork and Pull Request Workflow

1. Fork the repository and clone your fork.
2. Create a focused branch from the latest `main`.
3. Make the smallest complete change that addresses the approved issue.
4. Run the relevant build and tests.
5. Commit with a clear summary and push the branch to your fork.
6. Open a pull request referencing the issue and complete the pull request
   checklist.
7. Respond to review feedback without force-pushing over reviewed work unless
   the maintainer requests it.

Maintainers may request revisions or decline work that is incomplete,
unlicensed, unsafe, misleading, outside the project direction, or unsuitable
for the Microsoft Store release.

## Licensing

Unless a submission is conspicuously marked `Not a Contribution` or a separate
written agreement applies, intentionally submitted contributions are provided
under the Apache License 2.0 in [LICENSE](LICENSE), consistent with Section 5 of
that license. Third-party material retains its original license and must be
identified in the contribution and the applicable notices.

## Security and Conduct

Follow [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). Do not place vulnerability
details in a public issue or pull request; use the private process described in
[SECURITY.md](SECURITY.md).
