using nodeCommon;

namespace nodiWeb.Services;

public record NoteItemArg(string Text, bool IsChecked, int Order);
public record SaveNoteArgs(string Title, string? Content, string Color, string Type, List<NoteItemArg> Items);
