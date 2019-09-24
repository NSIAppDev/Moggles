using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moggles.Domain;

namespace Moggles.UnitTests
{
    public class InMemoryRepository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly List<T> _apps = new List<T>();
        public IEnumerable<T> Applications => _apps.ToList();
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_apps.ToList());
        }

        public async Task AddAsync(T entity)
        {
            _apps.Add(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _apps.RemoveAll(x => x.Id == entity.Id);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(T entity)
        {
            _apps.RemoveAll(x => x.Id == entity.Id);
            _apps.Add(entity);
            await Task.CompletedTask;
        }

        public async Task<T> FindByIdAsync(Guid id)
        {
            return await Task.FromResult(_apps.FirstOrDefault(x => x.Id == id));
        }
    }
}