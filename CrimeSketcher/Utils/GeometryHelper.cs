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

        public static PointF SnapAngulo(PointF origem, PointF ponto, float incrementoGraus)
        {
            float dx = ponto.X - origem.X;
            float dy = ponto.Y - origem.Y;
            float distancia = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distancia < 0.001f) return ponto;

            float passo = Math.Max(0.1f, incrementoGraus);
            float angulo = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
            float anguloSnapped = (float)(Math.Round(angulo / passo) * passo);
            float rad = anguloSnapped * (float)Math.PI / 180f;

            return new PointF(
                origem.X + distancia * (float)Math.Cos(rad),
                origem.Y + distancia * (float)Math.Sin(rad));
        }

        public static PointF SnapAngulo15(PointF origem, PointF ponto)
        {
            return SnapAngulo(origem, ponto, 15f);
        }

        public static float NormalizarAngulo360(float angulo)
        {
            float normalizado = angulo % 360f;
            if (normalizado < 0f)
                normalizado += 360f;

            if (Math.Abs(normalizado) < 0.0001f || Math.Abs(normalizado - 360f) < 0.0001f)
                return 0f;

            return normalizado;
        }
    }
}