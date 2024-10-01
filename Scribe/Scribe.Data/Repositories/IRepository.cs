namespace Scribe.Data.Repositories;

public interface IRepository<T>
{
    Task<List<T>> Add(params T[] entity);
    Task Update(params T[] entities);
    Task Delete(params T[] entities);
    Task<List<T>> GetAll();
    
    /// <exception cref="DirectoryNotFoundException">If the directory could not be found.</exception>
    /// <exception cref="UnauthorizedAccessException">When the operating system denies access to the directory.</exception>
    Task ExportToFile(string directoryPath, T entity);
    
    /// <exception cref="FileNotFoundException">If the file could not be found.</exception>
    /// <exception cref="UnauthorizedAccessException">When the operating system denies access to the file.</exception>
    /// <exception cref="FormatException">If the file is in an invalid format.</exception>
    /// <returns>The entity of type <typeparamref name="T"/> or null if the file could not be parsed.</returns>
    Task<T> ImportFromFile(string filePath);
}