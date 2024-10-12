using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Screens.Splash;

public class SplashViewModel(
    IEventAggregator eventAggregator, IRepository<Folder> folderRepository
) : BaseViewModel
{
    private bool _loadEventPublished;
    
    private bool _splashCompleted;
    private bool _logoAnimationFinished;
    
    private List<Folder>? _folders;
    
    public bool SplashCompleted
    {
        get => _splashCompleted; 
        set
        {
            _splashCompleted = value;
            RaisePropertyChanged();
        }
    }
    
    public void FinishSplash() => PublishLoadEvent();
        
    public void FinishLogoAnimation()
    {
        _logoAnimationFinished = true;
        CheckSplashCompletion();
    }
    
    public override async Task Load()
    {
        _folders ??= await folderRepository.GetAll();
        CheckSplashCompletion();
    } 

    private void CheckSplashCompletion() => SplashCompleted = _logoAnimationFinished && _folders != null;
    
    private void PublishLoadEvent()
    {
        if (!_loadEventPublished && SplashCompleted)
        {
            eventAggregator.Publish(new FoldersLoadedEvent(_folders!));
            _loadEventPublished = true;
        }
    }
}