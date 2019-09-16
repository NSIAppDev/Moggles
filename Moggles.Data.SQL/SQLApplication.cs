using System;

namespace Moggles.Data.SQL
{
    public class SQLApplication
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public Guid NewId { get; set; } = Guid.NewGuid();

    }
}