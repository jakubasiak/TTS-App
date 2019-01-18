using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TTS.Command
{
    public class RelayCommand : ICommand
    {

        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException("execute");
            this.canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute?.Invoke(parameter) ?? true;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this.canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (this.canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

    }
}
