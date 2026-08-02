# Twelve Ranks 0.0.1 — Microsoft Store Submission Readiness

Assessment date: 2026-08-02
Package version: `0.0.1.0`
Source commit: `f25897a9d2d117e0dfd4c85ed1919c56381dd416`
Verdict: **READY TO SUBMIT FOR CERTIFICATION**

## Executive result

Version 0.0.1 was rebuilt from a clean, detached worktree at the exact commit
above. The x86, x64, and ARM64 Store bundle build completed with 0 warnings and
0 errors. All 104 automated tests passed. The official Windows Application
Certification Kit reports `OVERALL_RESULT=PASS`.

The final package contains the corrected GitHub repository information and all
18 intended languages in every architecture package. Targeted scans found no
Twelve Ranks AI project, Windows App SDK AI/ML, ONNX, DirectML, tensor, or
`systemAIModels` payload names. The conventional offline computer opponent
remains present under `TwelveRanks.Computer`.

## Partner Center upload

- File:
  `BundleArtifacts\Release-f25897a-Certified\PartnerCenter\TwelveRanks.App_0.0.1.0_x86_x64_ARM64_bundle.msixupload`
- Size: `248,179,216` bytes
- SHA-256:
  `422AB880D5CBBBA63C1CB767FCF59F2BE34592CE15F9F8368E349B890D02E360`
- Identity: `46146A.Sharaf.TwelveRanks12x12Chess`
- Publisher: `CN=739C94CA-22F6-47C5-938D-9709BCF78641`
- Version: `0.0.1.0`
- Architectures: x86, x64, ARM64
- Signing: unsigned Store upload, as expected; Microsoft signs an accepted
  Store submission.

Upload only this `.msixupload` and confirm its SHA-256 immediately beforehand.
The contained unsigned bundle had SHA-256
`5C82EAC768F1E88688D52EB5220917E1024154FC97B9EABC983542E6D77012D7`.

## Build and tests

Visual Studio MSBuild 18.8.2 built the app with `Configuration=Release`,
`AppxBundle=Always`, `AppxBundlePlatforms=x86|x64|ARM64`,
`UapAppxPackageBuildMode=StoreUpload`, `GenerateAppxPackageOnBuild=true`,
`AppxPackageSigningEnabled=false`, and `AppxSymbolPackageEnabled=false`.

Result: **0 warnings, 0 errors; PackageSuccessfullyCreated**.

Build log:

- `TestResults\Store-0.0.1\Release-f25897a\TwelveRanks-0.0.1-f25897a-StoreBundle-build.log`

The test project was explicitly restored and run in Release.

Result: **104 passed, 0 failed, 0 skipped**.

TRX:

- `TestResults\Store-0.0.1\Release-f25897a\TwelveRanks-0.0.1-f25897a-Release.trx`

Automated tests do not prove packaged startup, navigation, save/resume,
language switching, or accessibility behavior.

## Package inspection

The upload contains exactly three architecture packages. Each nested manifest
reports the expected identity, publisher, and version, and embeds these 18
non-empty language declarations:

`en-US`, `ar`, `de`, `es`, `fr`, `hi`, `id`, `it`, `ja`, `ko`, `nl`, `pl`,
`pt-BR`, `tr`, `uk`, `vi`, `zh-Hans`, and `zh-Hant`.

There are no separate split-language packages by design. Each architecture
package embeds the same language resources. The embedded `DEVELOPERS_README.md`
in all three packages contains the corrected repository link:
`https://github.com/VSDeveleoper/TwelveRanks`.

Targeted filename inspection of every nested MSIX found zero AI/ML payload
matches. The source also omits the restricted `systemAIModels` capability and
namespace.

## Windows Application Certification Kit

- Kit: Windows Application Certification Kit `10.0.28000.2114`
- Validation type: command line
- Tested package:
  `BundleArtifacts\Release-f25897a-Certified\WACK\TwelveRanks.App_0.0.1.0_x86_x64_ARM64.msixbundle`
- Tested package SHA-256 after temporary signing:
  `58042787A2CC4A935EFFB1ABF949A21552DA62A154631015EC090E83C3437FC6`
- Report:
  `TestResults\Store-0.0.1\Release-f25897a\WACK\TwelveRanks-0.0.1-f25897a-WACK.xml`
- Report SHA-256:
  `985F06B703BA9580C22DD355CB57341B832ECA173FE7F80052E82858352CD5D1`
- Overall result: **PASS**
- Mandatory tests: **13 passed, 0 failed**
- Optional tests: **9 passed, 2 failed**
- Not-run tests: **0**

Command:

```powershell
appcert.exe test `
  -appxpackagepath "BundleArtifacts\Release-f25897a-Certified\WACK\TwelveRanks.App_0.0.1.0_x86_x64_ARM64.msixbundle" `
  -reportoutputpath "TestResults\Store-0.0.1\Release-f25897a\WACK\TwelveRanks-0.0.1-f25897a-WACK.xml"
```

The optional failures are **General metadata correctness**, for duplicate GUID
findings between WinUI and Windows UI metadata, and **Blocked executables**, for
process-launch APIs and executable-name heuristics in the self-contained
runtime payload. WACK classifies both as optional and still records PASS.

The tested bundle was extracted directly from the final `.msixupload` and
signed with a short-lived, non-exportable test certificate matching the
manifest publisher. The certificate was trusted only in Current User Trusted
People. The private certificate, trusted public certificate, and temporary CER
file were removed afterward; the verified remaining count was zero.

**Never upload the test-signed WACK bundle to Partner Center.**

WACK was authorized to install or activate the app as required. No manual app
launch or interactive runtime testing was performed by the agent. A WACK pass
does not establish broader runtime correctness.

## Readiness gates

| Gate | Status |
|---|---|
| Exact clean source commit | Pass |
| Release x86/x64/ARM64 build | Pass |
| Automated tests | Pass — 104/104 |
| Identity, publisher, and version | Pass |
| Eighteen-language packaging | Pass |
| Restricted `systemAIModels` capability removed | Pass |
| AI/ML package payload removed | Pass |
| `IL2026` and `IL2104` absent from Release build | Pass |
| Mandatory WACK tests | Pass — 13/13 |
| Store-facing repository claims | Pass |
| Runtime smoke test | Owner previously reported testing the app |
| Detailed accessibility validation | Recommended before submission |
| Privacy information and screenshots | Owner-managed Partner Center tasks |

No confirmed local package or mandatory WACK blocker remains.

## Remaining Partner Center tasks

Before pressing Submit, review:

- product reservation and package identity association;
- Games category and age-rating questionnaire;
- privacy/support information and data declarations;
- screenshots and localized descriptions;
- pricing, markets, availability, and hardware requirements;
- `runFullTrust` declaration and justification;
- EULA, third-party notices, and certification notes;
- the upload package hash recorded above.

Suggested `runFullTrust` explanation:

> Twelve Ranks is a packaged WinUI 3 desktop game running at medium integrity.
> `runFullTrust` is used only to run the local desktop application and store
> settings and unfinished games in per-user local application data. The app
> does not request administrator elevation, install services or drivers, access
> enterprise resources, or communicate with remote services.

Also perform keyboard, Narrator, High Contrast, scaling, window-size, Arabic
RTL, CJK, and Latin-layout checks on the packaged Release build.

## GitHub follow-up

After reviewing and committing this report separately, create tag `v0.0.1` at
`f25897a9d2d117e0dfd4c85ed1919c56381dd416` so it identifies the exact package
source. Keep the GitHub release source-only. Do not publish Store packages,
WACK reports, certificates, or generated certification artifacts there.

## Actions not performed

- No Git commit, tag, push, pull request, release, or visibility change.
- No Partner Center upload or submission.
- No manual app launch.
- No certificate or private key was retained.
- No generated evidence was added to source control.
