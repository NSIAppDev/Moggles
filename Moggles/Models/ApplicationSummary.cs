using System;

namespace Moggles.Models
{
    public class ApplicationSummary
    {
        public Guid Id { get; set; }
        public string AppName { get; set; }
        public bool HasBeenMigrated { get; set; }
        public bool IsDeleted { get; set; }
    }
}
