using System;
using System.Collections.Generic;

namespace Moggles.Models
{
	public class GlobalSearchModel
	{
		public string Keyword { get; set; }
		public List<Guid> ApplicationIds { get; set; }
		public string ProgressStatus { get; set; }
		public DateTime? CreatedStart { get; set; }
		public DateTime? CreatedEnd { get; set; }
		public List<EnvironmentFilter> Environments { get; set; }
		public string WorkItemIdentifier { get; set; }
		public List<string> AssignedTo { get; set; }
		public bool? IsPermanent { get; set; }
		public DateTime? ChangedStart { get; set; }
		public DateTime? ChangedEnd { get; set; }
		public bool? UserAccepted { get; set; }
	}

	public class EnvironmentFilter
	{
		public string Name { get; set; }
		public bool? Enabled { get; set; }
	}
}
