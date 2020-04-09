using ProjectKappa.Base.WPF;
using System;

namespace ProjectKappa.ViewModels
{
    public class CliTask : BasePropertyChanged
    {
        public CliTask(string name, Action execute)
        {
            Name = name;
            Execute = execute;
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public Action Execute
        {
            get => GetValue<Action>();
            set => SetValue(value);
        }

        public bool Finished
        {
            get => GetStructOrDefaultValue<bool>();
            set => SetValue(value);
        }
    }
}
