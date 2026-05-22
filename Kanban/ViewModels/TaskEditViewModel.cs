using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;

namespace Kanban.ViewModels
{
    public class TaskEditViewModel : ViewModelBase
    {
        private readonly Models.Task _task;
        private string _title;
        private string _description;
        private string _priority;
        private DateTime? _deadline;
        private string _status;
        private Window _window;
        private ObservableCollection<SubtaskItemViewModel> _subtasks;

        public TaskEditViewModel(Models.Task task)
        {
            _task = task;
            _title = task.Title;
            _description = task.Description;
            _priority = task.Priority;
            _deadline = task.Deadline;
            _status = task.Status ?? "InProgress";
            _subtasks = new ObservableCollection<SubtaskItemViewModel>();

            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask);
            AddRootSubtaskCommand = new RelayCommand(ExecuteAddRootSubtask);

            LoadSubtasks();
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        public DateTime? Deadline
        {
            get => _deadline;
            set => SetProperty(ref _deadline, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsInProgress => Status == "InProgress";

        public ObservableCollection<SubtaskItemViewModel> Subtasks
        {
            get => _subtasks;
            set => SetProperty(ref _subtasks, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand AddRootSubtaskCommand { get; }

        public void SetWindow(Window window) => _window = window;

        private void LoadSubtasks()
        {
            using var db = new KanbanContext();
            var rootSubtaskIds = db.SubtaskHierarchies
                .Where(h => h.TaskId == _task.Id && h.ParentSubtaskId == null)
                .Select(h => h.SubtaskId)
                .ToList();

            var subtasks = db.Subtasks
                .Where(s => rootSubtaskIds.Contains(s.Id))
                .OrderBy(s => s.Position)
                .ToList();

            Subtasks.Clear();
            foreach (var subtask in subtasks)
            {
                Subtasks.Add(new SubtaskItemViewModel(subtask, this));
            }
        }

        public void RefreshSubtasks()
        {
            LoadSubtasks();
        }

        private void ExecuteAddRootSubtask(object parameter)
        {
            var textBox = parameter as System.Windows.Controls.TextBox;
            var title = textBox?.Text?.Trim();

            if (string.IsNullOrEmpty(title)) return;

            using var db = new KanbanContext();
            var maxPosition = db.Subtasks
                .Where(s => s.TaskId == _task.Id)
                .Max(s => (int?)s.Position) ?? 0;

            var newSubtask = new Subtask
            {
                TaskId = _task.Id,
                Title = title,
                IsCompleted = false,
                Position = maxPosition + 1,
                CreatedAt = DateTime.Now
            };

            db.Subtasks.Add(newSubtask);
            db.SaveChanges();

            var hierarchy = new SubtaskHierarchy
            {
                TaskId = _task.Id,
                SubtaskId = newSubtask.Id,
                ParentSubtaskId = null,
                Level = 0,
                Order = Subtasks.Count + 1
            };
            db.SubtaskHierarchies.Add(hierarchy);
            db.SaveChanges();

            Subtasks.Add(new SubtaskItemViewModel(newSubtask, this));
            textBox.Text = "";
        }

        private void ExecuteSave(object parameter)
        {
            SaveToDatabase();
            _window.DialogResult = true;
            _window.Close();
        }

        private void ExecuteCancel(object parameter)
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private void ExecuteCompleteTask(object parameter)
        {
            Status = "Completed";
            SaveToDatabase();
            _window.DialogResult = true;
            _window.Close();
        }

        private void SaveToDatabase()
        {
            using var db = new KanbanContext();
            var existingTask = db.Tasks.FirstOrDefault(t => t.Id == _task.Id);

            if (existingTask != null)
            {
                existingTask.Title = Title;
                existingTask.Description = Description;
                existingTask.Priority = Priority;
                existingTask.Deadline = Deadline;
                existingTask.Status = Status;
                db.SaveChanges();

                _task.Title = Title;
                _task.Description = Description;
                _task.Priority = Priority;
                _task.Deadline = Deadline;
                _task.Status = Status;
            }
        }
    }
}