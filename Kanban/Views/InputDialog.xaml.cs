using System.Windows;

namespace Kanban.Views
{
    public partial class InputDialog : Window
    {
        public string Answer { get; set; }

        public InputDialog(string prompt, string defaultValue = "")
        {
            InitializeComponent();
            Title = prompt;
            Answer = defaultValue;
            DataContext = this;
            AnswerBox.Focus();
            AnswerBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}