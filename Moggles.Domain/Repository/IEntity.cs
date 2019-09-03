using System;

namespace Moggles.Domain.Repository
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
