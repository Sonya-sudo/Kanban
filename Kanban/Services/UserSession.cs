using Kanban.Models;

namespace Kanban.Services
{
    public class UserSession
    {
        public User CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        public void Login(User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}