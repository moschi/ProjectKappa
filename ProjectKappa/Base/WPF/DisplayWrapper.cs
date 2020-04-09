using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectKappa.Base.WPF
{
    public class DisplayWrapper<T>
        : DataWrapper<T>
    {
        public DisplayWrapper(T item, string displayValue, bool saveOldValues = false)
            : base(item, saveOldValues)
        {
            this.DisplayValue = displayValue;
            this.TooltipValue = displayValue;
        }

        #region Binding

        public string DisplayValue
        {
            get => this.GetValue<string>();
            set => this.SetValue(value);
        }

        public string TooltipValue
        {
            get => this.GetValue<string>();
            set => this.SetValue(value);
        }

        public Brush Color
        {
            get => this.GetValueOrDefault(Brushes.White);
            set
            {
                this.SetValue(value);
                this.OnPropertyChanged(nameof(this.NegativeColor));
            }
        }

        public Brush NegativeColor
        {
            get
            {
                SolidColorBrush brush = this.Color as SolidColorBrush;
                return new SolidColorBrush(brush.Color.GetNegativeColor());
            }
        }

        public bool IsVisible
        {
            get => this.GetStructOrDefaultValue(true);
            set => this.SetValue(value);
        }

        #endregion Binding

        public override string ToString() => this.DisplayValue;
    }
}
