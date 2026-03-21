using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using nodiApp.Models;
using nodiApp.Services;

namespace nodiApp.ViewModels;

[QueryProperty(nameof(NoteLocalId), "NoteLocalId")]
[QueryProperty(nameof(NoteTypeParam), "NoteType")]
public partial class NoteDetailViewModel(LocalDbService db) : ObservableObject
{
    [ObservableProperty] private int noteLocalId;
    [ObservableProperty] private string noteTypeParam = "Text";
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private string selectedColor = "Default";
    [ObservableProperty] private string noteType = "Text";
    [ObservableProperty] private ObservableCollection<ChecklistItemViewModel> checklistItems = [];
    [ObservableProperty] private bool isChecklist;

    private Note? _note;

    public async Task InitializeAsync()
    {
        if (NoteLocalId > 0)
        {
            _note = await db.GetNoteAsync(NoteLocalId);
            if (_note is not null)
            {
                Title = _note.Title;
                Content = _note.Content ?? string.Empty;
                SelectedColor = _note.Color.ToString();
                NoteType = _note.Type.ToString();
                IsChecklist = _note.Type == NoteType.Checklist;

                if (IsChecklist)
                {
                    var items = await db.GetChecklistItemsAsync(_note.LocalId);
                    ChecklistItems = new ObservableCollection<ChecklistItemViewModel>(
                        items.Select(i => new ChecklistItemViewModel { Text = i.Text, IsChecked = i.IsChecked, Order = i.Order }));
                }
            }
        }
        else
        {
            NoteType = NoteTypeParam;
            IsChecklist = NoteTypeParam == "Checklist";
        }
    }

    [RelayCommand]
    private void AddChecklistItem()
    {
        ChecklistItems.Add(new ChecklistItemViewModel { Order = ChecklistItems.Count });
    }

    [RelayCommand]
    private void RemoveChecklistItem(ChecklistItemViewModel item)
    {
        ChecklistItems.Remove(item);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_note is null)
        {
            _note = new Note
            {
                Title = Title,
                Content = IsChecklist ? null : Content,
                Color = Enum.TryParse<NoteColor>(SelectedColor, out var c) ? c : NoteColor.Default,
                Type = IsChecklist ? Models.NoteType.Checklist : Models.NoteType.Text,
                SyncStatus = SyncStatus.PendingCreate
            };
            await db.SaveNoteAsync(_note);
        }
        else
        {
            _note.Title = Title;
            _note.Content = IsChecklist ? null : Content;
            _note.Color = Enum.TryParse<NoteColor>(SelectedColor, out var c) ? c : NoteColor.Default;
            _note.UpdatedAt = DateTime.UtcNow;
            _note.SyncStatus = _note.SyncStatus == SyncStatus.PendingCreate ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;
            await db.SaveNoteAsync(_note);
        }

        if (IsChecklist)
        {
            var items = ChecklistItems.Select((item, idx) => new ChecklistItem
            {
                Text = item.Text,
                IsChecked = item.IsChecked,
                Order = idx
            }).ToList();
            await db.ReplaceChecklistItemsAsync(_note.LocalId, items);
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public partial class ChecklistItemViewModel : ObservableObject
{
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private bool isChecked;
    public int Order { get; set; }
}
