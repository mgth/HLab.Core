﻿using System;
using System.ComponentModel;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows.Input;
using HLab.Base;
using HLab.Notify.PropertyChanged.PropertyHelpers;

namespace HLab.Notify.PropertyChanged
{
    public interface ITrigger :IChildObject {}

    public class NotifyTrigger : ChildObject, ITrigger
    {
        public NotifyTrigger(ConfiguratorEntry configurator) : base(configurator)
        {
        }
    }

    public class NotifyCommand : ChildObject, ICommand
    {
        private Action<object> _execute = null;
        private Func<object,Task> _executeAsync = null;
        private Func<object,bool> _canExecuteFunc = null;
        private bool _canExecute = true;


        public void SetAction(Action execute) => _execute = o => execute();

        public void SetAction(Func<Task> execute) => _executeAsync = async  o =>
        {
                _executing = true;
                CanExecuteChanged?.Invoke(this,new EventArgs());
                await execute().ConfigureAwait(true);
                _executing = false;
                CanExecuteChanged?.Invoke(this,new EventArgs());
        };
        //{
        //        _executing = true;
        //        NotifyHelper.EventHandlerService.Invoke(CanExecuteChanged,this,new EventArgs());
        //        await NotifyHelper.EventHandlerService.InvokeAsync(execute);
        //        _executing = false;
        //        NotifyHelper.EventHandlerService.Invoke(CanExecuteChanged,this,new EventArgs());
        //};

        private volatile bool _executing = false;

        public NotifyCommand(ConfiguratorEntry configurator):base(configurator) {  }

        public void SetAction(Action<object> execute) => _execute = execute;
        public void SetCanExecute(Func<bool> func)
        {
            _canExecuteFunc = o => func();
        }
        public void SetCanExecute(Func<object,bool> func)
        {
            _canExecuteFunc = func;
        }

        public bool SetCanExecute(bool value)
        {
            if (_canExecute == value) return value;

            _canExecute = value;

            NotifyHelper.EventHandlerService.Invoke(CanExecuteChanged,this,new EventArgs());

            return value;
        }

        public bool SetCanExecute()
        {
            return SetCanExecute(CanExecute());
        }

        public bool CanExecute(object parameter=null)
        {
            if (_executing) return false;

            if (_canExecuteFunc == null) return _canExecute;
            return _canExecuteFunc(parameter);
        }

        async void ICommand.Execute(object parameter)
        {
            if (_executeAsync != null)
                await _executeAsync(parameter).ConfigureAwait(true);
            else
                _execute?.Invoke(parameter);
        }



        //public async void Execute(object parameter=null)
        //{
        //    if (_executeAsync != null)
        //        //await _executeAsync(parameter).ConfigureAwait(true);
        //        await _executeAsync(parameter);
        //    else
        //        _execute?.Invoke(parameter);
        //}

        public event EventHandler CanExecuteChanged;
    }

}