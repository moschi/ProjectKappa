using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectKappa.Base
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
        {
            if (array?.Any() == true && action != null)
            {
                foreach (T item in array)
                {
                    action(item);
                }
            }
        }

        public static System.Windows.Media.Color GetNegativeColor(this System.Windows.Media.Color color)
        {
            System.Windows.Media.Color result = new System.Windows.Media.Color();
            result.A = color.A;
            result.R = (byte)(byte.MaxValue - color.R);
            result.G = (byte)(byte.MaxValue - color.G);
            result.B = (byte)(byte.MaxValue - color.B);
            return result;
        }

        public static bool IsEmpty(this string str, bool whiteSpaceAlsoEmpty = true)
        {
            if (whiteSpaceAlsoEmpty)
            {
                return string.IsNullOrWhiteSpace(str);
            }
            else
            {
                return string.IsNullOrEmpty(str);
            }
        }
    }
}
