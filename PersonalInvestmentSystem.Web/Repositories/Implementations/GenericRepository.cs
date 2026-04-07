using Microsoft.EntityFrameworkCore;
using PersonalInvestmentSystem.Web.Data;
using PersonalInvestmentSystem.Web.Repositories.Interfaces;
using System.Linq.Expressions;

namespace PersonalInvestmentSystem.Web.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            // Get the primary key property
            var keyProperty = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties[0];
            if (keyProperty != null)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, keyProperty.PropertyInfo!);
                var idConstant = Expression.Constant(id);
                var equality = Expression.Equal(propertyAccess, idConstant);
                var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
                return await query.FirstOrDefaultAsync(lambda);
            }
            return null;
        }
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _dbSet.AddRangeAsync(entities);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.CountAsync(predicate);
    }
}
