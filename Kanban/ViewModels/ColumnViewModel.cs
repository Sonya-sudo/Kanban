using System.Collections.ObjectModel;
using System.Linq;
using Kanban.Models;
using Kanban.ViewModels.Base;

namespace Kanban.ViewModels
{
    public class ColumnViewModel : ViewModelBase
    {
        private readonly Column _model;
        private readonly ObservableCollection<TaskViewModel> _tasks;
        private readonly ReadOnlyObservableCollection<TaskViewModel> _tasksReadOnly;

        public ColumnViewModel(Column model)
        {
            _model = model;
            _tasks = new ObservableCollection<TaskViewModel>();
            _tasksReadOnly = new ReadOnlyObservableCollection<TaskViewModel>(_tasks);
        }

        public int Id => _model.Id;
        public string Name => _model.Name;
        public int Position => _model.Position;
        public int? WipLimit => _model.WipLimit;

        public ReadOnlyObservableCollection<TaskViewModel> Tasks => _tasksReadOnly;

        public void LoadTasks()
        {
            using var db = new KanbanContext();
            var tasks = db.Tasks
                .Where(t => t.ColumnId == _model.Id)
                .OrderBy(t => t.OrderInColumn)
                .ToList();

            _tasks.Clear();
            foreach (var task in tasks)
            {
                _tasks.Add(new TaskViewModel(task));  // ← TaskViewModel
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

            _tasks.Add(new TaskViewModel(task));  // ← TaskViewModel
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