// Core/ScaleManager.cs
using System;
using System.Drawing;

namespace CrimeSketcher.Core
{
    /// <summary>
    /// Gerencia escala do desenho (ex: 1:100 = 1cm no papel = 1m real)
    /// </summary>
    public class ScaleManager
    {
        public float EscalaNumerador { get; set; } = 1f;
        public float EscalaDenominador { get; set; } = 100f;
        public float PixelsPorCentimetro { get; set; } = 37.8f; // ~96 DPI
        public string UnidadeReal { get; set; } = "m";

        /// <summary>
        /// Fator de escala: quantos pixels representam 1 unidade real
        /// </summary>
        public float PixelsPorUnidadeReal =>
            PixelsPorCentimetro * (EscalaNumerador / EscalaDenominador) * 100f;

        /// <summary>
        /// Converte distância em pixels para medida real
        /// </summary>
        public float PixelsParaReal(float pixels)
        {
            if (PixelsPorUnidadeReal == 0) return 0;
            return pixels / PixelsPorUnidadeReal;
        }

        /// <summary>
        /// Converte medida real para pixels
        /// </summary>
        public float RealParaPixels(float real)
        {
            return real * PixelsPorUnidadeReal;
        }

        /// <summary>
        /// Texto formatado da medida
        /// </summary>
        public string FormatarMedida(float pixels)
        {
            float real = PixelsParaReal(pixels);
            if (real >= 1f)
                return $"{real:F2} {UnidadeReal}";
            else
                return $"{real * 100f:F1} cm";
        }

        /// <summary>
        /// Texto da escala atual
        /// </summary>
        public string TextoEscala =>
            $"1:{EscalaDenominador / EscalaNumerador:F0}";

        public float ZoomLevel { get; set; } = 1.0f;

        public float PixelsEfetivos => PixelsPorUnidadeReal * ZoomLevel;
    }
}