﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sdl.Community.XLIFF.Manager.Common;
using Sdl.Community.XLIFF.Manager.Controls;
using Sdl.Community.XLIFF.Manager.CustomEventArgs;
using Sdl.Community.XLIFF.Manager.Model;
using Sdl.Community.XLIFF.Manager.Model.ProjectSettings;
using Sdl.Community.XLIFF.Manager.Service;
using Sdl.Community.XLIFF.Manager.ViewModel;
using Sdl.Core.Settings;
using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.Desktop.IntegrationApi.Interfaces;
using Sdl.Desktop.IntegrationApi.Notifications.Events;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using Sdl.TranslationStudioAutomation.IntegrationApi.Presentation.DefaultLocations;

namespace Sdl.Community.XLIFF.Manager
{
	[View(
		Id = "XLIFFManager_View",
		Name = "XLIFFManager_Name",
		Description = "XLIFFManager_Description",
		Icon = "Icon",
		AllowViewParts = true,
		LocationByType = typeof(TranslationStudioDefaultViews.TradosStudioViewsLocation))]
	public class XLIFFManagerViewController : AbstractViewController
	{
		private readonly object _lockObject = new object();
		private List<Project> _projects;
		private ProjectFilesViewModel _projectFilesViewModel;
		private ProjectsNavigationViewModel _projectsNavigationViewModel;
		private ProjectFilesViewControl _projectFilesViewControl;
		private ProjectsNavigationViewControl _projectsNavigationViewControl;
		private ProjectFileActivityViewController _projectFileActivityViewController;
		private IStudioEventAggregator _eventAggregator;
		private ProjectsController _projectsController;
		private ImageService _imageService;
		private PathInfo _pathInfo;

		protected override void Initialize(IViewContext context)
		{
			_pathInfo = new PathInfo();
			_imageService = new ImageService();

			ActivationChanged += OnActivationChanged;

			_eventAggregator = SdlTradosStudio.Application.GetService<IStudioEventAggregator>();
			_eventAggregator.GetEvent<StudioWindowCreatedNotificationEvent>()?.Subscribe(OnStudioWindowCreatedNotificationEvent);

			_projectsController = SdlTradosStudio.Application.GetController<ProjectsController>();

			_projects = new List<Project>();

			// TODO this will be replaced with a call to recover the relevant data from the projects loaded in Studio			
			//var testDataUtil = new TestDataUtil(_imageService);
			//_projectModels = testDataUtil.GetTestProjectData();
		}

		protected override Control GetExplorerBarControl()
		{
			if (_projectsNavigationViewControl == null)
			{
				_projectsNavigationViewModel = new ProjectsNavigationViewModel(_projects);
				_projectsNavigationViewModel.ProjectSelectionChanged += OnProjectSelectionChanged;

				_projectsNavigationViewControl = new ProjectsNavigationViewControl(_projectsNavigationViewModel);
			}

			return _projectsNavigationViewControl;
		}

		protected override Control GetContentControl()
		{
			if (_projectFilesViewControl == null)
			{
				_projectFilesViewModel = new ProjectFilesViewModel(_projects?.Count > 0
					? _projects[0].ProjectFiles : null);

				_projectFilesViewControl = new ProjectFilesViewControl(_projectFilesViewModel);
				_projectsNavigationViewModel.ProjectFilesViewModel = _projectFilesViewModel;
			}

			return _projectFilesViewControl;
		}

		public EventHandler<ProjectSelectionChangedEventArgs> ProjectSelectionChanged;

		public List<Project> GetProjects()
		{
			return _projects;
		}

		public List<Project> GetSelectedProjects()
		{
			return _projectsNavigationViewModel.SelectedProjects?.Cast<Project>().ToList();
		}

		public List<ProjectFile> GetSelectedProjectFiles()
		{
			return _projectFilesViewModel.SelectedProjectFiles?.Cast<ProjectFile>().ToList();
		}

		public void UpdateProjectData(WizardContext wizardContext)
		{
			if (wizardContext == null || !wizardContext.Completed)
			{
				return;
			}

			_projects = new List<Project>(_projects);
			var project = _projects.FirstOrDefault(a => a.Id == wizardContext.Project.Id);

			if (project == null)
			{
				_projects.Add(wizardContext.Project);
				project = wizardContext.Project;
			}
			else
			{
				foreach (var wcProjectFile in wizardContext.ProjectFiles)
				{
					var projectFile = project.ProjectFiles.FirstOrDefault(a => a.FileId == wcProjectFile.FileId);
					if (projectFile == null)
					{
						project.ProjectFiles.Add(wcProjectFile);
					}
					else
					{
						projectFile.Status = wcProjectFile.Status;
						projectFile.Action = wcProjectFile.Action;
						projectFile.Date = wcProjectFile.Date;
						projectFile.XliffFilePath = wcProjectFile.XliffFilePath;
						//projectFile.Details = wcProjectFile.Details;

						projectFile.ProjectFileActivities = wcProjectFile.ProjectFileActivities;
					}
				}
			}

			if (_projectsNavigationViewModel != null)
			{
				_projectsNavigationViewModel.Projects = _projects;
			}

			if (_projectFilesViewModel != null)
			{
				_projectFilesViewModel.ProjectFiles = project.ProjectFiles;
			}


			UpdateProjectSettingsBundle(project);
		}

		public void UpdateProjectSettingsBundle(Project project)
		{
			var selectedProject = _projectsController.GetProjects()
				.FirstOrDefault(a => a.GetProjectInfo().Id.ToString() == project.Id);

			if (selectedProject != null)
			{
				var settingsGroup = selectedProject.GetSettings().GetSettingsGroup<XliffManagerProject>();

				var settingsProjectFiles = new List<XliffManagerProjectFile>();
				foreach (var projectFile in project.ProjectFiles)
				{
					var settingsFileActivities = new List<XliffManagerProjectFileActivity>();

					foreach (var fileActivity in projectFile.ProjectFileActivities)
					{
						var settingFileActivity = new XliffManagerProjectFileActivity();
						settingFileActivity.ProjectFileId.Value = projectFile.FileId;
						settingFileActivity.ActivityId.Value = fileActivity.ActivityId;
						settingFileActivity.Status.Value = fileActivity.Status.ToString();
						settingFileActivity.Action.Value = fileActivity.Action.ToString();
						settingFileActivity.Path.Value = fileActivity.Path;
						settingFileActivity.Name.Value = fileActivity.Name;
						settingFileActivity.Date.Value = fileActivity.DateToString;
						settingFileActivity.Details.Value = fileActivity.Details;

						settingsFileActivities.Add(settingFileActivity);
					}

					var settingProjectFile = new XliffManagerProjectFile();
					settingProjectFile.Activities.Value = settingsFileActivities;
					settingProjectFile.Path.Value = projectFile.Path;
					settingProjectFile.Action.Value = projectFile.Action.ToString();
					settingProjectFile.Status.Value = projectFile.Status.ToString();
					settingProjectFile.FileId.Value = projectFile.FileId;
					settingProjectFile.Path.Value = projectFile.Path;
					settingProjectFile.Name.Value = projectFile.Name;
					settingProjectFile.Location.Value = projectFile.Location;
					settingProjectFile.Date.Value = projectFile.DateToString;
					settingProjectFile.FileType.Value = projectFile.FileType;
					settingProjectFile.XliffFilePath.Value = projectFile.XliffFilePath;
					settingProjectFile.TargetLanguage.Value = projectFile.TargetLanguage.CultureInfo.Name;
					settingProjectFile.Details.Value = projectFile.Details;
					settingProjectFile.ShortMessage.Value = projectFile.ShortMessage;

					settingsProjectFiles.Add(settingProjectFile);
				}

				settingsGroup.ProjectFiles.Value = settingsProjectFiles;

				selectedProject.UpdateSettings(settingsGroup.SettingsBundle);
			}
		}

		private void OnProjectSelectionChanged(object sender, ProjectSelectionChangedEventArgs e)
		{
			ProjectSelectionChanged?.Invoke(this, e);
		}

		private void OnStudioWindowCreatedNotificationEvent(StudioWindowCreatedNotificationEvent e)
		{

		}

		private void OnActivationChanged(object sender, ActivationChangedEventArgs e)
		{
			if (e.Active)
			{
				SetProjectFileActivityViewController();
			}
		}

		private void SetProjectFileActivityViewController()
		{
			lock (_lockObject)
			{
				if (_projectFileActivityViewController != null)
				{
					return;
				}

				try
				{
					_projectFileActivityViewController =
						SdlTradosStudio.Application.GetController<ProjectFileActivityViewController>();

					_projectFilesViewModel.ProjectFileActivityViewModel =
						new ProjectFileActivityViewModel(_projectFilesViewModel?.SelectedProjectFile?.ProjectFileActivities);

					_projectFileActivityViewController.ViewModel = _projectFilesViewModel.ProjectFileActivityViewModel;
				}
				catch
				{
					// catch all; unable to locate the controller
				}
			}
		}

		public override void Dispose()
		{


			_projectFilesViewModel?.Dispose();

			if (_projectsNavigationViewModel != null)
			{
				_projectsNavigationViewModel.ProjectSelectionChanged -= OnProjectSelectionChanged;
				_projectsNavigationViewModel.Dispose();
			}


			base.Dispose();
		}
	}
}
