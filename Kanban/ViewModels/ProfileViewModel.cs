using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;

namespace Kanban.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly User _user;
        private string _fullName;
        private string _email;
        private Window _window;

        public ProfileViewModel(User user)
        {
            _user = user;
            _fullName = user.FullName;
            _email = user.Email;

            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email => _email;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public void SetWindow(Window window) => _window = window;

        private void ExecuteSave(object parameter)
        {
            using var db = new KanbanContext();
            var user = db.Users.FirstOrDefault(u => u.Id == _user.Id);
            if (user != null)
            {
                user.FullName = FullName;
                db.SaveChanges();

                _user.FullName = FullName;
            }

            MessageBox.Show("Профиль обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            _window.DialogResult = true;
            _window.Close();
        }

        private void ExecuteCancel(object parameter)
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}