using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectKappa.Base.WPF
{
    public class BasePropertyChanged
        : INotifyPropertyChanged
    {
        private bool saveOldValues = false;

        private Dictionary<string, object> newPropertyValues = new Dictionary<string, object>();
        private Dictionary<string, object> oldPropertyValues = new Dictionary<string, object>();

        public virtual BasePropertyChanged GetSimpleCopy()
        {
            BasePropertyChanged result = Activator.CreateInstance(this.GetType()) as BasePropertyChanged;
            result.saveOldValues = this.saveOldValues;
            result.newPropertyValues = new Dictionary<string, object>(this.newPropertyValues);
            result.oldPropertyValues = new Dictionary<string, object>(this.oldPropertyValues);
            return result;
        }

        protected virtual void SetSimpleCopy(BasePropertyChanged source)
        {
            this.saveOldValues = source.saveOldValues;
            this.newPropertyValues = new Dictionary<string, object>(source.newPropertyValues);
            this.oldPropertyValues = new Dictionary<string, object>(source.oldPropertyValues);
        }

        protected void AllPropertyChanged()
        {
            newPropertyValues.Keys.ForEach(x => this.OnPropertyChanged(x));
        }

        public void IgnoreOldValues()
        {
            if (this.saveOldValues)
            {
                this.saveOldValues = false;
                this.oldPropertyValues.Clear();
            }
        }

        public void SaveOldValues()
        {
            if (!this.saveOldValues)
            {
                this.saveOldValues = true;

                this.oldPropertyValues.Clear();

                foreach (KeyValuePair<string, object> item in this.newPropertyValues)
                {
                    this.oldPropertyValues.Add(item.Key, item.Value);
                }
            }
        }

        protected bool SetValue<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (this.newPropertyValues.ContainsKey(propertyName))
            {
                T oldValue = (T)this.newPropertyValues[propertyName];

                if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
                {
                    return false;
                }

                this.newPropertyValues[propertyName] = newValue;
                this.SetOldValue(oldValue, propertyName);
            }
            else
            {
                this.newPropertyValues.Add(propertyName, newValue);
                this.SetOldValue(newValue, propertyName);
            }

            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void SetOldValue<T>(T oldValue, string propertyName)
        {
            if (this.saveOldValues)
            {
                if (this.oldPropertyValues.ContainsKey(propertyName))
                {
                    this.oldPropertyValues[propertyName] = oldValue;
                }
                else
                {
                    this.oldPropertyValues.Add(propertyName, oldValue);
                }
            }
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null) where T : class => GetValueInternal<T>(this.newPropertyValues, propertyName);
        protected T GetOldValue<T>(string propertyName) where T : class => GetValueInternal<T>(this.oldPropertyValues, propertyName);

        private static T GetValueInternal<T>(Dictionary<string, object> propertyValues, string propertyName)
           where T : class
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                return (T)propertyValues[propertyName];
            }

            return null;
        }

        protected T GetValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
            where T : class
        {
            T val = GetValue<T>(propertyName);

            return val ?? defaultValue;
        }

        protected T? GetStructValue<T>([CallerMemberName] string propertyName = null) where T : struct => GetStructValueInternal<T>(this.newPropertyValues, propertyName);
        protected T? GetOldStructValue<T>(string propertyName) where T : struct => GetStructValueInternal<T>(this.oldPropertyValues, propertyName);

        private static T? GetStructValueInternal<T>(Dictionary<string, object> propertyValues, string propertyName)
            where T : struct
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                return (T?)propertyValues[propertyName];
            }

            return new T?();
        }

        protected T GetStructOrDefaultValue<T>(T defaultValue = default(T), [CallerMemberName] string propertyName = null)
            where T : struct
        {
            T? val = GetStructValue<T>(propertyName);

            return val ?? defaultValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
