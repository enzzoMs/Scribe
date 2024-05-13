using Scribe.Data.Database;
using Scribe.Data.Model;

namespace Scribe.Data.Repositories;

public class FolderRepository(ScribeContext context) : GenericRepository<Folder>(context);