# Twelve Ranks

Twelve Ranks is a modern 12 × 12 chess variant for Windows. It keeps the
objective and core rules of ordinary chess while expanding each army with the
Minister, the Chancellor, and two Royal Guard pawns.

The project is currently an initial development preview. Local two-player games
and a built-in conventional computer opponent are available; both work entirely
offline.

Project home: [github.com/VSDeveleoper/TwelveRanks](https://github.com/VSDeveleoper/TwelveRanks)

## What changes from ordinary chess

- The board has 12 files and 12 ranks.
- Each side begins with 12 back-rank pieces and 12 pawns.
- The **Minister** moves and captures like a bishop or knight. It may exchange
  places with a friendly king or queen it can reach when the resulting position
  leaves the king safe.
- The **Chancellor** moves and captures like a rook or knight.
- The pawns in front of the king and queen are **Royal Guards**. They may move
  one square forward, backward, or diagonally and may advance two clear squares
  forward from any rank. Royal Guards do not promote.
- A regular pawn may promote to a queen, rook, bishop, knight, Minister, or
  Chancellor.

All other normal chess rules apply, including check, checkmate, castling,
en passant, stalemate, threefold repetition, the fifty-move rule, and draws by
insufficient material.

## Features

- Local two-player play on one Windows device.
- Built-in conventional computer opponent with varied opening play, entirely
  offline.
- Complete legal-move and game-state validation.
- Undo and redo support.
- Scalable chess-piece artwork and captured-piece panels.
- Optional board coordinates.
- Distinct sounds for moves, captures, castling, promotion, check, and game
  over.
- Localized interface for 18 languages, including right-to-left Arabic.
- Single-instance application behavior and confirmation before exiting.
- Self-contained .NET 10 Release packages for x86, x64, and ARM64.

## Requirements

- Windows 10 version 1809 (build 17763) or later.
- .NET 10 SDK.
- Visual Studio with the Windows App SDK and C# desktop development components
  for packaged debugging and deployment.

NuGet restore supplies the Windows App SDK and Windows Community Toolkit
dependencies. Release packages bundle both the .NET 10 runtime and the Windows
App SDK runtime components.

## Build and test

From the repository root:

```powershell
dotnet restore .\TwelveRanks.slnx
dotnet build .\TwelveRanks.slnx --configuration Debug -p:Platform=x64
dotnet test .\TwelveRanks.Tests\TwelveRanks.Tests.csproj --configuration Debug
```

For the standard packaged development workflow, open `TwelveRanks.slnx` in
Visual Studio, select `TwelveRanks.App` as the startup project, choose an
explicit architecture such as `x64`, and deploy/run the app.

If the WinApp CLI is installed, the built packaged layout can also be launched
with:

```powershell
winapp run .\TwelveRanks.App\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64 --detach
```

## Repository layout

- `TwelveRanks.Core` — board model, rules, move generation, and game state.
- `TwelveRanks.Computer` — bounded conventional search with varied opening
  selection.
- `TwelveRanks.App` — packaged WinUI 3 application, localization, and assets.
- `TwelveRanks.Tests` — MSTest coverage for rules, pieces, game state, and the
  conventional opponent.
- `StoreSubmissionAssets` — prepared Microsoft Store artwork.

## Contributing

Contributions are welcome through the
[GitHub repository](https://github.com/VSDeveleoper/TwelveRanks). Read the
[contribution guide](https://github.com/VSDeveleoper/TwelveRanks/blob/main/CONTRIBUTING.md)
before opening an issue or pull request. Designers, developers, and prospective
AI or multiplayer sponsors should also read
[DEVELOPERS_README.md](DEVELOPERS_README.md) for the project priorities and
partnership boundaries.

Changes should remain deterministic and offline-first, include appropriate
tests, preserve accessibility and localization behavior, and avoid exposing
engine-only analysis through the player interface.

Use the repository's issue forms for bugs, features, artwork proposals, and
public partnership introductions. Report suspected vulnerabilities privately
as described in
[SECURITY.md](https://github.com/VSDeveleoper/TwelveRanks/blob/main/SECURITY.md).

## License

Except where a file or third-party notice states otherwise, the original Twelve
Ranks source code, documentation, and project-owned assets are licensed under
the [Apache License 2.0](LICENSE). The license permits use, modification, and
redistribution, including in commercial and proprietary works, subject to its
notice and attribution requirements.

Third-party assets retain their respective licenses. Noto Sans Symbols 2
artwork is used under the SIL Open Font License 1.1; the corresponding license
and attribution files are included in the repository and installed
application. See [ASSET_LICENSES.md](ASSET_LICENSES.md) for the asset inventory,
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the complete dependency
inventory, and
[TwelveRanks.App/Licenses](TwelveRanks.App/Licenses) for the distributable license
texts.

## Copyright

Copyright © 2026 A. Sharaf.
