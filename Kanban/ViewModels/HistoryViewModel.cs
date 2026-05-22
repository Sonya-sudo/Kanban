using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;

namespace Kanban.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainVM;
        private ObservableCollection<TaskViewModel> _completedTasks;
        private TaskViewModel _selectedTask;

        public HistoryViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _completedTasks = new ObservableCollection<TaskViewModel>();

            RestoreTaskCommand = new RelayCommand(ExecuteRestoreTask, CanExecuteRestore);
            CloseCommand = new RelayCommand(ExecuteClose);

            LoadCompletedTasks();
        }

        public ObservableCollection<TaskViewModel> CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        public TaskViewModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                SetProperty(ref _selectedTask, value);
                OnPropertyChanged(nameof(HasSelectedTasks));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasSelectedTasks => SelectedTask != null;

        public ICommand RestoreTaskCommand { get; }
        public ICommand CloseCommand { get; }

        private void LoadCompletedTasks()
        {
            using var db = new KanbanContext();
            var tasks = db.Tasks
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            CompletedTasks.Clear();
            foreach (var task in tasks)
            {
                CompletedTasks.Add(new TaskViewModel(task));
            }
        }

        private bool CanExecuteRestore(object parameter)
        {
            return SelectedTask != null;
        }

        private void ExecuteRestoreTask(object parameter)
        {
            if (SelectedTask == null) return;

            using var db = new KanbanContext();
            var task = db.Tasks.FirstOrDefault(t => t.Id == SelectedTask.Id);
            if (task != null)
            {
                task.Status = "InProgress";
                db.SaveChanges();
            }

            // Обновляем статус в ViewModel
            SelectedTask.Status = "InProgress";

            // Удаляем из списка завершённых
            CompletedTasks.Remove(SelectedTask);

            // Обновляем доску
            _mainVM?.RefreshAllColumns();

            // Сбрасываем выделение
            SelectedTask = null;

            // Принудительно обновляем команду
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteClose(object parameter)
        {
            var window = Application.Current.Windows
                .OfType<Views.HistoryWindow>().FirstOrDefault();
            window?.Close();
        }
    }
}