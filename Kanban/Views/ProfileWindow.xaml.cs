using System.Windows;
using Kanban.ViewModels;

namespace Kanban.Views
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(ProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.SetWindow(this);
        }
    }
}