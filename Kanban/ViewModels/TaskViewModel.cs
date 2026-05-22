using System;
using System.Linq;
using System.Windows.Input;
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;

namespace Kanban.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        private readonly Models.Task _model;
        private string _title;
        private string _priority;
        private string _description;
        private DateTime? _deadline;
        private string _status;
        private bool _isSelected;

        public TaskViewModel(Models.Task model)
        {
            _model = model;
            _title = model.Title;
            _priority = model.Priority;
            _description = model.Description;
            _deadline = model.Deadline;
            _status = model.Status ?? "InProgress";

            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask);
        }

        public int Id => _model.Id;

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value) && _model.Title != value)
                {
                    _model.Title = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Title = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public string Priority
        {
            get => _priority;
            set
            {
                if (SetProperty(ref _priority, value) && _model.Priority != value)
                {
                    _model.Priority = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Priority = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value) && _model.Description != value)
                {
                    _model.Description = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Description = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public DateTime? Deadline
        {
            get => _deadline;
            set
            {
                if (SetProperty(ref _deadline, value) && _model.Deadline != value)
                {
                    _model.Deadline = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Deadline = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    _model.Status = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Status = value;
                        db.SaveChanges();
                    }
                    OnPropertyChanged(nameof(IsInProgress));
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsInProgress => Status == "InProgress";
        public bool IsCompleted => Status == "Completed";
        public bool Escalated => _model.Escalated ?? false;

        public ICommand CompleteTaskCommand { get; }

        private void ExecuteCompleteTask(object parameter)
        {
            if (IsCompleted) return;
            Status = "Completed";
        }

        public void Restore()
        {
            Status = "InProgress";
        }
    }
}