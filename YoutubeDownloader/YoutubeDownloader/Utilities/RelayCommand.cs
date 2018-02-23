using System;
using System.Windows.Input;

namespace YoutubeDownloader
{
    public class RelayCommand<TParam> : ICommand
    {
        private Action<TParam> _execute;
        private Func<TParam, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<TParam> execute, Func<TParam, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        protected RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = (obj) => execute();
            _canExecute = (obj) => canExecute();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || parameter == null || _canExecute((TParam)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((TParam)parameter);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null) : base(execute, canExecute) { }
    }
}
