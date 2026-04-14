// Utils/MetrosTypeConverter.cs
using CrimeSketcher.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace CrimeSketcher.Utils
{
    /// <summary>
    /// TypeConverter que exibe valores float (armazenados em pixels)
    /// como medida em metros de acordo com a escala vigente.
    /// </summary>
    public class MetrosTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
            Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is float f)
            {
                var esc = ScaleManager.Atual;
                if (esc != null)
                {
                    float real = esc.PixelsParaReal(f);
                    return $"{real:F2} {esc.UnidadeReal}";
                }
                return $"{f:F1} px";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string s)
            {
                s = s.Trim();
                // Remove sufixo de unidade (m, cm, mm, px)
                foreach (var sufixo in new[] { "mm", "cm", "px", "m" })
                {
                    if (s.EndsWith(sufixo, StringComparison.OrdinalIgnoreCase))
                    {
                        s = s.Substring(0, s.Length - sufixo.Length).Trim();
                        break;
                    }
                }

                if (float.TryParse(s, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out float valorReal))
                {
                    var esc = ScaleManager.Atual;
                    if (esc != null)
                        return esc.RealParaPixels(valorReal);
                    return valorReal;
                }

                // Tenta com InvariantCulture
                if (float.TryParse(s, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out valorReal))
                {
                    var esc = ScaleManager.Atual;
                    if (esc != null)
                        return esc.RealParaPixels(valorReal);
                    return valorReal;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>
    /// TypeConverter que exibe PointF (em pixels) como coordenadas em metros.
    /// </summary>
    public class PosicaoMetrosConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
            Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PointF p)
            {
                var esc = ScaleManager.Atual;
                if (esc != null)
                {
                    float xm = esc.PixelsParaReal(p.X);
                    float ym = esc.PixelsParaReal(p.Y);
                    return $"{xm:F2} {esc.UnidadeReal}; {ym:F2} {esc.UnidadeReal}";
                }
                return $"{p.X:F1}; {p.Y:F1}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string s)
            {
                // Remove sufixos de unidade
                foreach (var sufixo in new[] { "mm", "cm", "px", "m" })
                {
                    s = s.Replace(sufixo, "");
                }
                s = s.Trim();

                var partes = s.Split(new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 2)
                {
                    if (float.TryParse(partes[0].Trim(), NumberStyles.Float,
                            CultureInfo.CurrentCulture, out float xReal) &&
                        float.TryParse(partes[1].Trim(), NumberStyles.Float,
                            CultureInfo.CurrentCulture, out float yReal))
                    {
                        var esc = ScaleManager.Atual;
                        if (esc != null)
                            return new PointF(esc.RealParaPixels(xReal),
                                              esc.RealParaPixels(yReal));
                        return new PointF(xReal, yReal);
                    }

                    // Tenta InvariantCulture
                    if (float.TryParse(partes[0].Trim(), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out xReal) &&
                        float.TryParse(partes[1].Trim(), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out yReal))
                    {
                        var esc = ScaleManager.Atual;
                        if (esc != null)
                            return new PointF(esc.RealParaPixels(xReal),
                                              esc.RealParaPixels(yReal));
                        return new PointF(xReal, yReal);
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>
    /// TypeConverter que exibe valores float (em pixels) como medida em metros,
    /// aplicando o fator de escala visual de trânsito quando ativo.
    /// </summary>
    public class MetrosTransitoTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string)) return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is float f)
            {
                var esc = ScaleManager.Atual;
                if (esc != null)
                    return esc.FormatarMedidaTransito(f);
                return $"{f:F1} px";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string s)
            {
                s = s.Trim();
                foreach (var sufixo in new[] { "mm", "cm", "px", "m" })
                {
                    if (s.EndsWith(sufixo, StringComparison.OrdinalIgnoreCase))
                    {
                        s = s.Substring(0, s.Length - sufixo.Length).Trim();
                        break;
                    }
                }

                float valorMetros = 0f;
                bool parsed = float.TryParse(s, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out valorMetros)
                    || float.TryParse(s, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out valorMetros);

                if (parsed)
                {
                    var esc = ScaleManager.Atual;
                    if (esc != null)
                    {
                        // O usuário editou o valor exibido (já com fator aplicado);
                        // reverter o fator para obter os metros reais antes de converter para pixels.
                        float metrosReais = valorMetros;
                        if (esc.FatorTransitoAtivo && esc.FatorTransito > 0)
                            metrosReais /= esc.FatorTransito;
                        return esc.RealParaPixels(metrosReais);
                    }
                    return valorMetros;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
