using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Splash;

public class SplashViewModel(
    IEventAggregator eventAggregator, IRepository<Folder> folderRepository
) : BaseViewModel
{
    private readonly IRepository<Folder> _folderRepository = folderRepository;
    private readonly IEventAggregator _eventAggregator = eventAggregator;

    private bool _splashEnded;
    private bool _logoAnimationFinished;
    private List<Folder>? _folders;

    public bool SplashEnded
    {
        get => _splashEnded;
        set
        {
            _splashEnded = value;
            RaisePropertyChanged();
        }
}

    public void EndLogoAnimation()
    {
        _logoAnimationFinished = true;
        PublishLoadEvent();
    }

    public override async Task Load()
    {
        if (_folders == null)
        {
            _folders = await _folderRepository.GetAll();
            PublishLoadEvent();
        }
    }

    private void PublishLoadEvent()
    {
        if (!SplashEnded && _logoAnimationFinished && _folders != null)
        {
            _eventAggregator.Publish(new FoldersLoadedEvent(_folders));
            SplashEnded = true;
        }
    }
}