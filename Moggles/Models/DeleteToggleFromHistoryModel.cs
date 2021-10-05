using System;

namespace Moggles.Models
{
    public class DeleteToggleFromHistoryModel
    {
        public Guid ToggleId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
