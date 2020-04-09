using System;
using System.Windows.Input;

namespace ProjectKappa.Base.WPF
{
    public class BaseCommand : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;

        public BaseCommand(Predicate<object> canExecute, Action<object> execute)
        {
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public BaseCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._execute(parameter);
        }
    }
}
