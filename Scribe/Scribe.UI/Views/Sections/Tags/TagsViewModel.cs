using System.Collections.ObjectModel;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.UI.Commands;
using Scribe.UI.Events;
using Scribe.UI.Views.Sections.Tags.State;

namespace Scribe.UI.Views.Sections.Tags;

public class TagsViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    
    private Folder? _associatedFolder;
    private ObservableCollection<TagViewState>? _tags;
    
    public TagsViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        
        eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);
        eventAggregator.Subscribe<TagAddedEvent>(this, OnTagAdded);
        eventAggregator.Subscribe<TagRemovedEvent>(this, OnTagRemoved);
        eventAggregator.Subscribe<TagSelectionChangedEvent>(this, OnTagSelectionChanged);

        ToggleTagSelectionCommand = new DelegateCommand(param =>
        {
            if (param is not TagViewState tagItem) return;
            ToggleTagItemSelection(tagItem);
        });
    }

    public ObservableCollection<TagViewState>? Tags
    {
        get => _tags;
        private set
        {
            _tags = value;
            RaisePropertyChanged();
        }
    }

    public ICommand ToggleTagSelectionCommand { get; }
    
    private void ToggleTagItemSelection(TagViewState tagViewState)
    {
        if (_tags == null) return;

        tagViewState.IsSelected = !tagViewState.IsSelected;
        
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagViewState.Name, tagViewState.IsSelected));
    }
    
    private void OnFolderSelected(FolderSelectedEvent folderEvent)
    {
        _associatedFolder = folderEvent.Folder;
        
        Tags = _associatedFolder == null ? null : new ObservableCollection<TagViewState>(
            _associatedFolder.Tags.Select(tag => new TagViewState(tag.Name))
        );
    }
    
    private void OnTagAdded(TagAddedEvent tagEvent)
    {
        var createdTag = tagEvent.CreatedTag;
        
        if (_tags == null || createdTag.FolderId != _associatedFolder?.Id) return;

        if (_tags.All(tagState => tagState.Name != createdTag.Name))
        {
            _tags.Add(new TagViewState(createdTag.Name));
        }
    }
    
    private void OnTagRemoved(TagRemovedEvent tagEvent)
    {
        var removedTag = tagEvent.RemovedTag;
        
        if (_tags == null || removedTag.FolderId != _associatedFolder?.Id) return;
        
        if (_associatedFolder.Tags.All(tag => tag.Name != removedTag.Name))
        {
            var tagState = _tags.FirstOrDefault(tagState => tagState.Name == removedTag.Name);

            if (tagState == null) return;
            
            if (tagState.IsSelected)
            {
                _eventAggregator.Publish(new TagSelectionChangedEvent(tagState.Name, false));
            }
                
            _tags.Remove(tagState);
        }
    }

    private void OnTagSelectionChanged(TagSelectionChangedEvent tagEvent)
    {
        var tagState = _tags?.FirstOrDefault(item => item.Name == tagEvent.TagName);

        if (tagState == null) return;

        tagState.IsSelected = tagEvent.IsSelected;
    }
}