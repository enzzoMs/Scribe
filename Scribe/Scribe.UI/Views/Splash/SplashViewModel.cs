using Scribe.Data.Model;
using Scribe.Data.Repositories;

namespace Scribe.UI.Views.Splash;

public class SplashViewModel(IRepository<Folder> folderRepository) : BaseViewModel
{
    private readonly IRepository<Folder> _folderRepository = folderRepository;
    private bool _splashEnded;
    private List<Folder>? _folders;
    
    public void EndSplash() => _splashEnded = true;

    public override async Task Load() => _folders = await _folderRepository.GetAll();
}