using System;
using System.ComponentModel;
using System.Globalization;

namespace CrimeSketcher.Utils
{
    public class SimNaoConverter : BooleanConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is bool boolValue)
            {
                return boolValue ? "Sim" : "Não";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                if (string.Equals(stringValue, "Sim", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (string.Equals(stringValue, "Não", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(stringValue, "Nao", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}