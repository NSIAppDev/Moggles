using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moggles.Domain;

namespace Moggles.UnitTests
{
    public class InMemoryApplicationRepository: IRepository<Application>
    {
        private readonly List<Application> _apps = new List<Application>();
        public IEnumerable<Application> Applications => _apps.ToList();
        public async Task<IEnumerable<Application>> GetAllAsync()
        {
            return await Task.FromResult(_apps.ToList());
        }

        public async Task AddAsync(Application entity)
        {
            _apps.Add(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Application entity)
        {
            _apps.RemoveAll(x => x.Id == entity.Id);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Application entity)
        {
            _apps.RemoveAll(x => x.Id == entity.Id);
            _apps.Add(entity);
            await Task.CompletedTask;
        }

        public async Task<Application> FindByIdAsync(Guid id)
        {
            return await Task.FromResult(_apps.FirstOrDefault(x => x.Id == id));
        }
    }
}