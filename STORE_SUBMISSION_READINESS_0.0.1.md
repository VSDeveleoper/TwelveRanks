# Twelve Ranks 0.0.1 — Microsoft Store Submission Readiness

Assessment date: 2026-08-01  
Assessed package version: `0.0.1.0`  
Overall verdict: **HOLD — not ready to submit**

## Executive result

The current source contains no system AI/ML components, while preserving its
built-in conventional computer opponent. It has 18-language resource parity,
targets Windows Desktop only, and aligns executable and MSIX versions at
`0.0.1.0`. A post-restoration Release x64 build completed with no warnings or
errors, and all 104 automated tests passed.

Do not upload the generated package documented below: it predates the final
metadata corrections and is superseded. Privacy information and screenshots
are owner-managed Partner Center tasks. After creating the release-source
commit, rebuild all three architectures and run WACK against that exact final
package.

## Release artifacts and evidence

### Latest generated Store upload package (superseded)

- File:
  `TwelveRanks.App\AppPackages\TwelveRanks_0.0.1_Store_18Languages_NoAI_LatestSDK_Final\TwelveRanks.App_0.0.1.0_x86_x64_ARM64_bundle.msixupload`
- Size: `249,793,369` bytes
- SHA-256:
  `39447FC855D27047AA743D3D98D4ADC41A6890657AE67820EEC8939AD61CED3D`
- Identity: `46146A.Sharaf.TwelveRanks12x12Chess`
- Publisher: `CN=739C94CA-22F6-47C5-938D-9709BCF78641`

This bundle predates the metadata corrections documented below. It is retained
only as inspection evidence and must not be uploaded to Partner Center. Rebuild
the Store package after the release-source commit, then run WACK against that
exact package.

- Version: `0.0.1.0`
- Architectures: x86, x64, ARM64
- Contents: one MSIX bundle; optional symbol packaging was disabled because
  symbols are not required for the Store upload.
- Signing: unsigned Store upload, as expected; Microsoft signs an accepted MSIX
  Store submission.

The upload contains no separate language resource packages by design. Language
is no longer an automatic split qualifier, so each of the three architecture
application packages embeds the same 18 resources:
`en-US`, `ar`, `de`, `es`, `fr`, `hi`, `id`, `it`, `ja`, `ko`, `nl`, `pl`,
`pt-BR`, `tr`, `uk`, `vi`, `zh-Hans`, and `zh-Hant`.

Nested-container inspection found 18 non-empty language declarations in each
x86, x64, and ARM64 MSIX. All 18 RESW files in that package's source snapshot
had the same 191-key contract. Filename and manifest scans of every nested MSIX
found no
`TwelveRanks.AI`, Windows AI/ML, ONNX, DirectML, tensor, `systemai`, or
`systemAIModels` entry.

The clean x86, x64, and ARM64 package build completed with **0 warnings and
0 errors**. No `IL2026` or `IL2104` diagnostic is present.

### Automated tests

- Command:
  `dotnet test .\TwelveRanks.Tests\TwelveRanks.Tests.csproj --configuration Release --no-restore --no-build`
- Result: **104 passed, 0 failed, 0 skipped**
- Thirteen restored test cases cover the conventional computer opponent,
  including legal selection, tactical captures, seeded opening
  reproducibility, varied near-equivalent opening choices, cancellation,
  fallback behavior, and argument validation.
- Limitation: unit tests do not exercise packaged app startup, navigation,
  save/resume, language switching, accessibility, or manifest contracts.

### WACK

Status for a package rebuilt from the current corrected source: **Not run**.

The following is retained as historical evidence for the preceding candidate,
not certification evidence for the eventual rebuilt package:

- Kit: Windows App Certification Kit `10.0.28000.2114`
- Tested package:
  `TwelveRanks.App\AppPackages\TwelveRanks_0.0.1_WACK\TwelveRanks.App_0.0.1.0_x86_x64_ARM64_WACK.msixbundle`
- Tested package SHA-256:
  `D580B964D9664A835FE3F736715D432C5B14BDE88FE55A146EBA804F2D68D0E0`
- Command:
  `appcert.exe test -appxpackagepath "<signed WACK bundle>" -reportoutputpath "F:\TwelveRanks\TestResults\Store-0.0.1\WACK\TwelveRanks-0.0.1-WACK.xml"`
- Overall result: **PASSED**
- Mandatory tests: passed
- Optional diagnostics: two failed
- `NOT RUN` results: none recorded

Reports:

- `TestResults\Store-0.0.1\WACK\TwelveRanks-0.0.1-WACK.htm`
  (`FF4A610AD8FD89F0630D2B8202EEFA0AFD1E9DA5939D8FB9ACA7BBC291C043A3`)
- `TestResults\Store-0.0.1\WACK\TwelveRanks-0.0.1-WACK.xml`
  (`E9424D114AEC3C3E371400BD5A00E46AE7CB5690C9F94D137CF42A2B009F125B`)
- `TestResults\Store-0.0.1\WACK\TwelveRanks-0.0.1-WACK-trace.txt`
  (`09A05586D3C886F08957AA5E2C839892E096CFCD8A31080DD9EB1E8A3D4D4520`)

The WACK copy was signed with a short-lived, non-exportable test certificate.
The public certificate was temporarily trusted for the run and all copies of
that certificate were removed from the Windows certificate stores afterward.
Never upload the test-signed WACK bundle to Partner Center.

The historical WACK bundle and preceding Store upload bundle had different
container hashes, but all `267` files inside their x64 MSIX payloads were
compared by name, size, and SHA-256 and were identical. That comparison does
not apply to the current source or the eventual rebuilt package.

This WACK version selected the package/static workflow for the historical
candidate. The trace records extraction and analysis of the Release x64 package but no
application activation, and no Twelve Ranks process was observed. The WACK pass
therefore does not prove interactive runtime behavior.

The two optional failures are:

1. **General metadata correctness** — duplicate WinUI/Windows UI GUID findings
   and invalid `MethodImpl` metadata in
   `Microsoft.Windows.Internal.AI.Generative*.winmd`.
2. **Blocked executables** — process-launch APIs and executable-name heuristics
   found in the self-contained .NET, WinUI, Windows App SDK, DirectML, ONNX, and
   app payload. The report treats this as an informational Windows S-mode
   diagnostic.

These failures do not change the historical WACK result. The AI/ML payload
identified by that run is absent from the restored source graph and the
superseded AI-free package, but only a fresh WACK run can confirm the resulting
diagnostics for the final rebuilt package.

## Readiness gates

| Gate | Status | Evidence / disposition |
|---|---|---|
| Release x64 source build | Pass | Post-metadata build completed with 0 warnings and 0 errors. |
| Store upload package | **Stale / rebuild required** | The existing package predates the metadata corrections and must not be uploaded. |
| Package identity, publisher, version | **Source pass; package verification pending** | Manifest and Partner Center identity agree; executable and MSIX versions are aligned at `0.0.1.0`. |
| x86, x64, ARM64 bundle | **Rebuild required** | All architectures are configured, but the prior bundle is superseded. |
| Core and conventional-opponent automated tests | Pass | 104/104 tests passed, including 13 conventional-opponent cases. |
| Mandatory WACK checks | **Stale / rerun required** | The pass applies to the preceding package, not the corrected candidate. |
| Package localization packaging | **Source pass; package verification pending** | The prior bundle proved the split configuration; the final rebuilt package must be reinspected. |
| Translation completeness | Pass | All 18 RESW files contain the same 207 active keys, including the exact restored conventional-opponent values from the retained PRI. |
| Restricted capabilities | **Source pass; package verification pending** | `systemAIModels` and its manifest namespace were removed; `runFullTrust` remains. |
| AI/ML package payload | **Source graph pass; package verification pending** | The conventional opponent has no NuGet dependencies; the resolved app graph and superseded nested payloads contain no AI/ML, ONNX, DirectML, or tensor package/file. |
| Release save/load safety | **Code/build resolved; runtime check pending** | Source-generated JSON is used, Release trimming is disabled, and the clean package build has no IL warnings. |
| Privacy information and screenshots | **Partner Center task — user managed** | The owner will enter privacy information and upload screenshots during submission. |
| Store claims match product | Pass | Board-color and runtime-dependency claims were corrected; the open-source claim is now supported by the root Apache 2.0 license. |
| Runtime smoke/accessibility | **Not verified** | WACK did not activate the app; user-operated checks are still required. |
| Reproducible release source | **Needs correction** | Package was built from an extensively dirty worktree. |

## Submission-intervening findings

### 1. Language packaging and translation parity

Severity: **Resolved in source; final package verification pending**

- `README.md` and `TwelveRanks.App\TwelveRanks.App.csproj` advertise an
  18-language localized UI.
- The superseded AI-free bundle demonstrated that all 18 language resources can
  be embedded in every architecture application package. The final rebuilt
  bundle must be reinspected after the resource-metadata corrections.
- The 31 newer Rules, resume, game-mode, and save-error strings are translated
  in all 17 non-English files.
- The 19 conventional-opponent resources were restored from the retained
  18-language package PRI, and two affected existing values were realigned to
  that same source.
- All 18 RESW files contain exactly 207 active keys. No missing, extra,
  duplicate, or future-AI UI keys were found.

MSBuild setting retained in the current source:

```xml
<AppxBundleAutoResourcePackageQualifiers>
  Scale|DXFeatureLevel
</AppxBundleAutoResourcePackageQualifiers>
```

This keeps the app's language resources inside each architecture package
instead of sending them through the faulty split-language-pack filter. The
superseded bundle verified that generated split configurations omit `Language`,
all three nested manifests declare the intended 18 languages, and the x64 PRI
contains candidates for those languages. Repeat that inspection on the final
rebuilt bundle.

### 2. Apparently unused restricted `systemAIModels` capability

Severity: **Resolved in source; final package verification pending**

- The `systemai` manifest namespace, its ignorable-namespace entry, and the
  restricted `systemAIModels` capability were removed.
- The source manifest and all three manifests in the superseded AI-free bundle
  contain neither `systemai` nor `systemAIModels`.
- No Partner Center restricted-capability approval is needed for a system AI
  model capability that is no longer declared.

Keep `runFullTrust`: it is expected for this packaged medium-integrity WinUI
desktop application. Suggested Partner Center explanation:

> Twelve Ranks is a packaged WinUI 3 desktop game running at medium integrity.
> `runFullTrust` is used only to run the local desktop application and store
> settings and unfinished games in per-user local application data. The app
> does not request administrator elevation, install services or drivers, access
> enterprise resources, or communicate with remote services.

### 3. Unused Windows AI/ML payload

Severity: **Resolved in source; final package verification pending**

- The former AI project remains removed. The conventional opponent was restored
  under the accurately named `TwelveRanks.Computer` project, which references
  only `TwelveRanks.Core` and has no NuGet dependencies.
- Its local UI/runtime paths, save mode, 18-language resources, and 13 automated
  test cases are restored. No future AI-service game mode is present.
- The aggregate Windows App SDK package is current at 2.3.1. Runtime 2.3.1 and
  WinUI 2.3.2 are referenced as explicit components.
- .NET 10/NuGet package pruning removes the aggregate package's unused AI, ML,
  DWrite, and Widgets feature branches and their transitive dependencies from
  the restored graph.
- The restored graph contains no Windows App SDK AI/ML, Windows Machine
  Learning, ONNX, DirectML, or tensor package.
- All three payloads in the superseded AI-free bundle contain zero matching
  AI/ML files.
- That superseded upload is `47,935,430` bytes smaller than the preceding
  AI-bearing candidate.

### 4. Save/resume serialization and trimming

Severity: **Code/build resolved; packaged runtime check pending**

- Save/load now uses a source-generated `JsonSerializerContext` instead of the
  reflection-based overloads that emitted `IL2026`.
- Release trimming is disabled because the WinUI/Windows SDK dependency graph
  is not trim-clean. This resolves the `IL2104` risk instead of suppressing the
  warning and shipping potentially damaged dependency code.
- The unused legacy Community Toolkit Markdown package, disclosure link, and
  packaged license entry were removed.
- The superseded AI-free x86/x64/ARM64 Store bundle build completed with 0
  warnings and 0 errors. Neither `IL2026` nor `IL2104` was emitted. The current
  post-metadata Release x64 source build is also warning-free.

Remaining verification: user-test save, exit, resume, malformed-save handling,
and upgrade behavior using the final packaged Release build. Disabling trimming
increased the Store upload size; that size tradeoff is accepted here in favor
of release reliability.

### 5. Partner Center content is owner-managed

Severity: **Portal completion task**

`StoreSubmissionAssets` currently contains identity notes and four artwork
files. The owner will enter the privacy information and upload screenshots in
Partner Center during submission. These are not local source/package blockers.

Required portal checks:

- product reservation and identity;
- Games category and age-rating questionnaire;
- privacy policy and support URLs;
- data collection/declaration answers;
- screenshots and localized descriptions;
- pricing, markets, availability, and hardware requirements;
- restricted-capability declarations;
- EULA choice and third-party notices;
- certification notes and test credentials, if any.

No Partner Center upload or portal mutation was performed.

### 6. Store-facing claims

Severity: **Resolved in source**

- README and package release notes now advertise optional board coordinates only.
- Unused board-color resources were removed from all 18 languages, and the
  visible Board Settings description now describes coordinate labels only.
- The project is now licensed under the standard Apache License 2.0. The
  `open-source` and `apache-2.0` package tags are supported by the root
  `LICENSE`, package license expression, README, and developer guide.
- The runtime deployment description now states that .NET 10 and Windows App SDK
  runtime components are bundled with the self-contained Release packages.

### 7. Accessibility requires runtime validation

Severity: **Medium**

Board, selection, legal-move, capture, and check colors are hardcoded in
`Views\GamePage.xaml` and `Views\GamePage.xaml.cs` without High Contrast theme
overrides. Automation names are present, but visual accessibility has not been
validated.

User-operated checks required on the packaged Release build:

- keyboard-only navigation and game operation;
- Narrator announcements and focus order;
- High Contrast visibility and non-color cues;
- 100%, 150%, 200%, and 300% scale;
- minimum, restored, maximized, and multi-monitor windows;
- Arabic RTL and representative CJK/Latin layouts after localization is fixed.

### 8. Release provenance needs tightening

Severity: **Medium**

- The manifest now targets `Windows.Desktop` only, matching the supported device
  family for this packaged WinUI desktop application.
- Assembly and file versions now align with MSIX version `0.0.1.0`; the final
  informational version will incorporate the release-source commit identifier.
- The package was built from an extensively dirty worktree, so the embedded
  commit identifier does not identify the actual release source.

Required action: create a deliberate reproducible source snapshot before the
final rebuild.

## Positive findings

- Package and Partner Center identity notes agree.
- All required package artwork files exist with valid dimensions.
- No PFX, private key, debug PDB, obvious secret, or test artifact was found in
  the application payload.
- The superseded package is self-contained and has no external framework-package
  dependency.
- Source inspection found no telemetry, advertising, accounts, purchases,
  multiplayer, user-generated content, camera, microphone, or direct network
  implementation.
- Game state and preferences are stored locally.
- Noto artwork provenance and OFL inclusion are documented.
- WinUI/Awesome Copilot checklist review found no confirmed mandatory WACK
  failure beyond the issues recorded above.

## Definition of Store-ready

The next candidate is ready for upload only after all of the following are true:

1. Preserve the `systemAIModels` removal and AI/ML-payload exclusion while
   retaining the conventional offline computer opponent.
2. Preserve the verified 18-language parity and final bundle coverage.
3. Complete packaged Release save/resume tests.
4. Preserve the corrected product, licensing, and runtime-deployment claims.
5. Complete privacy information, listing, screenshots, age rating, and other
   portal fields in Partner Center.
6. Perform the user-operated accessibility/runtime checklist.
7. Build from an identified release source snapshot.
8. Run tests and a fresh WACK pass against that exact rebuilt version.

## Official references

- Microsoft Store policies:
  https://learn.microsoft.com/en-us/windows/apps/publish/store-policies
- App capability declarations and restricted capabilities:
  https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations
- Uploading MSIX packages:
  https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/upload-app-packages
- Store code signing:
  https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options
- WACK command-line workflow:
  https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/windows-app-certification-kit
- WACK tests:
  https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/windows-app-certification-kit-tests

## Changes and external actions

Changed locally:

- `AGENTS.md` now permits WACK-driven install/activation only during an
  explicitly authorized release-certification run.
- `TwelveRanks.App\TwelveRanks.App.csproj` keeps language resources inside each
  architecture package by limiting automatic resource-package splitting to
  `Scale|DXFeatureLevel`.
- All 17 non-English RESW files include the 31 newer strings and align with the
  English resource contract. The exact conventional-opponent values were
  restored from the retained 18-language PRI, producing 207 aligned keys per
  language.
- Save/load serialization uses generated JSON metadata; Release trimming and
  the unused legacy Markdown dependency are removed.
- Ten unused or redundant direct Community Toolkit references were removed.
  SettingsControls remains direct; its Common, Extensions, Helpers, and
  Triggers dependencies are retained transitively.
- The former AI project remains removed. The offline conventional computer
  opponent, its bounded varied-opening selection, UI/runtime paths, save mode,
  localized strings, and 13 test cases were restored under the separate
  `TwelveRanks.Computer` project.
- The restricted `systemAIModels` capability and all Windows AI/ML package
  payload were removed from source. The superseded AI-free bundle provides
  supporting package evidence; the final bundle still requires rebuilding.
- Board-color and external framework-dependency claims were corrected in README,
  package metadata, and all 18 localized resources. The open-source claim was
  withheld until a project-wide license existed.
- The standard Apache License 2.0 was added as the project-wide license and is
  included in Release output; third-party components retain their own licenses.
- The manifest now targets Windows Desktop only, and executable/MSIX versions
  align at `0.0.1.0`.
- This readiness report and generated build/test/WACK evidence were added.

Machine state:

- The pre-existing loose Debug package registration and its four data files
  remained unchanged.
- The temporary WACK certificate was removed from all certificate stores.

Not performed:

- no Partner Center upload or submission;
- no public publishing, deployment, commit, push, purchase, or account change;
- no manual or interactive app launch;
- no WACK run against a package rebuilt from the current corrected source;
- no runtime behavior claim beyond the documented automated evidence.
