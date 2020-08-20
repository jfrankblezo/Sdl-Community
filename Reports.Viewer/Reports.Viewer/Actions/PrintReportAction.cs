﻿using System.Windows;
using Sdl.Community.Reports.Viewer.Model;
using Sdl.Community.Reports.Viewer.Service;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.Reports.Viewer.API.Model;
using Sdl.TranslationStudioAutomation.IntegrationApi;

namespace Sdl.Community.Reports.Viewer.Actions
{
	[Action("ReportsViewer_PrintReport_Action",
		Name = "ReportsViewer_PrintReport_Name",
		Description = "ReportsViewer_PrintReport_Description",
		ContextByType = typeof(ReportsViewerController),
		Icon = "Print"
	)]
	[ActionLayout(typeof(ReportsViewerReportGroups), 5, DisplayType.Normal)]
	public class PrintReportAction : BaseReportAction
	{
		private PathInfo _pathInfo;
		private ImageService _imageService;
		private ReportsViewerController _reportsViewerController;
		private bool _canEnable;
		private bool _isLoading;

		protected override void Execute()
		{
			_reportsViewerController.Print();
		}

		public override void UpdateEnabled(bool loading)
		{
			_isLoading = loading;
			SetEnabled();
		}

		public override void Initialize()
		{
			_pathInfo = new PathInfo();
			_imageService = new ImageService();
			_reportsViewerController = SdlTradosStudio.Application.GetController<ReportsViewerController>();
			_reportsViewerController.ReportSelectionChanged += ReportsViewerController_ReportSelectionChanged;

			SetEnabled();
		}

		private void ReportsViewerController_ReportSelectionChanged(object sender, CustomEventArgs.ReportSelectionChangedEventArgs e)
		{
			_canEnable = e.SelectedReports?.Count > 0;
			SetEnabled();
		}

		private void SetEnabled()
		{
			Enabled = !_isLoading && _canEnable;
		}
	}
}