# Security Policy

## Reporting a Vulnerability

Please report suspected security vulnerabilities through GitHub private
vulnerability reporting:

1. Open the repository's **Security** tab.
2. Select **Advisories**.
3. Select **Report a vulnerability**.

Do not disclose suspected vulnerabilities in public issues, discussions, pull
requests, or social media before they have been reviewed and, where necessary,
corrected.

Include the affected version or commit, Windows version, processor
architecture, reproduction steps, realistic impact, and any relevant logs.
Remove personal information, credentials, tokens, and unrelated system data
before attaching diagnostic material.

The maintainer will assess the report, request additional information when
needed, and coordinate disclosure after an appropriate correction is
available. Submitting a report does not guarantee that it will be classified
as a security vulnerability.

## Supported Versions

Security corrections target the latest Microsoft Store release and the current
`main` branch. Earlier releases and independently modified builds might not
receive corrections.

## System and Scope

Twelve Ranks is a packaged WinUI 3 desktop chess game for Windows. The
repository contains:

- The chess rules and game-state engine in `TwelveRanks.Core`.
- The offline conventional opponent in `TwelveRanks.Computer`.
- The packaged Windows application, local settings, saved-game handling,
  localization, and assets in `TwelveRanks.App`.
- Automated tests in `TwelveRanks.Tests`.
- Store artwork and repository build tooling.

The released application operates offline. It does not currently provide
accounts, cloud synchronization, online multiplayer, remote APIs, telemetry,
or an AI service integration. Future components are not covered until they are
implemented in this repository.

## Threat Model and Trust Boundaries

Security review should consider:

- Malformed, corrupted, or intentionally modified saved-game data.
- Files and paths handled by application-controlled persistence code.
- Package identity, manifest capabilities, signing, and update integrity.
- Native and managed dependencies included in packaged releases.
- Unintended network communication or disclosure of local data.
- Repository, build, and release-pipeline compromise.

Twelve Ranks uses the restricted `runFullTrust` capability required by its
packaged desktop application model. This capability must not be used to access
unrelated user data, bypass operating-system protections, or perform
undocumented background activity.

## Security Invariants

The following properties must remain true:

- Saved-game parsing must reject invalid data safely without executing code,
  escaping the application data directory, or reading arbitrary files.
- Application-controlled writes must remain limited to intended local
  application data and explicitly user-selected locations.
- The application must not transmit game state, settings, identifiers,
  diagnostics, or personal data without an explicit documented feature and
  appropriate user consent.
- Package identity, capabilities, supported architectures, and release
  contents must match the documented product.
- Credentials, signing keys, certificates, access tokens, and private
  configuration must never be committed to the repository.
- Dependency and asset provenance must remain documented, and third-party
  license obligations must be preserved.
- Security validation must fail safely and must not be disabled merely to make
  a build, test, package, or certification check pass.

## Reportable Findings and Severity Context

Reportable findings include realistic paths to:

- Arbitrary code execution or command execution.
- Unauthorized file reading, modification, or deletion.
- Privilege or package-identity boundary violations.
- Exposure of private data or unexpected network transmission.
- Unsafe deserialization or path traversal.
- Compromise of release packages, dependencies, or signing material.
- Persistent denial of service or saved-game corruption caused by plausible
  application inputs.

Severity should reflect realistic reachability and impact. The ability of a
Windows user to modify their own local game data, without crossing another
security boundary or causing additional impact, is not by itself a
vulnerability.

## Out of Scope

The following are normally not security vulnerabilities:

- Chess-rule disagreements, opponent strength, move selection, or gameplay
  balance.
- Cheating in a local game where all players share the same device and trust
  boundary.
- Cosmetic, localization, or accessibility defects without a security impact.
- Problems found only in unsupported, independently modified builds.
- Vulnerabilities exclusively in Windows, .NET, or the Windows App SDK without
  a Twelve Ranks-specific exposure; those should also be reported upstream.
- Hypothetical AI-service or multiplayer risks while those features are not
  present.

No additional accepted security risks or blanket finding exclusions are
declared.

## Known Limitations

Saved games and preferences are stored in the current Windows user's local
application data. They are not intended to contain credentials or other
secrets and are not encrypted against that same Windows user.

External documentation links are handed to Windows for opening in the user's
chosen browser. Windows Application Certification Kit results provide package
compatibility evidence but do not replace source review, dependency review, or
security testing.
