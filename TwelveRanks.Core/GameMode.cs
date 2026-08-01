namespace TwelveRanks.Core;

/// <summary>
/// Identifies the opponent arrangement for a game.
/// </summary>
public enum GameMode
{
    /// <summary>
    /// Two players share the local device.
    /// </summary>
    LocalTwoPlayer,

    /// <summary>
    /// A local player competes against the built-in computer opponent.
    /// </summary>
    Computer
}
