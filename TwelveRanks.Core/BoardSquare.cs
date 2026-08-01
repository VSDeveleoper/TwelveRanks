using System.Globalization;

namespace TwelveRanks.Core;

/// <summary>
/// Identifies one square on the 12-by-12 board.
/// </summary>
public readonly record struct BoardSquare
{
    /// <summary>
    /// Initializes a board square from zero-based coordinates.
    /// </summary>
    /// <param name="rank">The zero-based rank, measured from Black's back rank.</param>
    /// <param name="file">The zero-based file, measured from the left edge.</param>
    public BoardSquare(int rank, int file)
    {
        if (rank < 0 || rank >= BoardSpecification.Ranks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rank),
                rank,
                "The rank must be on the Twelve Ranks board.");
        }

        if (file < 0 || file >= BoardSpecification.Files)
        {
            throw new ArgumentOutOfRangeException(
                nameof(file),
                file,
                "The file must be on the Twelve Ranks board.");
        }

        Rank = rank;
        File = file;
    }

    /// <summary>
    /// Gets the zero-based rank measured from Black's back rank.
    /// </summary>
    public int Rank { get; }

    /// <summary>
    /// Gets the zero-based file measured from the left edge.
    /// </summary>
    public int File { get; }

    /// <summary>
    /// Gets the square's zero-based index in the engine board.
    /// </summary>
    public int Index => (Rank * BoardSpecification.Files) + File;

    /// <summary>
    /// Gets algebraic-style notation such as <c>a1</c> or <c>l12</c>.
    /// </summary>
    public string Notation
    {
        get
        {
            char fileName = (char)('a' + File);
            int displayedRank = BoardSpecification.Ranks - Rank;

            return string.Concat(
                fileName,
                displayedRank.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Creates a square from its zero-based engine index.
    /// </summary>
    /// <param name="index">An index from zero through 143.</param>
    /// <returns>The corresponding board square.</returns>
    public static BoardSquare FromIndex(int index)
    {
        if (index < 0 || index >= BoardSpecification.SquareCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                "The index must identify a Twelve Ranks square.");
        }

        return new BoardSquare(
            index / BoardSpecification.Files,
            index % BoardSpecification.Files);
    }

    /// <summary>
    /// Attempts to offset this square without leaving the board.
    /// </summary>
    /// <param name="rankDelta">The signed change in rank.</param>
    /// <param name="fileDelta">The signed change in file.</param>
    /// <param name="destination">The destination when the offset remains on board.</param>
    /// <returns><see langword="true"/> when the destination is on board.</returns>
    public bool TryOffset(
        int rankDelta,
        int fileDelta,
        out BoardSquare destination)
    {
        int destinationRank = Rank + rankDelta;
        int destinationFile = File + fileDelta;

        if (destinationRank < 0 ||
            destinationRank >= BoardSpecification.Ranks ||
            destinationFile < 0 ||
            destinationFile >= BoardSpecification.Files)
        {
            destination = default;
            return false;
        }

        destination = new BoardSquare(destinationRank, destinationFile);
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Notation;
    }
}
