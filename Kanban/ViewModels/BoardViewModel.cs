using System.Collections.ObjectModel;
using System.Linq;
using Kanban.Models;
using Kanban.ViewModels.Base;

namespace Kanban.ViewModels
{
    public class BoardViewModel : ViewModelBase
    {
        private readonly Board _model;
        private readonly MainViewModel _mainViewModel;
        private bool _isSelected;
        private readonly ObservableCollection<ColumnViewModel> _columns;
        private readonly ReadOnlyObservableCollection<ColumnViewModel> _columnsReadOnly;

        public BoardViewModel(Board model, MainViewModel mainViewModel)
        {
            _model = model;
            _mainViewModel = mainViewModel;
            _columns = new ObservableCollection<ColumnViewModel>();
            _columnsReadOnly = new ReadOnlyObservableCollection<ColumnViewModel>(_columns);
        }

        public int Id => _model.Id;
        public string Name => _model.Name;
        public bool IsPrivate => _model.IsPrivate ?? true;


        public ReadOnlyObservableCollection<ColumnViewModel> Columns => _columnsReadOnly;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public void UpdateName(string newName)
        {
            _model.Name = newName;
            OnPropertyChanged(nameof(Name));
        }

        public void LoadColumnsFromModel()
        {
            _columns.Clear();
            using var db = new KanbanContext();
            var columns = db.Columns
                .Where(c => c.BoardId == _model.Id)
                .OrderBy(c => c.Position)
                .ToList();

            foreach (var column in columns)
            {
                // ПЕРЕДАЁМ MainViewModel в ColumnViewModel
                _columns.Add(new ColumnViewModel(column, _mainViewModel));
            }
        }
    }
}