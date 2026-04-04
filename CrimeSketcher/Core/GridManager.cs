// Core/GridManager.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CrimeSketcher.Core
{
    public class GridManager
    {
        public bool Visivel { get; set; } = true;
        public bool SnapAtivo { get; set; } = true;
        public float EspacamentoPixels { get; set; } = 10f;
        public Color CorGrade { get; set; } = Color.FromArgb(40, 100, 100, 100);
        public Color CorGradePrincipal { get; set; } = Color.FromArgb(60, 80, 80, 80);
        public int SubdivisoesPrincipais { get; set; } = 5;

        private ScaleManager _scale;

        // Cache do tile para renderização da grade via TextureBrush
        private Bitmap _tileCache;
        private TextureBrush _brushCache;
        private float _cachedSpacing;
        private int _cachedCorGrade;
        private int _cachedCorPrincipal;
        private int _cachedSubdivisoes;
        private const int TILE_RESOLUTION = 128;

        public GridManager(ScaleManager scale)
        {
            _scale = scale;
        }

        /// <summary>
        /// Descarta o cache do tile, forçando regeneração no próximo desenho.
        /// </summary>
        public void InvalidarCache()
        {
            _brushCache?.Dispose();
            _brushCache = null;
            _tileCache?.Dispose();
            _tileCache = null;
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
        /// Desenha a grade no canvas usando um tile pré-renderizado com TextureBrush.
        /// Reduz centenas de chamadas DrawLine para um único FillRectangle.
        /// O tile é cacheado e só regenerado quando zoom ou configurações mudam.
        /// </summary>
        public void Desenhar(Graphics g, RectangleF areVisivel)
        {
            if (!Visivel) return;

            float spacing = EspacamentoPixels * _scale.ZoomLevel;
            if (spacing < 5f) spacing *= SubdivisoesPrincipais;

            RegenerarTileSeNecessario(spacing);

            if (_brushCache == null) return;

            var prevSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;

            try
            {
                g.FillRectangle(_brushCache, areVisivel);
            }
            finally
            {
                g.SmoothingMode = prevSmoothing;
            }
        }

        /// <summary>
        /// Regenera o tile bitmap e o TextureBrush caso os parâmetros tenham mudado.
        /// O tile representa um período completo da grade (SubdivisoesPrincipais células).
        /// </summary>
        private void RegenerarTileSeNecessario(float spacing)
        {
            int corGrade = CorGrade.ToArgb();
            int corPrincipal = CorGradePrincipal.ToArgb();
            int sub = Math.Max(1, SubdivisoesPrincipais);

            if (_tileCache != null
                && _cachedSpacing == spacing
                && _cachedCorGrade == corGrade
                && _cachedCorPrincipal == corPrincipal
                && _cachedSubdivisoes == sub)
            {
                return;
            }

            _brushCache?.Dispose();
            _brushCache = null;
            _tileCache?.Dispose();

            _tileCache = new Bitmap(TILE_RESOLUTION, TILE_RESOLUTION, PixelFormat.Format32bppPArgb);

            float tileWorldSize = spacing * sub;
            float invScale = TILE_RESOLUTION / tileWorldSize;
            float cellPx = (float)TILE_RESOLUTION / sub;

            using (var tg = Graphics.FromImage(_tileCache))
            {
                tg.Clear(Color.Transparent);
                tg.SmoothingMode = SmoothingMode.None;

                // Linhas menores (sólidas — substituem DashStyle.Dot que era ~5x mais caro)
                float penMinorW = Math.Max(1f, 0.5f * invScale);
                using var penFina = new Pen(CorGrade, penMinorW);
                for (int i = 1; i < sub; i++)
                {
                    float pos = i * cellPx;
                    tg.DrawLine(penFina, pos, 0, pos, TILE_RESOLUTION);
                    tg.DrawLine(penFina, 0, pos, TILE_RESOLUTION, pos);
                }

                // Linhas principais (mais espessas)
                float penMajorW = Math.Max(1f, 1f * invScale);
                using var penGrossa = new Pen(CorGradePrincipal, penMajorW);
                tg.DrawLine(penGrossa, 0, 0, 0, TILE_RESOLUTION);
                tg.DrawLine(penGrossa, 0, 0, TILE_RESOLUTION, 0);
            }

            float scale = tileWorldSize / TILE_RESOLUTION;
            _brushCache = new TextureBrush(_tileCache, WrapMode.Tile);
            _brushCache.ScaleTransform(scale, scale);

            _cachedSpacing = spacing;
            _cachedCorGrade = corGrade;
            _cachedCorPrincipal = corPrincipal;
            _cachedSubdivisoes = sub;
        }
    }
}