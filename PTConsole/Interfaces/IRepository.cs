using System.Linq.Expressions;

namespace PTConsole.Interfaces
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetAsync(Guid id);
        Task CreateAsync(TEntity entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task CreateBatchAsync(IEnumerable<TEntity> entities);
        Task DeleteBatchAsync(IEnumerable<TEntity> entities);
        Task DeleteBatchAsync(IEnumerable<Guid> ids);
        Task UpdateBatchAsync(IEnumerable<TEntity> entities);
    }
}
