using Kanban.Models;
using Kanban.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kanban.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainWindow(User authenticatedUser)
        {
            InitializeComponent();
            DataContext = new MainViewModel(authenticatedUser);
        }

        private void Avatar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.OpenProfileCommand?.Execute(null);
        }
        private void Task_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var taskVM = border?.DataContext as TaskViewModel;
            if (taskVM != null)
            {
                // Открываем окно редактирования
                using var db = new KanbanContext();
                var task = db.Tasks.FirstOrDefault(t => t.Id == taskVM.Id);
                if (task != null)
                {
                    var editVM = new TaskEditViewModel(task);
                    var editWindow = new TaskEditWindow(editVM);
                    editWindow.Owner = this;
                    editWindow.ShowDialog();

                    // Обновляем доску после закрытия окна
                    var mainVM = DataContext as MainViewModel;
                    if (mainVM?.SelectedBoard != null)
                    {
                        mainVM.LoadColumns(mainVM.SelectedBoard.Id);
                    }
                }
            }
        }
        private void ColumnName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var column = textBlock?.DataContext as ColumnViewModel;
            if (column == null) return;

            var inputDialog = new InputDialog("Введите новое название колонки", column.Name);
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                column.UpdateName(inputDialog.Answer);
            }
        }
        private void UserName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.OpenProfileCommand?.Execute(null);
        }
    }
}