﻿using System.Linq;
using System.Windows;
using Sdl.Community.XLIFF.Manager.Common;
using Sdl.Community.XLIFF.Manager.Interfaces;
using Sdl.Community.XLIFF.Manager.FileTypeSupport.SDLXLIFF;
using Sdl.Community.XLIFF.Manager.Model;
using Sdl.Community.XLIFF.Manager.Service;
using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using Sdl.TranslationStudioAutomation.IntegrationApi.Presentation.DefaultLocations;

namespace Sdl.Community.XLIFF.Manager.Actions.Export
{
	[Action("XLIFFManager_FilesContextMenu_ExportToXLIFF_Action", typeof(FilesController),
		Name = "XLIFFManager_ContextMenu_ExportToXLIFF_Name",
		Icon = "ExportTo",
		Description = "XLIFFManager_ContextMenu_ExportToXLIFF_Description")]
	[ActionLayout(typeof(TranslationStudioDefaultContextMenus.FilesContextMenuLocation), 8, DisplayType.Default, "", true)]
	public class XLIFFManagerFilesContextMenuExportToXLIFFAction : AbstractAction
	{
		private Controllers _controllers;
		private CustomerProvider _customerProvider;
		private PathInfo _pathInfo;
		private ImageService _imageService;
		private IDialogService _dialogService;
		private SegmentBuilder _segmentBuilder;

		protected override void Execute()
		{
			var wizardService = new WizardService(Enumerators.Action.Export, _pathInfo, _customerProvider,
				_imageService, _controllers, _segmentBuilder, _dialogService);

			var wizardContext = wizardService.ShowWizard(_controllers.FilesController, out var message);
			if (wizardContext == null && !string.IsNullOrEmpty(message))
			{				
				MessageBox.Show(message, PluginResources.XLIFFManager_Name, MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			_controllers.XliffManagerController.UpdateProjectData(wizardContext);
		}

		public override void Initialize()
		{
			_controllers = new Controllers();
			SetProjectsController();
			SetFilesController();
			_customerProvider = new CustomerProvider();
			_pathInfo = new PathInfo();
			_imageService = new ImageService();
			_dialogService = new DialogService();
			_segmentBuilder = new SegmentBuilder();

			SetEnabled();
		}

		private void SetProjectsController()
		{
			if (_controllers.ProjectsController != null)
			{
				_controllers.ProjectsController.SelectedProjectsChanged += ProjectsController_SelectedProjectsChanged;
			}
		}

		private void SetFilesController()
		{
			if (_controllers.FilesController != null)
			{
				_controllers.FilesController.SelectedFilesChanged += FilesController_SelectedFilesChanged;
			}
		}

		private void FilesController_SelectedFilesChanged(object sender, System.EventArgs e)
		{
			SetEnabled();
		}

		private void ProjectsController_SelectedProjectsChanged(object sender, System.EventArgs e)
		{
			SetEnabled();
		}

		private void SetEnabled()
		{
			Enabled = _controllers.FilesController.SelectedFiles.Any();
		}
	}
}
