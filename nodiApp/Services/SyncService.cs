using nodiApp.Models;

namespace nodiApp.Services;

public class SyncService(LocalDbService localDb, ApiService api)
{
    public async Task SyncAsync()
    {
        if (!api.IsAuthenticated) return;

        try
        {
            // Push pending changes
            var pending = await localDb.GetPendingSyncNotesAsync();
            foreach (var note in pending)
            {
                var items = await localDb.GetChecklistItemsAsync(note.LocalId);
                var payload = new
                {
                    title = note.Title,
                    content = note.Content,
                    color = note.Color.ToString(),
                    type = note.Type.ToString(),
                    isPinned = note.IsPinned,
                    isArchived = note.IsArchived,
                    checklistItems = items.Select(i => new { i.Text, i.IsChecked, i.Order }).ToList()
                };

                if (note.SyncStatus == SyncStatus.PendingCreate)
                {
                    var created = await api.CreateNoteAsync(payload);
                    if (created is not null)
                        await localDb.MarkSyncedAsync(note.LocalId, created.Id);
                }
                else if (note.SyncStatus == SyncStatus.PendingUpdate && note.ServerId.HasValue)
                {
                    await api.UpdateNoteAsync(note.ServerId.Value, payload);
                    await localDb.MarkSyncedAsync(note.LocalId, note.ServerId.Value);
                }
                else if (note.SyncStatus == SyncStatus.PendingDelete && note.ServerId.HasValue)
                {
                    await api.DeleteNoteAsync(note.ServerId.Value);
                    await localDb.DeleteNoteAsync(note, permanent: true);
                }
            }

            // Pull latest from server
            var serverNotes = await api.GetNotesAsync();
            var localNotes = serverNotes.Select(n => new Note
            {
                ServerId = n.Id,
                Title = n.Title,
                Content = n.Content,
                Color = Enum.TryParse<NoteColor>(n.Color, out var c) ? c : NoteColor.Default,
                Type = Enum.TryParse<NoteType>(n.Type, out var t) ? t : NoteType.Text,
                IsPinned = n.IsPinned,
                IsArchived = n.IsArchived,
                IsDeleted = n.IsDeleted,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                SyncStatus = SyncStatus.Synced
            }).ToList();

            var itemsMap = new Dictionary<int, List<ChecklistItem>>();
            foreach (var serverNote in serverNotes)
            {
                var noteLocalIdx = localNotes.FindIndex(n => n.ServerId == serverNote.Id);
                if (noteLocalIdx >= 0 && serverNote.ChecklistItems.Count > 0)
                {
                    itemsMap[noteLocalIdx] = serverNote.ChecklistItems.Select((i, idx) => new ChecklistItem
                    {
                        ServerId = i.Id,
                        Text = i.Text,
                        IsChecked = i.IsChecked,
                        Order = i.Order
                    }).ToList();
                }
            }

            await localDb.ClearAndReplaceAllAsync(localNotes, itemsMap);
        }
        catch
        {
            // Offline — continue with local data
        }
    }
}
