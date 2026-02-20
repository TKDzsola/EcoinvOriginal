using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Ecoinv.Components
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }
    }


    public sealed class IN2Bool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value != null) && (value is string))
                return (string)value == "I" || (string)value == "i";

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value != null) && (value is bool))
                return (bool)value ? "I" : "N";

            return "N";
        }
    }


    public sealed class NUllaEgy2Bool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value != null) && (value is int))
                return (int)value == 1;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value != null) && (value is bool))
                return (bool)value ? 1 : 0;

            return 0;
        }
    }


    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public sealed class EnumToCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var aaa = value.GetType();
            return EnumHelper.GetAllValuesAndDescriptions(aaa);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public sealed class CollectionConverterToEnum : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retvalue = value.ToString();
            if (parameter is Type)
            {
                var enumToDescription = (Enum)Enum.Parse((Type)parameter, value.ToString());
                retvalue = EnumHelper.GetDescription(enumToDescription);
            }

            return retvalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    [System.Diagnostics.DebuggerStepThrough]
    public sealed class EQ : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value == null;

            if (value == null)
                return false;

            var parameter_value = parameter.ToString();
            var string_value = value.ToString();

            return string.Compare(string_value, parameter_value, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var default_value = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (!(value is bool) || parameter == null || !(bool)value)
                return default_value;

            return targetType.IsEnum
              ? Enum.Parse(targetType, parameter.ToString(), true)
              : Convert(parameter, targetType);
        }


        private static object Convert(object value, Type targetType, CultureInfo culture = null) => ConvertTo(value, targetType, culture);

        private static object ConvertTo(object value, Type targetType, CultureInfo culture = null)
        {
            if (IsPrimitiveConvert(value, targetType, out var result))
                return result;
            else
            {
                return true;
            }
        }

        private static bool IsPrimitiveConvert(object value, Type targetType, out object result)
        {
            result = null;
            if (targetType == typeof(object))
            {
                result = value;
                return true;
            }

            if (value == null)
            {
                result = GetDefaultValue(targetType);
                return true;
            }

            if (!targetType.IsInstanceOfType(value))
                return false;

            result = value;
            return true;
        }

        private static object GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;


        private static bool IsGenericNullable(Type type) =>
          type is { IsGenericType: true } &&
          type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

        private static Type GetUnderlyingType(Type type) => type == null ? null : Nullable.GetUnderlyingType(type);
    }

    // ÚJ: Erika kérése - Törtszámok rugalmas kezelése (pont/vessző csere)
    public sealed class FlexDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0m;

            string input = value.ToString();
            if (string.IsNullOrWhiteSpace(input)) return 0m;

            // Megnézzük mi a rendszer tizedesjele
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // Kicseréljük az elválasztót a rendszernek megfelelőre (pont -> vessző vagy fordítva)
            if (decimalSeparator == ",")
                input = input.Replace(".", ",");
            else
                input = input.Replace(",", ".");

            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal result))
                return result;

            return 0m;
        }
    }
}