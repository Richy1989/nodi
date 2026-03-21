namespace nodeCommon;

/// <summary>
/// Background colour applied to a note. Serialised as a string in the API and database.
/// </summary>
public enum NoteColor
{
    Default, Salmon, Peach, LightYellow, Mint, Cyan, SkyBlue, CornflowerBlue, Lavender, HotPink, Tan, Silver
}

/// <summary>
/// Determines how a note's body is structured.
/// <list type="bullet">
///   <item><term>Text</term><description>Free-form text content.</description></item>
///   <item><term>Checklist</term><description>A list of checkable items.</description></item>
/// </list>
/// </summary>
public enum NoteType { Text, Checklist }
