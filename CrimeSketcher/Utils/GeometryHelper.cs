// Utils/GeometryHelper.cs
using System;
using System.Drawing;

namespace CrimeSketcher.Utils
{
    public static class GeometryHelper
    {
        /// <summary>
        /// Calcula a distância de um ponto a um segmento de reta
        /// </summary>
        public static float DistanciaPontoSegmento(PointF ponto,
            PointF segA, PointF segB)
        {
            float dx = segB.X - segA.X;
            float dy = segB.Y - segA.Y;
            float comprimento2 = dx * dx + dy * dy;

            if (comprimento2 == 0)
                return Distancia(ponto, segA);

            float t = Math.Max(0, Math.Min(1,
                ((ponto.X - segA.X) * dx + (ponto.Y - segA.Y) * dy)
                / comprimento2));

            var proj = new PointF(segA.X + t * dx, segA.Y + t * dy);
            return Distancia(ponto, proj);
        }

        public static float Distancia(PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float AnguloGraus(PointF origem, PointF destino)
        {
            return (float)(Math.Atan2(
                destino.Y - origem.Y,
                destino.X - origem.X) * 180 / Math.PI);
        }

        public static PointF PontoMedio(PointF a, PointF b)
        {
            return new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
        }

        public static PointF RotacionarPonto(PointF ponto,
            PointF centro, float anguloGraus)
        {
            float rad = anguloGraus * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float dx = ponto.X - centro.X;
            float dy = ponto.Y - centro.Y;

            return new PointF(
                centro.X + dx * cos - dy * sin,
                centro.Y + dx * sin + dy * cos);
        }
    }
}