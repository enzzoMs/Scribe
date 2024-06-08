using System.Collections.ObjectModel;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.UI.Command;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.Editor;

public class EditorViewModel : BaseViewModel
{
    private readonly Action<DocumentSelectedEvent> _onDocumentSelected;
    private Document? _selectedDocument;
    
    public EditorViewModel(IEventAggregator eventAggregator)
    {
        _onDocumentSelected = docEvent =>
        {
            if (!OpenDocuments.Contains(docEvent.Document))
                OpenDocuments.Add(docEvent.Document);

            SelectedDocument = docEvent.Document;
        };
        
        eventAggregator.Subscribe(_onDocumentSelected);
        
        CloseDocumentCommand = new DelegateCommand(param =>
        {
            if (param is Document doc) CloseDocument(doc);
        });
    }
    
    public ObservableCollection<Document> OpenDocuments { get; } = [];

    public Document? SelectedDocument { 
        get => _selectedDocument;
        set
        {
            _selectedDocument = value;
            RaisePropertyChanged();
        }
    }

    public ICommand CloseDocumentCommand { get; }

    private void CloseDocument(Document document)
    {
        var documentIndex = OpenDocuments.IndexOf(document);

        OpenDocuments.Remove(document);

        if (_selectedDocument == document)
        {
            Document? nextSelectedDocument = null;

            if (documentIndex == 0 && OpenDocuments.Count >= 1) 
                nextSelectedDocument = OpenDocuments[0];
            else if (OpenDocuments.Count >= 1)
                nextSelectedDocument = OpenDocuments[documentIndex - 1];

            SelectedDocument = nextSelectedDocument;
        }
    }
}