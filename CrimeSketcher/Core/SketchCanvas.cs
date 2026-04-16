// Core/SketchCanvas.cs
using CrimeSketcher.Library;
using CrimeSketcher.Tools;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace CrimeSketcher.Core
{
    public class SketchCanvas : Panel
    {
        private const int ScrollRange = 200000;
        private const int ScrollCenter = ScrollRange / 2;
        private const float ScrollPanPorUnidade = 0.25f;

        private readonly HScrollBar _scrollH;
        private readonly VScrollBar _scrollV;
        private bool _atualizandoScrollbars = false;

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

        // Tema
        private Color _corFundoCanvas;
        private Color _corHudFundo;
        private Color _corHudTexto;
        private Color _corReguaFundo;
        private Color _corReguaLinha;
        private Color _corReguaTexto;

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
        public event EventHandler ToolDeactivated;

        public SketchCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = SystemColors.Window;

            _escala = new ScaleManager();
            _grid = new GridManager(_escala);
            AplicarTemaSistema();

            _scrollH = new HScrollBar();
            _scrollV = new VScrollBar();
            InicializarScrollbars();
        }

        private void InicializarScrollbars()
        {
            _scrollH.TabStop = false;
            _scrollV.TabStop = false;

            _scrollH.Dock = DockStyle.Bottom;
            _scrollV.Dock = DockStyle.Right;
            _scrollH.Visible = true;
            _scrollV.Visible = true;

            _scrollH.Scroll += (_, __) =>
            {
                if (_atualizandoScrollbars) return;
                AplicarPanAPartirDosScrollbars();
                Invalidate();
            };

            _scrollV.Scroll += (_, __) =>
            {
                if (_atualizandoScrollbars) return;
                AplicarPanAPartirDosScrollbars();
                Invalidate();
            };

            Controls.Add(_scrollH);
            Controls.Add(_scrollV);
            _scrollH.BringToFront();
            _scrollV.BringToFront();

            AtualizarLayoutScrollbars();
            AtualizarScrollbarsAPartirDoPan();
        }

        private int LarguraViewport => Math.Max(1, ClientSize.Width - _scrollV.Width);
        private int AlturaViewport => Math.Max(1, ClientSize.Height - _scrollH.Height);

        private void AtualizarLayoutScrollbars()
        {
            _atualizandoScrollbars = true;
            try
            {
                _scrollH.Minimum = 0;
                _scrollH.Maximum = ScrollRange - 1;
                _scrollH.SmallChange = 20;
                _scrollH.LargeChange = Math.Max(10, LarguraViewport);

                _scrollV.Minimum = 0;
                _scrollV.Maximum = ScrollRange - 1;
                _scrollV.SmallChange = 20;
                _scrollV.LargeChange = Math.Max(10, AlturaViewport);
            }
            finally
            {
                _atualizandoScrollbars = false;
            }
        }

        private int LimitarValorScrollbar(ScrollBar sb, int valor)
        {
            int maxValor = Math.Max(sb.Minimum, sb.Maximum - sb.LargeChange + 1);
            return Math.Max(sb.Minimum, Math.Min(maxValor, valor));
        }

        private void AtualizarScrollbarsAPartirDoPan()
        {
            _atualizandoScrollbars = true;
            try
            {
                int valorH = LimitarValorScrollbar(_scrollH,
                    (int)Math.Round(ScrollCenter - (_panOffset.X / ScrollPanPorUnidade)));
                int valorV = LimitarValorScrollbar(_scrollV,
                    (int)Math.Round(ScrollCenter - (_panOffset.Y / ScrollPanPorUnidade)));

                _scrollH.Value = valorH;
                _scrollV.Value = valorV;
            }
            finally
            {
                _atualizandoScrollbars = false;
            }
        }

        private void AplicarPanAPartirDosScrollbars()
        {
            _panOffset = new PointF(
                (ScrollCenter - _scrollH.Value) * ScrollPanPorUnidade,
                (ScrollCenter - _scrollV.Value) * ScrollPanPorUnidade);
        }

        public void AplicarTemaSistema()
        {
            bool temaEscuro = SystemColors.Window.GetBrightness() < 0.5f;

            _corFundoCanvas = SystemColors.Window;
            _corHudTexto = SystemColors.WindowText;
            _corReguaFundo = ControlPaint.Light(SystemColors.Control, temaEscuro ? 0.1f : 0.35f);
            _corReguaLinha = SystemColors.GrayText;
            _corReguaTexto = SystemColors.ControlText;

            _corHudFundo = temaEscuro
                ? Color.FromArgb(180, 20, 20, 20)
                : Color.FromArgb(180, 245, 245, 245);

            _grid.CorGrade = temaEscuro
                ? Color.FromArgb(45, 220, 220, 220)
                : Color.FromArgb(40, 100, 100, 100);

            _grid.CorGradePrincipal = temaEscuro
                ? Color.FromArgb(80, 235, 235, 235)
                : Color.FromArgb(60, 80, 80, 80);

            Invalidate();
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

        private float PixelsParaMundo(float pixels)
        {
            float zoom = Math.Max(0.0001f, _escala.ZoomLevel);
            return pixels / zoom;
        }

        private void AtualizarContextoFerramenta()
        {
            if (_ferramentaAtual is SelectTool selectTool)
            {
                selectTool.ZoomLevel = _escala.ZoomLevel;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fundo
            g.Clear(_corFundoCanvas);

            // Aplicar transformação (pan + zoom)
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_escala.ZoomLevel, _escala.ZoomLevel);

            // Área visível no mundo
            var topLeft = ScreenToWorld(Point.Empty);
            var bottomRight = ScreenToWorld(
                new Point(LarguraViewport, AlturaViewport));
            var areaVisivel = new RectangleF(
                topLeft.X, topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y);

            // Grade
            _grid.Desenhar(g, areaVisivel);

            // Objetos do documento
            if (_documento != null)
            {
                var objetosOrdenados = _documento.Objetos
                    .Select((obj, indice) => new { Objeto = obj, Indice = indice })
                    .Where(x => x.Objeto.Visivel)
                    .OrderBy(x => x.Objeto.Camada)
                    .ThenBy(x => x.Indice)
                    .Select(x => x.Objeto)
                    .ToList();

                foreach (var obj in objetosOrdenados)
                {
                    DesenharObjetoComOpacidade(g, obj);
                }

                // Desenhar seleções após todos os objetos
                foreach (var obj in _documento.Objetos)
                {
                    obj.DesenharSelecao(g);
                }
            }

            // Preview da ferramenta atual
            _ferramentaAtual?.Desenhar(g);

            // Resetar transform para desenhar HUD
            g.ResetTransform();

            // HUD - Informações na tela
            DesenharHUD(g);
        }

        private void DesenharObjetoComOpacidade(Graphics g, BaseSketchObject obj)
        {
            float opacidade = Math.Clamp(obj.Opacidade, 0f, 1f);

            if (opacidade >= 0.999f)
            {
                obj.Desenhar(g);
                return;
            }

            var bounds = obj.GetBounds();
            if (bounds.Width <= 0.1f || bounds.Height <= 0.1f)
            {
                obj.Desenhar(g);
                return;
            }

            const float margem = 8f;
            var area = RectangleF.FromLTRB(
                bounds.Left - margem,
                bounds.Top - margem,
                bounds.Right + margem,
                bounds.Bottom + margem);

            int bmpW = Math.Max(1, (int)Math.Ceiling(area.Width));
            int bmpH = Math.Max(1, (int)Math.Ceiling(area.Height));

            using var bmp = new Bitmap(bmpW, bmpH, PixelFormat.Format32bppPArgb);
            using (var gb = Graphics.FromImage(bmp))
            {
                gb.SmoothingMode = SmoothingMode.AntiAlias;
                gb.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                gb.Clear(Color.Transparent);
                gb.TranslateTransform(-area.X, -area.Y);

                float opOriginal = obj.Opacidade;
                try
                {
                    obj.Opacidade = 1f;
                    obj.Desenhar(gb);
                }
                finally
                {
                    obj.Opacidade = opOriginal;
                }
            }

            using var ia = new ImageAttributes();
            var cm = new ColorMatrix { Matrix33 = opacidade };
            ia.SetColorMatrix(cm);

            g.DrawImage(
                bmp,
                new Rectangle((int)area.X, (int)area.Y, bmpW, bmpH),
                0,
                0,
                bmpW,
                bmpH,
                GraphicsUnit.Pixel,
                ia);
        }

        private void DesenharHUD(Graphics g)
        {
            // Informações de escala/zoom/posição já são exibidas na status bar

            // Régua horizontal
            DesenharRegua(g, true);
            DesenharRegua(g, false);
        }

        private void DesenharRegua(Graphics g, bool horizontal)
        {
            float rulerSize = 20f;
            using (var bgBrush = new SolidBrush(_corReguaFundo))
            using (var pen = new Pen(_corReguaLinha, 0.5f))
            using (var font = new Font("Segoe UI", 6f))
            {
                if (horizontal)
                {
                    g.FillRectangle(bgBrush, 0, 0, LarguraViewport, rulerSize);
                    g.DrawLine(pen, 0, rulerSize, LarguraViewport, rulerSize);

                    float spacing = _grid.EspacamentoPixels * _escala.ZoomLevel;
                    float start = _panOffset.X % spacing;

                    for (float x = start; x < LarguraViewport; x += spacing)
                    {
                        float worldX = (x - _panOffset.X) / _escala.ZoomLevel;
                        g.DrawLine(pen, x, rulerSize - 5, x, rulerSize);

                        if (Math.Abs(worldX % (_grid.EspacamentoPixels *
                            _grid.SubdivisoesPrincipais)) < 1)
                        {
                            g.DrawLine(pen, x, 0, x, rulerSize);
                            using (var textBrush = new SolidBrush(_corReguaTexto))
                            {
                                float realX = _escala.PixelsParaReal(worldX);
                                g.DrawString($"{realX:F1}", font,
                                    textBrush, x + 2, 2);
                            }
                        }
                    }
                }
                else
                {
                    g.FillRectangle(bgBrush, 0, 0, rulerSize, AlturaViewport);
                    g.DrawLine(pen, rulerSize, 0, rulerSize, AlturaViewport);

                    float spacing = _grid.EspacamentoPixels * _escala.ZoomLevel;
                    float start = _panOffset.Y % spacing;

                    for (float y = start; y < AlturaViewport; y += spacing)
                    {
                        float worldY = (y - _panOffset.Y) / _escala.ZoomLevel;
                        g.DrawLine(pen, rulerSize - 5, y, rulerSize, y);

                        if (Math.Abs(worldY % (_grid.EspacamentoPixels *
                            _grid.SubdivisoesPrincipais)) < 1)
                        {
                            g.DrawLine(pen, 0, y, rulerSize, y);
                            var state = g.Save();
                            g.TranslateTransform(2, y + 2);
                            g.RotateTransform(90);
                            using (var textBrush = new SolidBrush(_corReguaTexto))
                            {
                                float realY = _escala.PixelsParaReal(worldY);
                                g.DrawString($"{realY:F1}", font,
                                    textBrush, 0, 0);
                            }
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
            if (_grid.SnapAtivo && _ferramentaAtual is not SelectTool)
                worldPos = _grid.Snap(worldPos);

            AtualizarContextoFerramenta();
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
                AtualizarScrollbarsAPartirDoPan();
                Invalidate();
                return;
            }

            var worldPos = _cursorWorld;
            if (_grid.SnapAtivo && _ferramentaAtual is not SelectTool)
                worldPos = _grid.Snap(worldPos);

            AtualizarContextoFerramenta();

            // Mudar cursor se estiver sobre ponto de controle de curva
            AtualizarCursor(worldPos);

            _ferramentaAtual?.OnMouseMove(e, worldPos);
            Invalidate();
        }

        private void AtualizarCursor(PointF worldPos)
        {
            // Verificar se o cursor está sobre ponto de controle de curva
            if (_ferramentaAtual is Tools.SelectTool selectTool)
            {
                if (selectTool.EstaSobreArticulacaoCorpo(worldPos, 10f))
                {
                    this.Cursor = Cursors.Hand;
                    return;
                }

                if (selectTool.EstaSobreHandleParede(worldPos, 10f))
                {
                    this.Cursor = Cursors.Hand;
                    return;
                }

                var objetoSelecionado = selectTool.ObjetoSelecionado;

                if (objetoSelecionado != null)
                {
                    int handle = objetoSelecionado.GetHandleAtPoint(worldPos, PixelsParaMundo(8f), _escala.ZoomLevel);
                    if (handle == 8)
                    {
                        this.Cursor = Cursors.Hand;
                        return;
                      }
                    if (handle >= 0)
                    {
                        this.Cursor = Cursors.SizeNWSE;
                        return;
                    }
                }

                // Verificar StreetObject
                if (objetoSelecionado is Objects.StreetObject street && street.TemCurva)
                {
                    if (street.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                    {
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }
                // Verificar MarkObject
                else if (objetoSelecionado is Objects.MarkObject mark && mark.TemCurva)
                {
                    if (mark.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                    {
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }
                else if (objetoSelecionado is Objects.ArrowObject arrow && arrow.TemCurva)
                {
                    if (arrow.ContemPontoCurva(worldPos, PixelsParaMundo(12f)))
                    {
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }
            }

            // Cursor padrão da ferramenta
            if (_ferramentaAtual != null)
            {
                this.Cursor = _ferramentaAtual.Cursor;
            }
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

            // Desativar ferramenta com clique do botão direito (sem Ctrl)
            if (e.Button == MouseButtons.Right && !ModifierKeys.HasFlag(Keys.Control))
            {
                if (_ferramentaAtual != null && _ferramentaAtual.Nome != "Selecionar")
                {
                    _ferramentaAtual?.Cancelar();
                    _ferramentaAtual = null;
                    this.Cursor = Cursors.Default;
                    ToolDeactivated?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                return;
            }

            var worldPos = ScreenToWorld(e.Location);
            if (_grid.SnapAtivo && _ferramentaAtual is not SelectTool)
                worldPos = _grid.Snap(worldPos);

            AtualizarContextoFerramenta();
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

            AtualizarScrollbarsAPartirDoPan();
            ZoomChanged?.Invoke(this, _escala.ZoomLevel);
            Invalidate();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            AtualizarLayoutScrollbars();
            AtualizarScrollbarsAPartirDoPan();
        }

        /// <summary>
        /// Centralizar vista
        /// </summary>
        public void CentralizarVista()
        {
            _panOffset = new PointF(LarguraViewport / 2f, AlturaViewport / 2f);
            AtualizarScrollbarsAPartirDoPan();
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
                LarguraViewport / contentW,
                AlturaViewport / contentH);

            _panOffset = new PointF(
                LarguraViewport / 2f - (minX + contentW / 2) * _escala.ZoomLevel,
                AlturaViewport / 2f - (minY + contentH / 2) * _escala.ZoomLevel);

            AtualizarScrollbarsAPartirDoPan();
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