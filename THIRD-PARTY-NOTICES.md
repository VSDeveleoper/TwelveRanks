# Third-Party Notices

Reviewed against the current project files and restored NuGet package graphs on
2026-08-01.

This file is included in the Twelve Ranks application payload. Runtime entries
follow the resolved application graph in
`TwelveRanks.App\obj\project.assets.json`; development and test-only dependencies
are identified separately.

The package includes readable license and attribution files under `Licenses\`:

- `MIT.txt` for dependencies distributed under the MIT License.
- `NotoSansSymbols2-OFL.txt` for the Noto-derived chess-piece artwork.
- `MicrosoftDotNetLibraryLicense.txt` and
  `MicrosoftDotNetLibraryThirdPartyNotices.txt` as supplemental Microsoft .NET
  library license records.
- `MicrosoftDotNetRuntimeLicense.txt` and
  `MicrosoftDotNetRuntimeThirdPartyNotices.txt` for the bundled .NET 10
  runtime.
- `Microsoft-WindowsAppSDK-Runtime-license.txt`,
  `Microsoft-WindowsAppSDK-Runtime-NOTICE.txt`,
  `Microsoft-WindowsAppSDK-WinUI-license.txt`, and
  `Microsoft-WindowsAppSDK-WinUI-NOTICE.txt`, copied from the resolved Windows
  App SDK packages during the build.
- `Microsoft-WebView2-LICENSE.txt` and `Microsoft-WebView2-NOTICE.txt`, copied
  from the resolved WebView2 SDK package.

## Bundled Microsoft Runtimes

Twelve Ranks targets `net10.0-windows10.0.26100.0`. Its Release publish profiles
are self-contained for x86, x64, and ARM64, and the current restored runtime
pack is Microsoft.NETCore.App 10.0.10.

The project also sets `WindowsAppSDKSelfContained` to `true`, so the Windows App
SDK runtime components are carried with the application rather than requiring a
separately installed Windows App Runtime framework package.

## Windows App SDK, WinUI, and WebView2 Packages

The following packages support the WinUI 3 application and its packaged Windows
runtime:

- `Microsoft.WindowsAppSDK` 2.3.1 (aggregate dependency coordinator; direct
  package assets are excluded and unused feature branches are pruned)
- `Microsoft.WindowsAppSDK.WinUI` 2.3.2
- `Microsoft.WindowsAppSDK.Base` 2.0.4
- `Microsoft.WindowsAppSDK.Foundation` 2.3.5
- `Microsoft.WindowsAppSDK.InteractiveExperiences` 2.1.3
- `Microsoft.WindowsAppSDK.Runtime` 2.3.1
- `Microsoft.Web.WebView2` 1.0.3719.77
- `Microsoft.Windows.SDK.BuildTools.MSIX` 1.7.251221100

The WebView2 SDK package is a transitive dependency of
`Microsoft.WindowsAppSDK.WinUI`; Twelve Ranks does not instantiate or display a
WebView2 control. The restored application graph contains no Windows App SDK
AI, Windows machine-learning, ONNX, DirectML, or tensor package.

These Microsoft packages use the license and notice files supplied with their
respective packages. The principal Windows App SDK, WinUI, and WebView2 terms
are included in `Licenses\`.

## Windows Community Toolkit

Twelve Ranks uses the Windows Community Toolkit Settings controls for the
application settings interface. The current restored Toolkit graph is:

- `CommunityToolkit.Common` 8.2.1
- `CommunityToolkit.WinUI.Controls.SettingsControls` 8.2.251219
- `CommunityToolkit.WinUI.Extensions` 8.2.251219
- `CommunityToolkit.WinUI.Helpers` 8.2.251219
- `CommunityToolkit.WinUI.Triggers` 8.2.251219

The installed package metadata identifies these Community Toolkit packages as
MIT licensed. Copyright belongs to the .NET Foundation and contributors.

## Noto Sans Symbols 2 Artwork

The standard chess-piece SVG masters, and the Minister and Chancellor
composites derived from them, use glyph outlines from Noto Sans Symbols 2.

- Source: Noto Sans Symbols 2
- Copyright: 2022 The Noto Project Authors
- License: SIL Open Font License, Version 1.1

The original font, source notes, checksum, and license are retained under
`third_party\NotoSansSymbols2\`. A verbatim license copy is included in the
application's `Licenses\` folder.

## Other Microsoft and .NET Runtime Dependencies

The self-contained application graph also contains Microsoft .NET runtime
packages. Their applicable Microsoft license and third-party notice texts are
included under `Licenses\`.

## Development and Test Tooling

The following dependencies are used for restoring, building, analyzing, or
testing the source tree and are not Twelve Ranks gameplay features:

- `Microsoft.Windows.SDK.BuildTools` 10.0.28000.2526
- `MSTest`, `MSTest.TestFramework`, `MSTest.TestAdapter`, and
  `MSTest.Analyzers` 4.3.3
- `Microsoft.NET.Test.Sdk`, `Microsoft.TestPlatform.*`, and
  `Microsoft.CodeCoverage` 18.4.0
- `Microsoft.Testing.Platform*` 2.3.3
- `Microsoft.Testing.Extensions.CodeCoverage` 18.9.0
- `Microsoft.ApplicationInsights` 2.23.0
- `Microsoft.DiaSymReader` 2.2.9
- `Microsoft.Extensions.DependencyModel` 10.0.8
- `Newtonsoft.Json` 13.0.3
- Microsoft `System.*` support packages resolved by the test host

The local NuGet metadata identifies these packages as MIT licensed except where
a package supplies its own Microsoft license file or SDK license URL.

## Twelve Ranks Source License

These notices cover third-party materials and do not relicense them. Except
where a file or notice states otherwise, the original Twelve Ranks source code,
documentation, and project-owned assets are licensed under the Apache License
2.0 in the repository-root `LICENSE` file.
