
using Kanban.Models;
using Kanban.ViewModels.Base;
using Kanban.ViewModels.Commands;
using Kanban.Views;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Kanban.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email;
        private string _errorMessage;
        private Window _currentWindow;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public void SetWindow(Window window) => _currentWindow = window;

        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Введите email и пароль";
                return;
            }

            using var db = new KanbanContext();
            var user = db.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ErrorMessage = "Неверный email или пароль";
                return;
            }

            var mainWindow = new MainWindow(user);
            mainWindow.Show();

            // Вместо _currentWindow?.Close()
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private void ExecuteRegister(object parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Введите email и пароль";
                return;
            }

            if (password.Length < 6)
            {
                ErrorMessage = "Пароль должен быть не менее 6 символов";
                return;
            }

            using var db = new KanbanContext();

            if (db.Users.Any(u => u.Email == Email))
            {
                ErrorMessage = "Пользователь уже существует";
                return;
            }

            var user = new User
            {
                Email = Email,
                PasswordHash = HashPassword(password),
                FullName = Email.Split('@')[0],
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            var role = db.Roles.FirstOrDefault(r => r.Name == "owner");
            if (role == null)
            {
                role = new Role { Name = "owner" };
                db.Roles.Add(role);
                db.SaveChanges();
            }

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            db.UserRoles.Add(userRole);
            db.SaveChanges();

            MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            var mainWindow = new MainWindow(user);
            mainWindow.Show();

            // Вместо _currentWindow?.Close()
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}