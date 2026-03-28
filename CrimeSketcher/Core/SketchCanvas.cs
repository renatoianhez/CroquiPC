// Core/SketchCanvas.cs
using CrimeSketcher.Library;
using CrimeSketcher.Tools;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
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
                // Separar objetos por camada
                var camadaInferior = new List<BaseSketchObject>();
                var camadaSuperior = new List<BaseSketchObject>();
                foreach (var obj in _documento.Objetos)
                {
                    if (!obj.Visivel) continue;
                    string tipo = obj.Tipo?.ToLowerInvariant() ?? "";
                    if (tipo.Contains("símbolo") || tipo.Contains("symbol") ||
                        tipo.Contains("corpo") || tipo.Contains("stickfigure") ||
                        tipo.Contains("seta") || tipo.Contains("arrow") ||
                        tipo.Contains("marca") || tipo.Contains("mark") ||
                        tipo.Contains("texto") || tipo.Contains("text"))
                    {
                        camadaSuperior.Add(obj);
                    }
                    else
                    {
                        camadaInferior.Add(obj);
                    }
                }

                // Desenhar camada inferior (ruas, cruzamentos, paredes, etc)
                foreach (var obj in camadaInferior)
                {
                    obj.Desenhar(g);
                }
                // Desenhar camada superior (símbolos, corpos, setas, marcas, textos)
                foreach (var obj in camadaSuperior)
                {
                    obj.Desenhar(g);
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

        private void DesenharHUD(Graphics g)
        {
            // Escala no canto inferior esquerdo
            using (var font = new Font("Segoe UI", 9f))
            using (var bgBrush = new SolidBrush(_corHudFundo))
            using (var textBrush = new SolidBrush(_corHudTexto))
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
            using (var bgBrush = new SolidBrush(_corReguaFundo))
            using (var pen = new Pen(_corReguaLinha, 0.5f))
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
                            g.DrawLine(pen, x, 0, x, rulerSize);
                            using (var textBrush = new SolidBrush(_corReguaTexto))
                            {
                                g.DrawString($"{worldX:F0}", font,
                                    textBrush, x + 2, 2);
                            }
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
                            g.DrawLine(pen, 0, y, rulerSize, y);
                            var state = g.Save();
                            g.TranslateTransform(2, y + 2);
                            g.RotateTransform(90);
                            using (var textBrush = new SolidBrush(_corReguaTexto))
                            {
                                g.DrawString($"{worldY:F0}", font,
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

                var objetoSelecionado = selectTool.ObjetoSelecionado;

                if (objetoSelecionado != null)
                {
                    int handle = objetoSelecionado.GetHandleAtPoint(worldPos, 8f);
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
                    if (street.ContemPontoCurva(worldPos, 12f))
                    {
                        this.Cursor = Cursors.SizeAll;
                        return;
                    }
                }
                // Verificar MarkObject
                else if (objetoSelecionado is Objects.MarkObject mark && mark.TemCurva)
                {
                    if (mark.ContemPontoCurva(worldPos, 12f))
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