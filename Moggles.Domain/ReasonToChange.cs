using System;
using System.Collections.Generic;
using System.Text;

namespace Moggles.Domain
{
    public class ReasonToChange : AggregateRoot
    {
        public string AddedByUser { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }

        public static ReasonToChange Create(string addedByUser, string description)
        {
            return new ReasonToChange
            {
                Id = Guid.NewGuid(),
                AddedByUser = addedByUser,
                DateAdded = DateTime.UtcNow,
                Description = description
            };
        }
    }
}
