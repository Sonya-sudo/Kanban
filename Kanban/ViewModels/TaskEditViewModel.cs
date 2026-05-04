using System;
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
        private string _priority;
        private string _description;
        private DateOnly? _deadline;
        private Window _window;

        public TaskEditViewModel(Models.Task task)
        {
            _task = task;
            _title = task.Title;
            _priority = task.Priority;
            _description = task.Description;
            _deadline = task.Deadline;

            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Priority { get => _priority; set => SetProperty(ref _priority, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public DateOnly? Deadline { get => _deadline; set => SetProperty(ref _deadline, value); }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public void SetWindow(Window window) => _window = window;

        private void ExecuteSave(object parameter)
        {
            using var db = new KanbanContext();
            var existingTask = db.Tasks.FirstOrDefault(t => t.Id == _task.Id);
            if (existingTask != null)
            {
                existingTask.Title = Title;
                existingTask.Priority = Priority;
                existingTask.Description = Description;
                existingTask.Deadline = Deadline;
                db.SaveChanges();
            }
            _window.DialogResult = true;
            _window.Close();
        }

        private void ExecuteCancel(object parameter)
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}