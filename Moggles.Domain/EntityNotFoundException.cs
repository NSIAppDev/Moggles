using System;

namespace Moggles.Domain
{
    public class EntityNotFoundException : Exception
    {
        public string EntityType { get; }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, string entityType) : base(message)
        {
            EntityType = entityType;
        }
    }
}