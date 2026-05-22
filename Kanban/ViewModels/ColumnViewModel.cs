using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;

namespace Kanban.ViewModels
{
    public class ColumnViewModel : ViewModelBase
    {
        private readonly Column _model;
        private readonly MainViewModel _mainViewModel;
        private string _name;
        private readonly ObservableCollection<TaskViewModel> _tasks;
        private readonly ReadOnlyObservableCollection<TaskViewModel> _tasksReadOnly;

        public ColumnViewModel(Column model, MainViewModel mainViewModel)
        {
            _model = model;
            _mainViewModel = mainViewModel;
            _name = model.Name;
            _tasks = new ObservableCollection<TaskViewModel>();
            _tasksReadOnly = new ReadOnlyObservableCollection<TaskViewModel>(_tasks);

            MoveLeftCommand = new RelayCommand(ExecuteMoveLeft);
            MoveRightCommand = new RelayCommand(ExecuteMoveRight);
        }

        public int Id => _model.Id;

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value) && _model.Name != value)
                {
                    _model.Name = value;
                    using var db = new KanbanContext();
                    var column = db.Columns.FirstOrDefault(c => c.Id == _model.Id);
                    if (column != null)
                    {
                        column.Name = value;
                        db.SaveChanges();
                    }
                }
            }
        }
        public void UpdateName(string newName)
        {
            if (_model.Name != newName)
            {
                _model.Name = newName;
                OnPropertyChanged(nameof(Name));

                // Сохраняем в БД
                using var db = new KanbanContext();
                var column = db.Columns.FirstOrDefault(c => c.Id == _model.Id);
                if (column != null)
                {
                    column.Name = newName;
                    db.SaveChanges();
                }
            }
        }
        public int Position => _model.Position;
        public int? WipLimit => _model.WipLimit;

        public ReadOnlyObservableCollection<TaskViewModel> Tasks => _tasksReadOnly;

        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }

        private void ExecuteMoveLeft(object parameter)
        {
            if (parameter is not TaskViewModel task) return;
            if (_mainViewModel == null) return;

            var columns = _mainViewModel.CurrentColumns;
            if (columns == null || columns.Count == 0) return;

            int currentIndex = columns.IndexOf(this);
            if (currentIndex > 0)
            {
                var targetColumn = columns[currentIndex - 1];
                _mainViewModel.MoveTaskToColumn(task, this, targetColumn);
            }
        }

        private void ExecuteMoveRight(object parameter)
        {
            if (parameter is not TaskViewModel task) return;
            if (_mainViewModel == null) return;

            var columns = _mainViewModel.CurrentColumns;
            if (columns == null || columns.Count == 0) return;

            int currentIndex = columns.IndexOf(this);
            if (currentIndex < columns.Count - 1)
            {
                var targetColumn = columns[currentIndex + 1];
                _mainViewModel.MoveTaskToColumn(task, this, targetColumn);
            }
        }

        public void LoadTasks()
        {
            using var db = new KanbanContext();
            var tasks = db.Tasks
                .Where(t => t.ColumnId == _model.Id && (t.Status == null || t.Status == "InProgress"))
                .OrderBy(t => t.OrderInColumn)
                .ToList();

            _tasks.Clear();
            foreach (var task in tasks)
            {
                _tasks.Add(new TaskViewModel(task));
            }
        }

        public void AddTask(string title, string priority = null)
        {
            using var db = new KanbanContext();
            var maxOrder = db.Tasks
                .Where(t => t.ColumnId == _model.Id)
                .Max(t => (int?)t.OrderInColumn) ?? 0;

            var task = new Models.Task
            {
                ColumnId = _model.Id,
                Title = title,
                Priority = priority,
                OrderInColumn = maxOrder + 1,
                CreatedAt = System.DateTime.Now
            };

            db.Tasks.Add(task);
            db.SaveChanges();

            _tasks.Add(new TaskViewModel(task));
        }

        public void DeleteTask(int taskId)
        {
            using var db = new KanbanContext();
            var task = db.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                db.Tasks.Remove(task);
                db.SaveChanges();

                var taskVM = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskVM != null)
                    _tasks.Remove(taskVM);
            }
        }
    }
}