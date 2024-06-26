using System.Collections.ObjectModel;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.UI.Command;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.Tags;

public record TagItem(string Name, bool IsSelected = false);

public class TagsViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    
    private Folder? _associatedFolder;
    private ObservableCollection<TagItem>? _tagItems;
    
    public TagsViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        
        eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);
        eventAggregator.Subscribe<TagAddedEvent>(this, OnTagAdded);
        eventAggregator.Subscribe<TagRemovedEvent>(this, OnTagRemoved);

        ToggleTagSelectionCommand = new DelegateCommand(param =>
        {
            if (param is not TagItem tagItem) return;
            ToggleTagItemSelection(tagItem);
        });
    }

    public ObservableCollection<TagItem>? TagItems
    {
        get => _tagItems;
        private set
        {
            _tagItems = value;
            RaisePropertyChanged();
        }
    }

    public ICommand ToggleTagSelectionCommand { get; }
    
    private void ToggleTagItemSelection(TagItem tagItem)
    {
        if (_tagItems == null) return;

        var itemIndex = _tagItems.IndexOf(tagItem);
        var newTagItem = tagItem with { IsSelected = !tagItem.IsSelected };
        _tagItems[itemIndex] = newTagItem;
        
        _eventAggregator.Publish(new TagSelectionChangedEvent(newTagItem.Name, newTagItem.IsSelected));
    }
    
    private void OnFolderSelected(FolderSelectedEvent folderEvent)
    {
        _associatedFolder = folderEvent.Folder;
        
        TagItems = _associatedFolder == null ? 
            null : new ObservableCollection<TagItem>(_associatedFolder.Tags.Select(tag => new TagItem(tag.Name)).ToList());
    }
    
    private void OnTagAdded(TagAddedEvent tagEvent)
    {
        var createdTag = tagEvent.CreatedTag;
        
        if (_tagItems == null || createdTag.FolderId != _associatedFolder?.Id) return;

        if (_tagItems.All(tagItem => tagItem.Name != createdTag.Name))
        {
            _tagItems.Add(new TagItem(createdTag.Name));
        }
    }
    
    private void OnTagRemoved(TagRemovedEvent tagEvent)
    {
        var removedTag = tagEvent.RemovedTag;
        
        if (_tagItems == null || removedTag.FolderId != _associatedFolder?.Id) return;
        
        if (_associatedFolder.Tags.All(tag => tag.Name != removedTag.Name))
        {
            var associatedTagItem = _tagItems.FirstOrDefault(tagItem => tagItem.Name == removedTag.Name);
            
            if (associatedTagItem?.IsSelected == true)
            {
                _eventAggregator.Publish(new TagSelectionChangedEvent(associatedTagItem.Name, false));
            }
            _tagItems.Remove(associatedTagItem);
        }
    }
}