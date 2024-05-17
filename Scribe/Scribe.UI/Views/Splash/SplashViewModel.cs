﻿using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Splash;

public class SplashViewModel(
    IEventAggregator eventAggregator, IRepository<Folder> folderRepository
) : BaseViewModel
{
    private readonly IRepository<Folder> _folderRepository = folderRepository;
    private readonly IEventAggregator _eventAggregator = eventAggregator;

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
        _folders ??= await _folderRepository.GetAll();
        CheckSplashCompletion();
    } 

    private void CheckSplashCompletion() => SplashCompleted = _logoAnimationFinished && _folders != null;
    
    private void PublishLoadEvent()
    {
        if (!_loadEventPublished && SplashCompleted)
        {
            _eventAggregator.Publish(new FoldersLoadedEvent(_folders!));
            _loadEventPublished = true;
        }
    }
}