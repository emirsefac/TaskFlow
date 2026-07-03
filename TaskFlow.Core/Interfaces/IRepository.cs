namespace TaskFlow.Core.Interfaces;

// Tüm repository'lerin ortak kullanacağı temel CRUD işlemleri (generic repository pattern)
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveChangesAsync();
}
