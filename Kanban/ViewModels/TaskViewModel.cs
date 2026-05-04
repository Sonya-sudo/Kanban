using Kanban.Models;
using Kanban.ViewModels.Base;

namespace Kanban.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        private readonly Models.Task _model;

        public TaskViewModel(Models.Task model)
        {
            _model = model;
        }

        public int Id => _model.Id;

        public string Title
        {
            get => _model.Title;
            set
            {
                if (_model.Title != value)
                {
                    _model.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Priority
        {
            get => _model.Priority;
            set
            {
                if (_model.Priority != value)
                {
                    _model.Priority = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Escalated => _model.Escalated ?? false;

        public void UpdateInDatabase()
        {
            using var db = new KanbanContext();
            var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
            if (task != null)
            {
                task.Title = _model.Title;
                task.Priority = _model.Priority;
                db.SaveChanges();
            }
        }
    }
}