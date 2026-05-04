using System.Windows;

namespace Kanban.Services
{
    public interface IWindowFactory
    {
        Window GetWindow<TViewModel>() where TViewModel : class;
        void Register<TViewModel, TWindow>() where TViewModel : class where TWindow : Window, new();
    }
}