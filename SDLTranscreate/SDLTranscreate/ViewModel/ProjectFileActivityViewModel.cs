﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Sdl.Community.Transcreate.Commands;
using Sdl.Community.Transcreate.Model;
using Sdl.Community.Transcreate.View;

namespace Sdl.Community.Transcreate.ViewModel
{
	public class ProjectFileActivityViewModel : BaseModel, IDisposable
	{
		private List<ProjectFileActivity> _projectFileActivities;
		private ProjectFileActivity _selectedProjectFileActivity;
		private IList _selectedProjectFileActivities;
		private bool _isProjectFileSelected;
		private ICommand _openFolderCommand;
		private ICommand _viewReportCommand;

		public ProjectFileActivityViewModel(List<ProjectFileActivity> projectFileActivities)
		{
			ProjectFileActivities = projectFileActivities;
			SelectedProjectFileActivity = ProjectFileActivities?.Count > 0 ? ProjectFileActivities[0] : null;
			SelectedProjectFileActivities = new List<ProjectFileActivity> { SelectedProjectFileActivity };
		}

		public ICommand OpenFolderCommand => _openFolderCommand ?? (_openFolderCommand = new CommandHandler(OpenFolder));

		public ICommand ViewReportCommand => _viewReportCommand ?? (_viewReportCommand = new CommandHandler(ViewReport));

		public List<ProjectFileActivity> ProjectFileActivities
		{
			get => _projectFileActivities ?? (_projectFileActivities = new List<ProjectFileActivity>());
			set
			{
				_projectFileActivities = value;
				OnPropertyChanged(nameof(ProjectFileActivities));
			}
		}

		public IList SelectedProjectFileActivities
		{
			get => _selectedProjectFileActivities;
			set
			{
				_selectedProjectFileActivities = value;
				OnPropertyChanged(nameof(SelectedProjectFileActivities));
			}
		}

		public ProjectFileActivity SelectedProjectFileActivity
		{
			get => _selectedProjectFileActivity;
			set
			{
				_selectedProjectFileActivity = value;
				OnPropertyChanged(nameof(SelectedProjectFileActivity));

				IsProjectFileSelected = _selectedProjectFileActivity != null;
			}
		}

		public bool IsProjectFileSelected
		{
			get => _isProjectFileSelected;
			set
			{
				if (_isProjectFileSelected == value)
				{
					return;
				}

				_isProjectFileSelected = value;
				OnPropertyChanged(nameof(IsProjectFileSelected));
			}
		}

		private void OpenFolder(object parameter)
		{
			if (SelectedProjectFileActivity.ProjectFile?.Project?.Path == null)
			{
				return;
			}

			var path = Path.Combine(SelectedProjectFileActivity.ProjectFile.Project.Path, SelectedProjectFileActivity.Path.Trim('\\'));
			if (Directory.Exists(path))
			{
				System.Diagnostics.Process.Start("explorer.exe", path);
			}
		}

		private void ViewReport(object parameter)
		{
			if (string.IsNullOrEmpty(SelectedProjectFileActivity?.Report))
			{
				return;
			}

			if (SelectedProjectFileActivity.ProjectFile?.Project?.Path == null)
			{
				return;
			}

			var path = Path.Combine(SelectedProjectFileActivity.ProjectFile.Project.Path, SelectedProjectFileActivity.Report.Trim('\\'));
			if (File.Exists(path))
			{
				var viewModel = new ReportViewModel
				{
					HtmlUri = path,
					WindowTitle = "Report"
				};

				var view = new ReportWindow(viewModel);
				view.ShowDialog();
			}
		}

		public void Dispose()
		{
		}
	}
}
