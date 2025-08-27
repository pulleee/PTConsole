using PTConsole.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PTConsole.Infrastructure
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DatabaseContext _context;
        private DbSet<TEntity> _dbSet;

        private bool _disposed = false;

        public Repository(DatabaseContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<TEntity?> GetAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task CreateAsync(TEntity entity)
        {
            create(entity);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            update(entity);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            delete(entity);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);

            if (entity == null) return;

            await DeleteAsync(entity);
        }

        public async Task CreateBatchAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                create(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateBatchAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                update(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBatchAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                delete(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBatchAsync(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                var entity = await GetAsync(id);

                if (entity == null) return;

                delete(entity);
            }

            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        private void create(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        private void update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        private void delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbSet.Remove(entity);
        }
    }
}
