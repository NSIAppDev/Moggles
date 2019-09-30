using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moggles.Domain;

namespace Moggles.UnitTests
{
    public class InMemoryRepository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly List<T> _entities = new List<T>();
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_entities.ToList());
        }

        public async Task AddAsync(T entity)
        {
            _entities.Add(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _entities.RemoveAll(x => x.Id == entity.Id);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(T entity)
        {
            _entities.RemoveAll(x => x.Id == entity.Id);
            _entities.Add(entity);
            await Task.CompletedTask;
        }

        public async Task<T> FindByIdAsync(Guid id)
        {
            return await Task.FromResult(_entities.FirstOrDefault(x => x.Id == id));
        }
    }
}