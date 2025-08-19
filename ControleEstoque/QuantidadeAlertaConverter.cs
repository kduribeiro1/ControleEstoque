using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ControleEstoque
{
    public class QuantidadeAlertaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                if (!double.TryParse(values[0]?.ToString(), out double total))
                {
                    total = 0; // Se não for possível converter, assume 0
                }
                if (!double.TryParse(values[1]?.ToString(), out double minimo))
                {
                    minimo = 0; // Se não for possível converter, assume 0
                }
                if (total <= 0)
                {
                    return 2;
                }
                if (minimo > 0)
                {
                    if (total <= minimo)
                    {
                        return 2;
                    }
                    else
                    {
                        double alerta = Math.Ceiling((double)minimo * (double)1.15);
                        if (total <= alerta)
                        {
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
