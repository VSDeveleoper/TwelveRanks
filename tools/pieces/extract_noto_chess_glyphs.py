"""Extract the six standard chess silhouettes from Noto Sans Symbols 2.

The generated SVG files share one coordinate system and preserve the relative
size and baseline of the original glyphs. WinUI colors the six monochrome
masters in memory before passing them to its SVG image decoder.
"""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from fontTools.misc.transform import Transform
from fontTools.pens.boundsPen import BoundsPen
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont


CANVAS_SIZE = 1000
PADDING = 70
REPOSITORY_ROOT = Path(__file__).resolve().parents[2]
FONT_PATH = (
    REPOSITORY_ROOT
    / "third_party"
    / "NotoSansSymbols2"
    / "NotoSansSymbols2-Regular.ttf"
)
OUTPUT_DIRECTORY = (
    REPOSITORY_ROOT / "TwelveRanks.App" / "Assets" / "Pieces" / "Source"
)


@dataclass(frozen=True)
class PieceGlyph:
    name: str
    code_point: int


PIECES = (
    PieceGlyph("king", 0x265A),
    PieceGlyph("queen", 0x265B),
    PieceGlyph("rook", 0x265C),
    PieceGlyph("bishop", 0x265D),
    PieceGlyph("knight", 0x265E),
    PieceGlyph("pawn", 0x265F),
)


def format_number(value: float) -> str:
    return f"{value:.6f}".rstrip("0").rstrip(".")


def main() -> None:
    font = TTFont(FONT_PATH)
    try:
        glyph_set = font.getGlyphSet()
        character_map = font.getBestCmap()
        horizontal_metrics = font["hmtx"].metrics

        records: list[tuple[PieceGlyph, str, tuple[float, float, float, float], int]] = []
        for piece in PIECES:
            glyph_name = character_map[piece.code_point]
            glyph = glyph_set[glyph_name]

            bounds_pen = BoundsPen(glyph_set)
            glyph.draw(bounds_pen)
            if bounds_pen.bounds is None:
                raise ValueError(f"U+{piece.code_point:04X} has no drawable outline")

            advance_width, _ = horizontal_metrics[glyph_name]
            records.append((piece, glyph_name, bounds_pen.bounds, advance_width))

        maximum_advance = max(record[3] for record in records)
        minimum_y = min(record[2][1] for record in records)
        maximum_y = max(record[2][3] for record in records)
        usable_size = CANVAS_SIZE - (2 * PADDING)
        scale = min(
            usable_size / maximum_advance,
            usable_size / (maximum_y - minimum_y),
        )

        OUTPUT_DIRECTORY.mkdir(parents=True, exist_ok=True)
        for piece, glyph_name, _, advance_width in records:
            path_pen = SVGPathPen(glyph_set)
            translate_x = (CANVAS_SIZE - (advance_width * scale)) / 2
            translate_y = PADDING + (maximum_y * scale)
            transform = Transform(
                scale,
                0,
                0,
                -scale,
                translate_x,
                translate_y,
            )
            transformed_pen = TransformPen(path_pen, transform)
            glyph_set[glyph_name].draw(transformed_pen)
            path_data = path_pen.getCommands()

            svg = f"""<?xml version="1.0" encoding="utf-8"?>
<!--
  Derived from Noto Sans Symbols 2, U+{piece.code_point:04X}.
  Copyright 2022 The Noto Project Authors.
  Licensed under the SIL Open Font License, Version 1.1.
-->
<svg xmlns="http://www.w3.org/2000/svg"
     viewBox="0 0 {CANVAS_SIZE} {CANVAS_SIZE}"
     role="img"
     aria-labelledby="title">
  <title id="title">Chess {piece.name}</title>
  <path d="{path_data}" fill="currentColor" />
</svg>
"""
            (OUTPUT_DIRECTORY / f"{piece.name}.svg").write_text(
                svg,
                encoding="utf-8",
                newline="\n",
            )

    finally:
        font.close()


if __name__ == "__main__":
    main()
