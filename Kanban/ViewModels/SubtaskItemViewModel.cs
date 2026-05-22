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
    public class SubtaskItemViewModel : ViewModelBase
    {
        private readonly Subtask _model;
        private readonly TaskEditViewModel _parentVM;
        private string _title;
        private bool? _isCompleted;
        private ObservableCollection<SubtaskItemViewModel> _children;

        public SubtaskItemViewModel(Subtask model, TaskEditViewModel parentVM)
        {
            _model = model;
            _parentVM = parentVM;
            _title = model.Title;
            _isCompleted = model.IsCompleted;
            _children = new ObservableCollection<SubtaskItemViewModel>();

            RemoveCommand = new RelayCommand(ExecuteRemove);
            AddChildCommand = new RelayCommand(ExecuteAddChild);

            LoadChildren();
        }

        public int Id => _model.Id;
        public int TaskId => _model.TaskId;

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    _model.Title = value;
                    using var db = new KanbanContext();
                    var subtask = db.Subtasks.FirstOrDefault(s => s.Id == _model.Id);
                    if (subtask != null)
                    {
                        subtask.Title = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public bool? IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    _model.IsCompleted = value;
                    using var db = new KanbanContext();
                    var subtask = db.Subtasks.FirstOrDefault(s => s.Id == _model.Id);
                    if (subtask != null)
                    {
                        subtask.IsCompleted = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public ObservableCollection<SubtaskItemViewModel> Children => _children;

        public ICommand RemoveCommand { get; }
        public ICommand AddChildCommand { get; }

        private void LoadChildren()
        {
            using var db = new KanbanContext();
            var childrenIds = db.SubtaskHierarchies
                .Where(h => h.ParentSubtaskId == _model.Id)
                .Select(h => h.SubtaskId)
                .ToList();

            var children = db.Subtasks
                .Where(s => childrenIds.Contains(s.Id))
                .OrderBy(s => s.Position)
                .ToList();

            _children.Clear();
            foreach (var child in children)
            {
                _children.Add(new SubtaskItemViewModel(child, _parentVM));
            }
        }

        private void ExecuteRemove(object parameter)
        {
            using var db = new KanbanContext();
            var hierarchies = db.SubtaskHierarchies.Where(h => h.SubtaskId == _model.Id || h.ParentSubtaskId == _model.Id).ToList();
            db.SubtaskHierarchies.RemoveRange(hierarchies);

            var subtask = db.Subtasks.FirstOrDefault(s => s.Id == _model.Id);
            if (subtask != null)
            {
                db.Subtasks.Remove(subtask);
                db.SaveChanges();
            }

            _parentVM?.RefreshSubtasks();
        }

        private void ExecuteAddChild(object parameter)
        {
            var title = parameter as string;
            if (string.IsNullOrEmpty(title)) return;

            using var db = new KanbanContext();

            // Создаём новую подзадачу
            var newSubtask = new Subtask
            {
                TaskId = _model.TaskId,
                Title = title,
                IsCompleted = false,
                Position = (_children.Count + 1) * 10,
                CreatedAt = DateTime.Now
            };
            db.Subtasks.Add(newSubtask);
            db.SaveChanges();

            // Создаём связь родитель-ребёнок
            var hierarchy = new SubtaskHierarchy
            {
                TaskId = _model.TaskId,
                SubtaskId = newSubtask.Id,
                ParentSubtaskId = _model.Id,
                Level = 1,
                Order = _children.Count + 1
            };
            db.SubtaskHierarchies.Add(hierarchy);
            db.SaveChanges();

            // Добавляем в UI
            _children.Add(new SubtaskItemViewModel(newSubtask, _parentVM));
        }
    }
}