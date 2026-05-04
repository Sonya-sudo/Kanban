using System.Windows;
using Kanban.ViewModels;

namespace Kanban.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            viewModel.SetWindow(this);
            DataContext = viewModel;
        }
    }
}