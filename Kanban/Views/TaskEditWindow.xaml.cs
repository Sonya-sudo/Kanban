using System.Windows;
using Kanban.ViewModels;

namespace Kanban.Views
{
    public partial class TaskEditWindow : Window
    {
        public TaskEditWindow(TaskEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.SetWindow(this);
        }
    }
}