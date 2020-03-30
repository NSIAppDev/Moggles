using System;
using System.Collections.Generic;

namespace Moggles.Domain
{
    public class ReasonToChange : AggregateRoot
    {
        public string AddedByUser { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public List<string> Environments { get; set; }

        public static ReasonToChange Create(string addedByUser, string description, List<string> environments)
        {
            return new ReasonToChange
            {
                Id = Guid.NewGuid(),
                AddedByUser = addedByUser,
                DateAdded = DateTime.UtcNow,
                Description = description,
                Environments = environments 
            };
        }
    }
}
