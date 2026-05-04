using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Kanban.Services
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _viewModelToWindowMap = new();

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Register<TViewModel, TWindow>()
            where TViewModel : class
            where TWindow : Window, new()
        {
            _viewModelToWindowMap[typeof(TViewModel)] = typeof(TWindow);
        }

        public Window GetWindow<TViewModel>() where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            var windowType = _viewModelToWindowMap[typeof(TViewModel)];
            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;
            return window;
        }
    }
}