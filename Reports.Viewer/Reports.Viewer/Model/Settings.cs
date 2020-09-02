﻿namespace Sdl.Community.Reports.Viewer.Model
{
	public class Settings
	{
		public Settings()
		{
			DisplayDateSuffixWithReportName = true;
			GroupByType = "Language";
		}

		public bool DisplayDateSuffixWithReportName { get; set; }

		public string GroupByType { get; set; }
	}
}
