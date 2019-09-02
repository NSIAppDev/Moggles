using System;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class Application : IEntity
    {
        public Guid Id { get; set; }
        public string AppName { get; set; }
    }
}