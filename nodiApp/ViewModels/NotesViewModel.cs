using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nodiApp.Models;
using nodiApp.Services;

namespace nodiApp.ViewModels;

public partial class NotesViewModel(LocalDbService db, SyncService sync) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<NoteWithItems> notes = [];
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string searchText = string.Empty;

    private List<NoteWithItems> _allNotes = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsRefreshing = true;
        await sync.SyncAsync();
        await RefreshLocalAsync();
        IsRefreshing = false;
    }

    public async Task RefreshLocalAsync()
    {
        var rawNotes = await db.GetNotesAsync();
        _allNotes = [];
        foreach (var note in rawNotes)
        {
            var items = note.Type == NoteType.Checklist ? await db.GetChecklistItemsAsync(note.LocalId) : [];
            _allNotes.Add(new NoteWithItems(note, items));
        }
        ApplySearch();
    }

    partial void OnSearchTextChanged(string value) => ApplySearch();

    private void ApplySearch()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allNotes
            : _allNotes.Where(n =>
                n.Note.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (n.Note.Content?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

        Notes = new ObservableCollection<NoteWithItems>(filtered);
    }

    [RelayCommand]
    private async Task TogglePinAsync(NoteWithItems noteWithItems)
    {
        var note = noteWithItems.Note;
        note.IsPinned = !note.IsPinned;
        note.SyncStatus = SyncStatus.PendingUpdate;
        await db.SaveNoteAsync(note);
        await RefreshLocalAsync();
    }

    [RelayCommand]
    private async Task ArchiveAsync(NoteWithItems noteWithItems)
    {
        var note = noteWithItems.Note;
        note.IsArchived = true;
        note.SyncStatus = SyncStatus.PendingUpdate;
        await db.SaveNoteAsync(note);
        await RefreshLocalAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(NoteWithItems noteWithItems)
    {
        await db.DeleteNoteAsync(noteWithItems.Note);
        await RefreshLocalAsync();
    }

    [RelayCommand]
    private async Task OpenNoteAsync(NoteWithItems noteWithItems)
    {
        await Shell.Current.GoToAsync("notedetail", new Dictionary<string, object>
        {
            { "NoteLocalId", noteWithItems.Note.LocalId }
        });
    }

    [RelayCommand]
    private async Task CreateNoteAsync(string type)
    {
        await Shell.Current.GoToAsync("notedetail", new Dictionary<string, object>
        {
            { "NoteLocalId", 0 },
            { "NoteType", type }
        });
    }
}

public record NoteWithItems(Note Note, List<ChecklistItem> Items);
