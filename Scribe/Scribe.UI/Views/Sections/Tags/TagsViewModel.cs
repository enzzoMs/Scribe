using System.Collections.ObjectModel;
using Scribe.Data.Model;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.Tags;

public record TagInfo(string Name, int UsageCount);

public class TagsViewModel : BaseViewModel
{
    private Folder? _associatedFolder;
    private ObservableCollection<TagInfo>? _tagsInfo;
    
    public TagsViewModel(IEventAggregator eventAggregator)
    {
        eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);
        eventAggregator.Subscribe<TagAddedEvent>(this, OnTagAdded);
        eventAggregator.Subscribe<TagRemovedEvent>(this, OnTagRemoved);
    }

    public ObservableCollection<TagInfo>? TagsInfo
    {
        get => _tagsInfo;
        private set
        {
            _tagsInfo = value;
            RaisePropertyChanged();
        }
    }
    
    private void OnFolderSelected(FolderSelectedEvent folderEvent)
    {
        _associatedFolder = folderEvent.Folder;

        var tagsInfo = _associatedFolder?.Tags.Select(tag => 
            new TagInfo(
                Name: tag.Name, 
                UsageCount: _associatedFolder.Documents.Count(doc => doc.Tags.Any(docTag => docTag.Name == tag.Name)))
        ).ToList();

        TagsInfo = tagsInfo == null ? null : new ObservableCollection<TagInfo>(tagsInfo);
    }
    
    private void OnTagAdded(TagAddedEvent tagEvent)
    {
        var createdTag = tagEvent.CreatedTag;
        
        if (_tagsInfo == null || createdTag.FolderId != _associatedFolder?.Id) return;

        if (_tagsInfo.All(tag => tag.Name != createdTag.Name))
        {
            _tagsInfo.Add(new TagInfo(createdTag.Name, 1));
        }
        else
        {
            var associatedTagInfo = _tagsInfo.First(tagInfo => tagInfo.Name == createdTag.Name);
            var tagIndex = _tagsInfo.IndexOf(associatedTagInfo);
            
            _tagsInfo[tagIndex] = associatedTagInfo with { UsageCount = associatedTagInfo.UsageCount + 1 };
        }
    }
    
    private void OnTagRemoved(TagRemovedEvent tagEvent)
    {
        var removedTag = tagEvent.RemovedTag;
        
        if (_tagsInfo == null || removedTag.FolderId != _associatedFolder?.Id) return;

        var associatedTagInfo = _tagsInfo.First(tagInfo => tagInfo.Name == removedTag.Name);
        var tagIndex = _tagsInfo.IndexOf(associatedTagInfo);

        if (associatedTagInfo.UsageCount == 1)
        {
            _tagsInfo.Remove(associatedTagInfo);
        }
        else
        {
            _tagsInfo[tagIndex] = associatedTagInfo with { UsageCount = associatedTagInfo.UsageCount - 1 };
        }
    }
}