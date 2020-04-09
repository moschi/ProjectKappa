using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectKappa.Base.WPF
{
    public class DataWrapper<T>
        : BasePropertyChanged
    {
        private T item;

        public DataWrapper(T item, bool saveOldValues = false)
        {
            if (saveOldValues)
            {
                this.SaveOldValues();
            }

            this.Item = item;
        }

        public T Item
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
                this.OnPropertyChanged(nameof(this.Item));
            }
        }
    }
}
