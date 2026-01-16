using System;
using System.Globalization;
using System.Windows.Data;
using Ecoinv.BL;

namespace Ecoinv.Components
{
  public class StatusToTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return "Ismeretlen";

      // Ha nem int → nem konvertálható → visszaadja az eredetit
      if (!int.TryParse(value.ToString(), out int status))
        return value.ToString();

      return status switch
      {
        (int)InvoiceStatus.InProgress => "Készítés alatt",
        (int)InvoiceStatus.Normal => "Kész",
        (int)InvoiceStatus.Storno => "Stornózott",
        _ => "Ismeretlen"
      };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null; // nincs visszakonverzió
    }
  }
}
