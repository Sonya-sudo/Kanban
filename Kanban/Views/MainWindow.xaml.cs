using System.Windows;
using Kanban.Models;
using Kanban.ViewModels;

namespace Kanban.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(User authenticatedUser)
        {
            InitializeComponent();
            DataContext = new MainViewModel(authenticatedUser);
        }
    }
}