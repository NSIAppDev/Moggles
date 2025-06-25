using System;
using System.Collections.Generic;

namespace Moggles.Models
{
	public class GlobalSearchFeatureToggleViewModel
	{
		public Guid Id { get; set; }
		public string ToggleName { get; set; }
		public string ApplicationName { get; set; }
		public Guid ApplicationId { get; set; }
		public List<FeatureToggleStatusViewModel> Environments { get; set; }
		public string AssignedTo { get; set; }
		public string WorkItemIdentifier { get; set; }
		public string Notes { get; set; }
		public string ProgressStatus { get; set; }
		public bool UserAccepted { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ChangedDate { get; set; }
		public bool IsPermanent { get; set; }
		public bool HasBeenMigrated { get; set; }
	}
}
