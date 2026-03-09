// Core/SketchCanvas.cs
using CrimeSketcher.Library;
using CrimeSketcher.Objects;
using CrimeSketcher.Tools;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CrimeSketcher.Core
{
    public class SketchCanvas : Panel
    {
        private SketchDocument _documento;
        private ScaleManager _escala;
        private GridManager _grid;
        private ITool _ferramentaAtual;

        // Pan/Zoom
        private PointF _panOffset = new PointF(0, 0);
        private bool _panning = false;
        private Point _panStart;
        private PointF _panOffsetStart;

        // Cursor info
        private PointF _cursorWorld;

        public SketchDocument Documento
        {
            get => _documento;
            set
            {
                _documento = value;
                _documento.DocumentoAlterado += (s, e) => Invalidate();
                Invalidate();
            }
        }

        public ScaleManager Escala
        {
            get => _escala;
            set => _escala = value;
        }

        public GridManager Grid
        {
            get => _grid;
            set => _grid = value;
        }

        public ITool FerramentaAtual
        {
            get => _ferramentaAtual;
            set
            {
                _ferramentaAtual?.Cancelar();
                _ferramentaAtual = value;
                this.Cursor = value?.Cursor ?? Cursors.Default;
                Invalidate();
            }
        }

        public PointF CursorMundo => _cursorWorld;

        public event EventHandler<PointF> CursorMoved;
        public event EventHandler<float> ZoomChanged;

        public SketchCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.White;

            _escala = new ScaleManager();
            _grid = new GridManager(_escala);
        }

        /// <summary>
        /// Converte coordenada de tela para coordenada do mundo
        /// </summary>
        public PointF ScreenToWorld(Point screen)
        {
            float x = (screen.X - _panOffset.X) / _escala.ZoomLevel;
            float y = (screen.Y - _panOffset.Y) / _escala.ZoomLevel;
            return new PointF(x, y);
        }

        /// <summary>
        /// Converte coordenada do mundo para tela
        /// </summary>
        public PointF WorldToScreen(PointF world)
        {
            float x = world.X * _escala.ZoomLevel + _panOffset.X;
            float y = world.Y * _escala.ZoomLevel + _panOffset.Y;
            return new PointF(x, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fundo
            g.Clear(Color.FromArgb(245, 245, 240));

            // Aplicar transformação (pan + zoom)
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_escala.ZoomLevel, _escala.ZoomLevel);

            // Área visível no mundo
            var topLeft = ScreenToWorld(Point.Empty);
            var bottomRight = ScreenToWorld(
                new Point(Width, Height));
            var areaVisivel = new RectangleF(
                topLeft.X, topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y);

            // Grade
            _grid.Desenhar(g, areaVisivel);

            // Objetos do documento
            if (_documento != null)
            {
                foreach (var obj in _documento.Objetos)
                {
                    if (obj.Visivel)
                    {
                        obj.Desenhar(g);
                    }
                }
            }

            // Preview da ferramenta atual
            _ferramentaAtual?.Desenhar(g);

            // Resetar transform para desenhar HUD
            g.ResetTransform();

            // HUD - Informações na tela
            DesenharHUD(g);
        }

        private void DesenharHUD(Graphics g)
        {
            // Escala no canto inferior esquerdo
            using (var font = new Font("Segoe UI", 9f))
            using (var bgBrush = new SolidBrush(
                Color.FromArgb(200, 40, 40, 40)))
            using (var textBrush = new SolidBrush(Color.White))
            {
                string info = $"Escala: {_escala.TextoEscala}  |  " +
                    $"Zoom: {_escala.ZoomLevel * 100:F0}%  |  " +
                    $"Pos: ({_cursorWorld.X:F0}, {_cursorWorld.Y:F0})";

                if (_grid.SnapAtivo)
                    info += "  |  SNAP";

                var size = g.MeasureString(info, font);
                var rect = new RectangleF(
                    5, Height - size.Height - 8,
                    size.Width + 10, size.Height + 6);

                g.FillRoundedRectangle(bgBrush,
                    rect.X, rect.Y, rect.Width, rect.Height, 3);
                g.DrawString(info, font, textBrush, rect.X + 5, rect.Y + 3);
            }

            // Régua horizontal
            DesenharRegua(g, true);
            DesenharRegua(g, false);
        }

        private void DesenharRegua(Graphics g, bool horizontal)
        {
            float rulerSize = 20f;
            using (var bgBrush = new SolidBrush(
                Color.FromArgb(230, 250, 250, 245)))
            using (var pen = new Pen(Color.Gray, 0.5f))
            using (var font = new Font("Segoe UI", 6f))
            {
                if (horizontal)
                {
                    g.FillRectangle(bgBrush, 0, 0, Width, rulerSize);
                    g.DrawLine(pen, 0, rulerSize, Width, rulerSize);

                    float spacing = _grid.EspacamentoPixels * _escala.ZoomLevel;
                    float start = _panOffset.X % spacing;

                    for (float x = start; x < Width; x += spacing)
                    {
                        float worldX = (x - _panOffset.X) / _escala.ZoomLevel;
                        g.DrawLine(pen, x, rulerSize - 5, x, rulerSize);

                        if (Math.Abs(worldX % (_grid.EspacamentoPixels *
                            _grid.SubdivisoesPrincipais)) < 1)
                        {
                            g.DrawLine(Pens.DimGray, x, 0, x, rulerSize);
                            g.DrawString($"{worldX:F0}", font,
                                Brushes.DimGray, x + 2, 2);
                        }
                    }
                }
                else
                {
                    g.FillRectangle(bgBrush, 0, 0, rulerSize, Height);
                    g.DrawLine(pen, rulerSize, 0, rulerSize, Height);

                    float spacing = _grid.EspacamentoPixels * _escala.ZoomLevel;
                    float start = _panOffset.Y % spacing;

                    for (float y = start; y < Height; y += spacing)
                    {
                        float worldY = (y - _panOffset.Y) / _escala.ZoomLevel;
                        g.DrawLine(pen, rulerSize - 5, y, rulerSize, y);

                        if (Math.Abs(worldY % (_grid.EspacamentoPixels *
                            _grid.SubdivisoesPrincipais)) < 1)
                        {
                            g.DrawLine(Pens.DimGray, 0, y, rulerSize, y);
                            var state = g.Save();
                            g.TranslateTransform(2, y + 2);
                            g.RotateTransform(90);
                            g.DrawString($"{worldY:F0}", font,
                                Brushes.DimGray, 0, 0);
                            g.Restore(state);
                        }
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();

            if (e.Button == MouseButtons.Middle ||
                (e.Button == MouseButtons.Right &&
                 ModifierKeys.HasFlag(Keys.Control)))
            {
                _panning = true;
                _panStart = e.Location;
                _panOffsetStart = _panOffset;
                this.Cursor = Cursors.SizeAll;
                return;
            }

            var worldPos = ScreenToWorld(e.Location);
            if (_grid.SnapAtivo)
                worldPos = _grid.Snap(worldPos);

            _ferramentaAtual?.OnMouseDown(e, worldPos);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _cursorWorld = ScreenToWorld(e.Location);
            CursorMoved?.Invoke(this, _cursorWorld);

            if (_panning)
            {
                _panOffset = new PointF(
                    _panOffsetStart.X + e.X - _panStart.X,
                    _panOffsetStart.Y + e.Y - _panStart.Y);
                Invalidate();
                return;
            }

            var worldPos = _cursorWorld;
            if (_grid.SnapAtivo)
                worldPos = _grid.Snap(worldPos);

            _ferramentaAtual?.OnMouseMove(e, worldPos);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panning)
            {
                _panning = false;
                this.Cursor = _ferramentaAtual?.Cursor ?? Cursors.Default;
                return;
            }

            var worldPos = ScreenToWorld(e.Location);
            if (_grid.SnapAtivo)
                worldPos = _grid.Snap(worldPos);

            _ferramentaAtual?.OnMouseUp(e, worldPos);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            float zoomAnterior = _escala.ZoomLevel;
            float fator = e.Delta > 0 ? 1.15f : 1f / 1.15f;

            _escala.ZoomLevel = Math.Max(0.1f,
                Math.Min(10f, _escala.ZoomLevel * fator));

            // Zoom centrado no cursor
            float ratio = _escala.ZoomLevel / zoomAnterior;
            _panOffset = new PointF(
                e.X - (e.X - _panOffset.X) * ratio,
                e.Y - (e.Y - _panOffset.Y) * ratio);

            ZoomChanged?.Invoke(this, _escala.ZoomLevel);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _ferramentaAtual?.OnKeyDown(e);
            Invalidate();
        }

        /// <summary>
        /// Centralizar vista
        /// </summary>
        public void CentralizarVista()
        {
            _panOffset = new PointF(Width / 2f, Height / 2f);
            Invalidate();
        }

        /// <summary>
        /// Ajustar zoom para mostrar tudo
        /// </summary>
        public void ZoomParaMostrarTudo()
        {
            if (_documento == null || _documento.Objetos.Count == 0)
            {
                CentralizarVista();
                return;
            }

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var obj in _documento.Objetos)
            {
                var bounds = obj.GetBounds();
                minX = Math.Min(minX, bounds.Left);
                minY = Math.Min(minY, bounds.Top);
                maxX = Math.Max(maxX, bounds.Right);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            float contentW = maxX - minX + 100;
            float contentH = maxY - minY + 100;

            _escala.ZoomLevel = Math.Min(
                Width / contentW,
                Height / contentH);

            _panOffset = new PointF(
                Width / 2f - (minX + contentW / 2) * _escala.ZoomLevel,
                Height / 2f - (minY + contentH / 2) * _escala.ZoomLevel);

            Invalidate();
        }

        /// <summary>
        /// Exportar vista atual como imagem
        /// </summary>
        public Bitmap ExportarImagem(int largura, int altura)
        {
            var bmp = new Bitmap(largura, altura);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                // Calcular escala para caber tudo
                if (_documento != null)
                {
                    float minX = float.MaxValue, minY = float.MaxValue;
                    float maxX = float.MinValue, maxY = float.MinValue;

                    foreach (var obj in _documento.Objetos)
                    {
                        var bounds = obj.GetBounds();
                        minX = Math.Min(minX, bounds.Left);
                        minY = Math.Min(minY, bounds.Top);
                        maxX = Math.Max(maxX, bounds.Right);
                        maxY = Math.Max(maxY, bounds.Bottom);
                    }

                    float contentW = maxX - minX + 50;
                    float contentH = maxY - minY + 50;
                    float scale = Math.Min(largura / contentW,
                        altura / contentH) * 0.9f;

                    g.TranslateTransform(
                        largura / 2f - (minX + contentW / 2) * scale,
                        altura / 2f - (minY + contentH / 2) * scale);
                    g.ScaleTransform(scale, scale);

                    foreach (var obj in _documento.Objetos)
                    {
                        obj.Selecionado = false;
                        obj.Desenhar(g);
                    }
                }
            }
            return bmp;
        }
    }
}