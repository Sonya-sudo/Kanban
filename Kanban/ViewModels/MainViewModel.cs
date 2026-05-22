
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;
using Kanban.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Kanban.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly User _currentUser;
        private BoardViewModel _selectedBoard;
        private string _currentBoardTitle = "Загрузка...";
        private bool _isLoading;
        private readonly ObservableCollection<BoardViewModel> _boards;
        private readonly ReadOnlyObservableCollection<BoardViewModel> _boardsReadOnly;
        private readonly ObservableCollection<ColumnViewModel> _currentColumns;
        private readonly ReadOnlyObservableCollection<ColumnViewModel> _currentColumnsReadOnly;

        public ICommand RenameBoardCommand { get; }
        public MainViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _boards = new ObservableCollection<BoardViewModel>();
            _boardsReadOnly = new ReadOnlyObservableCollection<BoardViewModel>(_boards);
            _currentColumns = new ObservableCollection<ColumnViewModel>();
            _currentColumnsReadOnly = new ReadOnlyObservableCollection<ColumnViewModel>(_currentColumns);

            AddBoardCommand = new RelayCommand(ExecuteAddBoard);
            ExitCommand = new RelayCommand(ExecuteExit);
            SelectBoardCommand = new RelayCommand(ExecuteSelectBoard);
            AddColumnCommand = new RelayCommand(ExecuteAddColumn);
            DeleteColumnCommand = new RelayCommand(ExecuteDeleteColumn, CanExecuteDeleteColumn);
            DeleteBoardCommand = new RelayCommand(ExecuteDeleteBoard);
            AddTaskCommand = new RelayCommand(ExecuteAddTask);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask);
            OpenTaskEditCommand = new RelayCommand(ExecuteOpenTaskEdit);
            OpenProfileCommand = new RelayCommand(ExecuteOpenProfile);
            RenameBoardCommand = new RelayCommand(ExecuteRenameBoard);
            RenameColumnCommand = new RelayCommand(ExecuteRenameColumn);
            OpenHistoryCommand = new RelayCommand(ExecuteOpenHistory);

            LoadBoards();
        }

        public ReadOnlyObservableCollection<BoardViewModel> Boards => _boardsReadOnly;
        public ReadOnlyObservableCollection<ColumnViewModel> CurrentColumns => _currentColumnsReadOnly;

        public ICommand RenameColumnCommand { get; }
        public ICommand AddBoardCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand SelectBoardCommand { get; }
        public ICommand AddColumnCommand { get; }
        public ICommand DeleteColumnCommand { get; }
        public ICommand DeleteBoardCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand OpenTaskEditCommand { get; }
        public ICommand OpenProfileCommand { get; }
        public ICommand OpenHistoryCommand { get; }


        private void ExecuteOpenHistory(object parameter)
        {
            var historyVM = new HistoryViewModel(this);
            var historyWindow = new HistoryWindow();
            historyWindow.DataContext = historyVM;
            historyWindow.Owner = Application.Current.MainWindow;
            historyWindow.ShowDialog();
        }

        public void RefreshAllColumns()
        {
            if (SelectedBoard != null)
                LoadColumns(SelectedBoard.Id);
        }

        private void ExecuteRenameColumn(object parameter)
        {
            if (parameter is not ColumnViewModel column) return;

            var inputDialog = new InputDialog("Введите новое название колонки", column.Name);
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                var newName = inputDialog.Answer;

                // Сохраняем в БД
                using var db = new KanbanContext();
                var dbColumn = db.Columns.FirstOrDefault(c => c.Id == column.Id);
                if (dbColumn != null)
                {
                    dbColumn.Name = newName;
                    db.SaveChanges();
                }

                // Принудительно обновляем название в ViewModel
                column.UpdateName(newName);

                // Дополнительно: перезагружаем колонки, чтобы UI точно обновился
                LoadColumns(SelectedBoard.Id);
            }
        }

        private void ExecuteRenameBoard(object parameter)
        {
            if (SelectedBoard == null) return;

            var inputDialog = new InputDialog("Введите новое название доски", SelectedBoard.Name);
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                var newName = inputDialog.Answer;

                using var db = new KanbanContext();
                var board = db.Boards.FirstOrDefault(b => b.Id == SelectedBoard.Id);
                if (board != null)
                {
                    board.Name = newName;
                    db.SaveChanges();

                    // Обновляем CurrentBoardTitle (заголовок)
                    CurrentBoardTitle = newName;
                    SelectedBoard.UpdateName(newName);
                    // Обновляем название в BoardViewModel через поле _model
                    var field = typeof(BoardViewModel).GetField("_model",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var model = field?.GetValue(SelectedBoard) as Board;
                    if (model != null)
                    {
                        model.Name = newName;
                    }

                }
            }
        }
        public BoardViewModel SelectedBoard
        {
            get => _selectedBoard;
            private set
            {
                if (SetProperty(ref _selectedBoard, value) && value != null)
                {
                    CurrentBoardTitle = value.Name;
                    LoadColumns(value.Id);
                }
            }
        }

        public string CurrentBoardTitle
        {
            get => _currentBoardTitle;
            private set => SetProperty(ref _currentBoardTitle, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public string UserName => _currentUser?.FullName ?? "Неизвестный";

        public void MoveTaskToColumn(TaskViewModel task, ColumnViewModel sourceColumn, ColumnViewModel targetColumn)
        {
            if (sourceColumn == null || targetColumn == null) return;

            using var db = new KanbanContext();
            var dbTask = db.Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (dbTask == null) return;

            dbTask.ColumnId = targetColumn.Id;

            var maxOrder = db.Tasks
                .Where(t => t.ColumnId == targetColumn.Id)
                .Max(t => (int?)t.OrderInColumn) ?? 0;
            dbTask.OrderInColumn = maxOrder + 1;

            db.SaveChanges();

            sourceColumn.LoadTasks();
            targetColumn.LoadTasks();
        }

        private void LoadBoards()
        {
            IsLoading = true;
            try
            {
                using var db = new KanbanContext();
                var boardIds = db.UserBoards
                    .Where(ub => ub.UserRole.UserId == _currentUser.Id)
                    .Select(ub => ub.BoardId)
                    .ToList();

                var boards = db.Boards
                    .Where(b => boardIds.Contains(b.Id))
                    .ToList();

                _boards.Clear();
                foreach (var board in boards)
                {
                    // ПЕРЕДАЁМ MainViewModel в BoardViewModel
                    var boardVM = new BoardViewModel(board, this);
                    boardVM.LoadColumnsFromModel();
                    _boards.Add(boardVM);
                }

                if (_boards.Any())
                    SelectedBoard = _boards.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки досок: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadColumns(int boardId)
        {
            IsLoading = true;
            try
            {
                using var db = new KanbanContext();
                var columns = db.Columns
                    .Where(c => c.BoardId == boardId)
                    .OrderBy(c => c.Position)
                    .ToList();

                _currentColumns.Clear();
                foreach (var column in columns)
                {
                    // ПЕРЕДАЁМ MainViewModel в ColumnViewModel
                    var columnVM = new ColumnViewModel(column, this);
                    columnVM.LoadTasks();
                    _currentColumns.Add(columnVM);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteAddBoard(object parameter)
        {
            try
            {
                using var db = new KanbanContext();
                var userRole = db.UserRoles.FirstOrDefault(ur => ur.UserId == _currentUser.Id);

                if (userRole == null)
                {
                    MessageBox.Show("Ошибка: роль пользователя не найдена");
                    return;
                }

                var newBoard = new Board { Name = $"Доска {_boards.Count + 1}", IsPrivate = true };
                db.Boards.Add(newBoard);
                db.SaveChanges();

                db.Columns.Add(new Column { BoardId = newBoard.Id, Name = "Нужно сделать", Position = 1 });
                db.Columns.Add(new Column { BoardId = newBoard.Id, Name = "В работе", Position = 2 });
                db.Columns.Add(new Column { BoardId = newBoard.Id, Name = "Готово", Position = 3 });
                db.SaveChanges();

                db.UserBoards.Add(new UserBoard { BoardId = newBoard.Id, UserRoleId = userRole.Id });
                db.SaveChanges();

                // ПЕРЕДАЁМ MainViewModel в BoardViewModel
                var boardVM = new BoardViewModel(newBoard, this);
                boardVM.LoadColumnsFromModel();
                _boards.Add(boardVM);
                SelectedBoard = boardVM;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания доски: {ex.Message}");
            }
        }

        private void ExecuteSelectBoard(object parameter)
        {
            if (parameter is BoardViewModel board)
                SelectedBoard = board;
        }

        private void ExecuteAddColumn(object parameter)
        {
            if (SelectedBoard == null) return;

            var name = $"Колонка {SelectedBoard.Columns.Count + 1}";
            var position = SelectedBoard.Columns.Count + 1;

            using var db = new KanbanContext();
            db.Columns.Add(new Column { BoardId = SelectedBoard.Id, Name = name, Position = position });
            db.SaveChanges();
            LoadColumns(SelectedBoard.Id);
        }

        private bool CanExecuteDeleteColumn(object parameter)
        {
            return parameter is ColumnViewModel column && column.Tasks.Count == 0;
        }

        private void ExecuteDeleteColumn(object parameter)
        {
            if (parameter is not ColumnViewModel column) return;

            if (column.Tasks.Count > 0)
            {
                MessageBox.Show("Нельзя удалить колонку с задачами");
                return;
            }

            if (MessageBox.Show($"Удалить колонку \"{column.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            using var db = new KanbanContext();
            var columnToDelete = db.Columns.FirstOrDefault(c => c.Id == column.Id);
            if (columnToDelete != null)
            {
                db.Columns.Remove(columnToDelete);
                db.SaveChanges();
                LoadColumns(SelectedBoard.Id);
            }
        }

        private void ExecuteDeleteBoard(object parameter)
        {
            if (SelectedBoard == null) return;

            if (MessageBox.Show($"Удалить доску \"{SelectedBoard.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            using var db = new KanbanContext();
            var board = db.Boards.FirstOrDefault(b => b.Id == SelectedBoard.Id);
            if (board != null)
            {
                var columns = db.Columns.Where(c => c.BoardId == board.Id).ToList();
                foreach (var col in columns)
                {
                    var tasks = db.Tasks.Where(t => t.ColumnId == col.Id).ToList();
                    db.Tasks.RemoveRange(tasks);
                }
                db.Columns.RemoveRange(columns);
                db.UserBoards.RemoveRange(db.UserBoards.Where(ub => ub.BoardId == board.Id));
                db.Boards.Remove(board);
                db.SaveChanges();

                _boards.Remove(SelectedBoard);
                SelectedBoard = _boards.FirstOrDefault();
            }
        }

        private void ExecuteAddTask(object parameter)
        {
            if (parameter is not ColumnViewModel column) return;

            column.AddTask("Новая задача");
            LoadColumns(SelectedBoard.Id);
        }

        private void ExecuteDeleteTask(object parameter)
        {
            if (parameter is not TaskViewModel task) return;

            if (MessageBox.Show($"Удалить задачу \"{task.Title}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            using var db = new KanbanContext();
            var dbTask = db.Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (dbTask != null)
            {
                db.Tasks.Remove(dbTask);
                db.SaveChanges();
                LoadColumns(SelectedBoard.Id);
            }
        }

        private void ExecuteOpenTaskEdit(object parameter)
        {
            if (parameter is not TaskViewModel taskVM) return;

            using var db = new KanbanContext();
            var task = db.Tasks.FirstOrDefault(t => t.Id == taskVM.Id);
            if (task != null)
            {
                var editVM = new TaskEditViewModel(task);
                var editWindow = new TaskEditWindow(editVM);
                editWindow.Owner = Application.Current.MainWindow;
                editWindow.ShowDialog();
                LoadColumns(SelectedBoard.Id); 
            }
        }

        private void ExecuteOpenProfile(object parameter)
        {
            var profileVM = new ProfileViewModel(_currentUser);
            var profileWindow = new ProfileWindow(profileVM);
            profileWindow.Owner = Application.Current.MainWindow;
            profileWindow.ShowDialog();
        }

        private void ExecuteExit(object parameter)
        {
            Application.Current.Shutdown();
        }
    }
}