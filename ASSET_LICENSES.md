# Asset Licensing and Provenance

This inventory separates Twelve Ranks project-owned artwork from third-party
materials. It must be updated whenever an asset is added, replaced, or
relicensed.

## Project-Owned Assets

The following assets are declared project-owned and, except where a file states
otherwise, are licensed under the repository's
[Apache License 2.0](LICENSE):

- `TwelveRanks.App/Assets/Branding/` — master emblem and logo artwork.
- `TwelveRanks.App/Assets/PackageIcon.png` and
  `TwelveRanks.App/Assets/TwelveRanks.ico` — application icons.
- `TwelveRanks.App/Assets/LockScreenLogo.scale-200.png`,
  `SplashScreen.scale-200.png`, `Square150x150Logo.scale-200.png`,
  `Square44x44Logo.scale-200.png`,
  `Square44x44Logo.targetsize-24_altform-unplated.png`, `StoreLogo.png`, and
  `Wide310x150Logo.scale-200.png` — generated package artwork.
- `StoreSubmissionAssets/StoreLogos/` — Microsoft Store listing artwork.

Copyright © 2026 A. Sharaf.

## Noto-Derived Chess-Piece Artwork

The chess-piece SVG masters under
`TwelveRanks.App/Assets/Pieces/Source/`, including the Minister and Chancellor
composites, are derived from Noto Sans Symbols 2 and retain the SIL Open Font
License 1.1 terms.

- Upstream project: Noto Sans Symbols 2.
- Upstream copyright: 2022 The Noto Project Authors.
- License: SIL Open Font License, Version 1.1.
- Retained source font and license: `third_party/NotoSansSymbols2/`.
- Packaged license copy:
  `TwelveRanks.App/Licenses/NotoSansSymbols2-OFL.txt`.
- Detailed derivation notes:
  `TwelveRanks.App/Assets/Pieces/README.md`.

The Apache License 2.0 does not replace or override the SIL Open Font License
for these materials.

## Third-Party License Texts

Files under `TwelveRanks.App/Licenses/` are retained license and notice texts
for dependencies and redistributed runtime components. They remain governed by
their respective terms and are not relicensed under Apache-2.0. See
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the dependency inventory.

## Requirements for New Artwork

Every artwork contribution must identify:

- the contributor and copyright holder;
- whether the work is original or derived;
- all source material, fonts, references, and generation tools;
- the exact license and any attribution or notice requirements;
- editable source files and the procedure used to produce distributable
  outputs; and
- whether any trademark, likeness, dataset, or third-party platform terms
  apply.

Do not submit copied artwork or material whose ownership or redistribution
rights cannot be demonstrated. A pull request must update this inventory and
all required license or notice files when it changes the application assets.

Unless a separate written agreement is accepted and recorded, intentionally
submitted project-owned artwork is contributed under the Apache License 2.0
terms described in [CONTRIBUTING.md](CONTRIBUTING.md).
