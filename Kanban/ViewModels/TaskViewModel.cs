using System.Linq;
using System.Windows.Media;
using Kanban.Models;
using Kanban.ViewModels.Base;

namespace Kanban.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        private readonly Models.Task _model;
        private string _title;
        private string _color;

        public TaskViewModel(Models.Task model)
        {
            _model = model;
            _title = model.Title;
            _color = model.Color ?? "#E2E8F0"; // серый по умолчанию
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

        public string Color
        {
            get => _color;
            set
            {
                if (SetProperty(ref _color, value))
                {
                    _model.Color = value;
                    using var db = new KanbanContext();
                    var task = db.Tasks.FirstOrDefault(t => t.Id == _model.Id);
                    if (task != null)
                    {
                        task.Color = value;
                        db.SaveChanges();
                    }
                }
            }
        }

        public bool Escalated => _model.Escalated ?? false;
    }
}