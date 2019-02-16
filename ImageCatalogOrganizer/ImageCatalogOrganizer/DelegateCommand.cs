using System;
using System.Windows.Input;

namespace ImageCatalogOrganizer
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged = (a, b) => { };

        public DelegateCommand(Action<object> execute)
            : this(execute, o => true)
        {
        }

        public DelegateCommand(Action<object> execute,
                               Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }

}
