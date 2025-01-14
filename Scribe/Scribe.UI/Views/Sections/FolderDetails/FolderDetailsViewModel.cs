﻿using System.IO;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Commands;
using Scribe.UI.Events;
using Scribe.UI.Views.Errors;

namespace Scribe.UI.Views.Sections.FolderDetails;

public class FolderDetailsViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRepository<Folder> _foldersRepository;
    private readonly IRepository<Document> _documentsRepository;

    private Folder? _selectedFolder;

    private bool _inEditMode;
    
    public FolderDetailsViewModel(
        IEventAggregator eventAggregator, 
        IRepository<Folder> foldersRepository, 
        IRepository<Document> documentsRepository)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        _documentsRepository = documentsRepository;

        _eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);

        EnterEditModeCommand = new DelegateCommand( _ => InEditMode = true);
        UpdateFolderNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newFolderName) return;
            InEditMode = false;
            UpdateFolderName(newFolderName);
        });
        DeleteFolderCommand = new DelegateCommand(_ => DeleteFolder());
        ImportDocumentsCommand = new DelegateCommand(param =>
        {
            if (param is not string[] documentsFilePaths) return;
            ImportDocuments(documentsFilePaths);
        });
    }
    
    public static DocumentFileFormats DocumentImportFileFormat => DocumentFileFormats.Json;
    
    public Folder? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            _selectedFolder = value;
            InEditMode = false;
            
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FolderNavigationPosition));
        }
    }
    
    public int FolderNavigationPosition
    {
        get => _selectedFolder?.NavigationIndex + 1 ?? 1;
        set => UpdateFolderPosition(value - 1);
    }

    public bool InEditMode
    {
        get => _inEditMode;
        set
        {
            _inEditMode = value;
            RaisePropertyChanged();
        }
    }
    
    public ICommand UpdateFolderNameCommand { get; }

    public ICommand DeleteFolderCommand { get; }
    
    public ICommand EnterEditModeCommand { get; }

    public ICommand ImportDocumentsCommand { get; }
    
    public async void ExportFolder(string directoryPath)
    {
        if (_selectedFolder == null) return;
        try
        {
            await _foldersRepository.ExportToFile(directoryPath, _selectedFolder);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or UnauthorizedAccessException)
        {
            RaiseViewModelError(new DocumentExportError());
        }
    }
    
    private async void ImportDocuments(IEnumerable<string> documentsFilePaths)
    {
        if (_selectedFolder == null) return;

        var newDocuments = new List<Document>();
        
        foreach (var documentPath in documentsFilePaths)
        {
            Document importedDocument;

            try
            {
                importedDocument = await _documentsRepository.ImportFromFile(documentPath);
            }
            catch (FormatException)
            {
                RaiseViewModelError(new DocumentImportFileFormatError(documentPath));
                continue;
            }
            catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException)
            {
                RaiseViewModelError(new DocumentImportFilePathError(documentPath));
                continue;
            }

            // Adding the folder ID to the imported document
            var newDocument = new Document(
                _selectedFolder.Id, importedDocument.CreatedTimestamp, importedDocument.LastModifiedTimestamp, importedDocument.Name
            )
            {
                Content = importedDocument.Content,
                Tags = importedDocument.Tags
                    .Select(importedTag => importedTag.Name)
                    .Distinct()
                    .Select(tagName => new Tag(tagName, _selectedFolder.Id))
                    .ToList()
            };
            
            newDocuments.Add(newDocument);
            
            foreach (var tag in newDocument.Tags)
            {
                _eventAggregator.Publish(new TagAddedEvent(tag));
            }
        }
        
        var addedDocuments = await _documentsRepository.Add(newDocuments.ToArray());

        foreach (var document in addedDocuments)
        {
            _eventAggregator.Publish(new DocumentCreatedEvent(document));
        }
    }
    
    private async void UpdateFolderName(string newFolderName)
    {
        if (_selectedFolder == null || _selectedFolder.Name == newFolderName) return;

        _selectedFolder.Name = newFolderName.Trim();
        await _foldersRepository.Update(_selectedFolder);
        
        RaisePropertyChanged(nameof(SelectedFolder));
        _eventAggregator.Publish(new FolderUpdatedEvent(_selectedFolder.Id));
    }

    private void UpdateFolderPosition(int newIndex)
    {
        if (_selectedFolder == null || _selectedFolder.NavigationIndex == newIndex) return;

        var oldIndex = _selectedFolder.NavigationIndex;
        
        _eventAggregator.Publish(new FolderPositionUpdatedEvent(_selectedFolder.Id, oldIndex, newIndex));
    }

    private async void DeleteFolder()
    {
        if (_selectedFolder == null) return;
        
        await _foldersRepository.Delete(_selectedFolder);
        
        _eventAggregator.Publish(new FolderDeletedEvent(_selectedFolder));
    }

    private void OnFolderSelected(FolderSelectedEvent folderEvent) => SelectedFolder = folderEvent.Folder;
}