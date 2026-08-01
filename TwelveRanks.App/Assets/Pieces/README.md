# Chess piece artwork

`Source` contains the editable monochrome SVG masters for the six standard
piece types and the Twelve Ranks Minister and Chancellor. They are derived from
Noto Sans Symbols 2 and are covered by the SIL Open Font License in
`third_party/NotoSansSymbols2/OFL.txt`. A verbatim copy is included in the
packaged application at `Licenses/NotoSansSymbols2-OFL.txt`.

`Source/minister.svg` is a self-contained, editable composite made solely
from the local Noto bishop (U+265D) and knight (U+265E) masters. It combines
the bishop base with a scaled knight crest; it does not load or depend on an
external image.

`Source/chancellor.svg` follows the same self-contained design: it combines
the local Noto rook (U+265C) base with the same scaled Noto knight crest. Both
compound-piece masters remain editable and require no external image.

The application should apply side colors, outlines, shadows, and state
indicators at render time. In particular, both Royal Guard pawns reuse
`Source/pawn.svg`; their red indicator is a separate overlay and their
different behavior belongs in the game model.

The WinUI piece control loads the eight masters, applies each side's fill and
outline in memory, and passes the colored SVG directly to `SvgImageSource`.
This keeps one editable asset per piece type.

Regenerate the standard SVGs from the repository root with:

```powershell
python -m pip install -r .\tools\pieces\requirements.txt
python .\tools\pieces\extract_noto_chess_glyphs.py
```

That script regenerates only the six standard masters. The Minister and
Chancellor are intentionally maintained as documented composites of the
generated standard-piece source paths.
