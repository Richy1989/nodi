using SQLite;
using nodiApp.Models;

namespace nodiApp.Services;

public class LocalDbService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDb()
    {
        if (_db is not null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "nodi.db3");
        _db = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _db.CreateTableAsync<Note>();
        await _db.CreateTableAsync<ChecklistItem>();
        await _db.CreateTableAsync<Tag>();
        await _db.CreateTableAsync<NoteTag>();
        return _db;
    }

    // Notes
    public async Task<List<Note>> GetNotesAsync(bool archived = false, bool deleted = false)
    {
        var db = await GetDb();
        return await db.Table<Note>()
            .Where(n => n.IsArchived == archived && n.IsDeleted == deleted && n.SyncStatus != SyncStatus.PendingDelete)
            .OrderByDescending(n => n.IsPinned)
            .ToListAsync();
    }

    public async Task<Note?> GetNoteAsync(int localId)
    {
        var db = await GetDb();
        return await db.Table<Note>().FirstOrDefaultAsync(n => n.LocalId == localId);
    }

    public async Task<int> SaveNoteAsync(Note note)
    {
        var db = await GetDb();
        if (note.LocalId == 0)
            return await db.InsertAsync(note);
        return await db.UpdateAsync(note);
    }

    public async Task DeleteNoteAsync(Note note, bool permanent = false)
    {
        var db = await GetDb();
        if (permanent || note.ServerId is null)
        {
            await db.DeleteAsync(note);
            await db.Table<ChecklistItem>().DeleteAsync(i => i.NoteLocalId == note.LocalId);
        }
        else
        {
            note.IsDeleted = true;
            note.SyncStatus = SyncStatus.PendingDelete;
            await db.UpdateAsync(note);
        }
    }

    // Checklist items
    public async Task<List<ChecklistItem>> GetChecklistItemsAsync(int noteLocalId)
    {
        var db = await GetDb();
        return await db.Table<ChecklistItem>()
            .Where(i => i.NoteLocalId == noteLocalId)
            .OrderBy(i => i.Order)
            .ToListAsync();
    }

    public async Task ReplaceChecklistItemsAsync(int noteLocalId, List<ChecklistItem> items)
    {
        var db = await GetDb();
        await db.Table<ChecklistItem>().DeleteAsync(i => i.NoteLocalId == noteLocalId);
        foreach (var item in items)
        {
            item.LocalId = 0;
            item.NoteLocalId = noteLocalId;
        }
        if (items.Count > 0)
            await db.InsertAllAsync(items);
    }

    // Tags
    public async Task<List<Tag>> GetTagsAsync()
    {
        var db = await GetDb();
        return await db.Table<Tag>().ToListAsync();
    }

    public async Task<int> SaveTagAsync(Tag tag)
    {
        var db = await GetDb();
        if (tag.LocalId == 0) return await db.InsertAsync(tag);
        return await db.UpdateAsync(tag);
    }

    public async Task<List<Tag>> GetNoteTagsAsync(int noteLocalId)
    {
        var db = await GetDb();
        var noteTags = await db.Table<NoteTag>().Where(nt => nt.NoteLocalId == noteLocalId).ToListAsync();
        var tagIds = noteTags.Select(nt => nt.TagLocalId).ToList();
        var allTags = await db.Table<Tag>().ToListAsync();
        return allTags.Where(t => tagIds.Contains(t.LocalId)).ToList();
    }

    // Sync helpers
    public async Task<List<Note>> GetPendingSyncNotesAsync()
    {
        var db = await GetDb();
        return await db.Table<Note>().Where(n => n.SyncStatus != SyncStatus.Synced).ToListAsync();
    }

    public async Task MarkSyncedAsync(int noteLocalId, int serverId)
    {
        var db = await GetDb();
        var note = await db.Table<Note>().FirstOrDefaultAsync(n => n.LocalId == noteLocalId);
        if (note is not null)
        {
            note.ServerId = serverId;
            note.SyncStatus = SyncStatus.Synced;
            await db.UpdateAsync(note);
        }
    }

    public async Task ClearAndReplaceAllAsync(List<Note> serverNotes, Dictionary<int, List<ChecklistItem>> serverItems)
    {
        var db = await GetDb();
        await db.DeleteAllAsync<Note>();
        await db.DeleteAllAsync<ChecklistItem>();
        if (serverNotes.Count > 0)
            await db.InsertAllAsync(serverNotes);
        foreach (var kvp in serverItems)
        {
            if (kvp.Value.Count > 0)
                await db.InsertAllAsync(kvp.Value);
        }
    }
}
