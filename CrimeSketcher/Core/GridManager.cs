// Core/GridManager.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Core
{
    public class GridManager
    {
        public bool Visivel { get; set; } = true;
        public bool SnapAtivo { get; set; } = true;
        public float EspacamentoPixels { get; set; } = 20f;
        public Color CorGrade { get; set; } = Color.FromArgb(40, 100, 100, 100);
        public Color CorGradePrincipal { get; set; } = Color.FromArgb(60, 80, 80, 80);
        public int SubdivisoesPrincipais { get; set; } = 5;

        private ScaleManager _scale;

        public GridManager(ScaleManager scale)
        {
            _scale = scale;
        }

        /// <summary>
        /// Ajusta ponto ao grid mais próximo
        /// </summary>
        public PointF Snap(PointF ponto)
        {
            if (!SnapAtivo) return ponto;

            float spacing = EspacamentoPixels * _scale.ZoomLevel;
            float x = (float)Math.Round(ponto.X / spacing) * spacing;
            float y = (float)Math.Round(ponto.Y / spacing) * spacing;
            return new PointF(x, y);
        }

        public PointF SnapToPoint(PointF ponto, PointF[] pontosExistentes,
            float tolerancia = 10f)
        {
            // Primeiro tenta snap a pontos existentes
            foreach (var p in pontosExistentes)
            {
                float dist = (float)Math.Sqrt(
                    Math.Pow(ponto.X - p.X, 2) + Math.Pow(ponto.Y - p.Y, 2));
                if (dist < tolerancia)
                    return p;
            }
            // Senão, snap ao grid
            return Snap(ponto);
        }

        /// <summary>
        /// Desenha a grade no canvas
        /// </summary>
        public void Desenhar(Graphics g, RectangleF areVisivel)
        {
            if (!Visivel) return;

            float spacing = EspacamentoPixels * _scale.ZoomLevel;
            if (spacing < 5f) spacing *= SubdivisoesPrincipais;

            using (var penFina = new Pen(CorGrade, 0.5f))
            using (var penGrossa = new Pen(CorGradePrincipal, 1f))
            {
                penFina.DashStyle = DashStyle.Dot;

                int startX = (int)(areVisivel.Left / spacing) - 1;
                int endX = (int)(areVisivel.Right / spacing) + 1;
                int startY = (int)(areVisivel.Top / spacing) - 1;
                int endY = (int)(areVisivel.Bottom / spacing) + 1;

                for (int i = startX; i <= endX; i++)
                {
                    float x = i * spacing;
                    bool principal = (i % SubdivisoesPrincipais == 0);
                    g.DrawLine(principal ? penGrossa : penFina,
                        x, areVisivel.Top, x, areVisivel.Bottom);
                }

                for (int j = startY; j <= endY; j++)
                {
                    float y = j * spacing;
                    bool principal = (j % SubdivisoesPrincipais == 0);
                    g.DrawLine(principal ? penGrossa : penFina,
                        areVisivel.Left, y, areVisivel.Right, y);
                }
            }
        }
    }
}