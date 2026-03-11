// Tools/SelectTool.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class SelectTool : ITool
    {
        public string Nome => "Selecionar";
        public Cursor Cursor => Cursors.Default;

        private readonly SketchDocument _doc;
        private BaseSketchObject? _objetoArrastando;
        private bool _arrastando = false;
        private bool _redimensionando = false;
        private bool _rotacionando = false;
        private int _alcaAtiva = -1;
        private BaseSketchObject? _objetoTransformando;
        private PointF _ultimoPontoMouse;
        private RectangleF _selecaoRetangular;
        private bool _selecionandoArea = false;
        private PointF _inicioSelecao;
        private readonly UndoRedoManager _undoRedo;

        // Controle de arrasto de ponto de curva
        private bool _arrastandoPontoCurva = false;
        private BaseSketchObject? _objetoComCurva = null;
        private PointF _pontoCurvaAnterior;

        // Múltipla seleção
        private readonly HashSet<BaseSketchObject> _objetosSelecionados = new HashSet<BaseSketchObject>();
        private readonly Dictionary<BaseSketchObject, PointF> _posicoesAnterioresGrupo = new Dictionary<BaseSketchObject, PointF>();

        public BaseSketchObject? ObjetoSelecionado { get; private set; }
        public IReadOnlyCollection<BaseSketchObject> ObjetosSelecionados => _objetosSelecionados;

        public event EventHandler<BaseSketchObject?>? SelectionChanged;
        public event EventHandler<IReadOnlyCollection<BaseSketchObject>>? MultiSelectionChanged;

        public SelectTool(SketchDocument doc, UndoRedoManager undoRedo)
        {
            _doc = doc;
            _undoRedo = undoRedo;
        }

        private void DesselcionarTodos()
        {
            foreach (var obj in _objetosSelecionados)
            {
                obj.Selecionado = false;
            }
            _objetosSelecionados.Clear();
            ObjetoSelecionado = null;
        }

        private void AdicionarSelecionado(BaseSketchObject obj)
        {
            if (!_objetosSelecionados.Contains(obj))
            {
                _objetosSelecionados.Add(obj);
                obj.Selecionado = true;
            }
        }

        private void RemoverSelecionado(BaseSketchObject obj)
        {
            _objetosSelecionados.Remove(obj);
            obj.Selecionado = false;
        }

        private void ToggleSelecionado(BaseSketchObject obj)
        {
            if (_objetosSelecionados.Contains(obj))
            {
                RemoverSelecionado(obj);
            }
            else
            {
                AdicionarSelecionado(obj);
            }
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button != MouseButtons.Left) return;

            // Verificar se está clicando no ponto de controle de curva
            if (ObjetoSelecionado is StreetObject street && street.TemCurva)
            {
                if (street.ContemPontoCurva(worldPos, 12f))
                {
                    _arrastandoPontoCurva = true;
                    _objetoComCurva = street;
                    _pontoCurvaAnterior = street.PontoCurva.Value;
                    return;
                }
            }
            else if (ObjetoSelecionado is MarkObject mark && mark.TemCurva)
            {
                if (mark.ContemPontoCurva(worldPos, 12f))
                {
                    _arrastandoPontoCurva = true;
                    _objetoComCurva = mark;
                    _pontoCurvaAnterior = mark.PontoCurva.Value;
                    return;
                }
            }

            if (ObjetoSelecionado != null && !ObjetoSelecionado.Bloqueado)
            {
                int handle = ObjetoSelecionado.GetHandleAtPoint(worldPos, 8f);
                if (handle >= 0)
                {
                    _objetoTransformando = ObjetoSelecionado;
                    _alcaAtiva = handle;
                    _ultimoPontoMouse = worldPos;
                    _rotacionando = handle == 8;
                    _redimensionando = handle >= 0 && handle <= 7;
                    return;
                }
            }

            var hit = _doc.HitTest(worldPos);
            bool ctrlPressed = Control.ModifierKeys.HasFlag(Keys.Control);
            bool shiftPressed = Control.ModifierKeys.HasFlag(Keys.Shift);

            if (hit != null)
            {
                if (ctrlPressed)
                {
                    ToggleSelecionado(hit);
                    ObjetoSelecionado = hit;
                }
                else if (shiftPressed && _objetosSelecionados.Count > 0)
                {
                    AdicionarSelecionado(hit);
                    ObjetoSelecionado = hit;
                }
                else
                {
                    DesselcionarTodos();
                    AdicionarSelecionado(hit);
                    ObjetoSelecionado = hit;
                }

                _objetoArrastando = hit;
                _ultimoPontoMouse = worldPos;

                _posicoesAnterioresGrupo.Clear();
                foreach (var obj in _objetosSelecionados)
                {
                    _posicoesAnterioresGrupo[obj] = obj.Posicao;
                }

                _arrastando = true;

                SelectionChanged?.Invoke(this, hit);
                var lista = _objetosSelecionados.ToList();
                MultiSelectionChanged?.Invoke(this, lista);
            }
            else
            {
                if (!ctrlPressed && !shiftPressed)
                {
                    DesselcionarTodos();
                    SelectionChanged?.Invoke(this, null);
                    var lista = _objetosSelecionados.ToList();
                    MultiSelectionChanged?.Invoke(this, lista);
                }

                _selecionandoArea = true;
                _inicioSelecao = worldPos;
                _selecaoRetangular = RectangleF.Empty;
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            if (_arrastandoPontoCurva && _objetoComCurva != null)
            {
                if (_objetoComCurva is StreetObject street)
                {
                    street.MoverPontoCurva(worldPos);
                }
                else if (_objetoComCurva is MarkObject mark)
                {
                    mark.MoverPontoCurva(worldPos);
                }
            }
            else if (_redimensionando && _objetoTransformando != null)
            {
                AplicarRedimensionamento(_objetoTransformando, worldPos);
                _ultimoPontoMouse = worldPos;
            }
            else if (_rotacionando && _objetoTransformando != null)
            {
                AplicarRotacao(_objetoTransformando, worldPos);
                _ultimoPontoMouse = worldPos;
            }
            else if (_arrastando && _objetoArrastando != null && !_objetoArrastando.Bloqueado)
            {
                float dx = worldPos.X - _ultimoPontoMouse.X;
                float dy = worldPos.Y - _ultimoPontoMouse.Y;
                _ultimoPontoMouse = worldPos;

                foreach (var obj in _objetosSelecionados)
                {
                    if (!obj.Bloqueado)
                    {
                        obj.Mover(dx, dy);
                    }
                }
            }
            else if (_selecionandoArea)
            {
                float x = Math.Min(_inicioSelecao.X, worldPos.X);
                float y = Math.Min(_inicioSelecao.Y, worldPos.Y);
                float w = Math.Abs(worldPos.X - _inicioSelecao.X);
                float h = Math.Abs(worldPos.Y - _inicioSelecao.Y);
                _selecaoRetangular = new RectangleF(x, y, w, h);
            }
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (_arrastandoPontoCurva && _objetoComCurva != null)
            {
                if (_objetoComCurva is StreetObject street && street.PontoCurva.HasValue)
                {
                    _ = _pontoCurvaAnterior != street.PontoCurva.Value;
                }
                else if (_objetoComCurva is MarkObject mark && mark.PontoCurva.HasValue)
                {
                    _ = _pontoCurvaAnterior != mark.PontoCurva.Value;
                }

                _arrastandoPontoCurva = false;
                _objetoComCurva = null;
            }
            else if (_arrastando && _objetoArrastando != null)
            {
                bool algumMoveu = false;
                foreach (var obj in _objetosSelecionados)
                {
                    if (_posicoesAnterioresGrupo.TryGetValue(obj, out var posAnterior) &&
                        posAnterior != obj.Posicao)
                    {
                        algumMoveu = true;
                        break;
                    }
                }

                if (algumMoveu)
                {
                    foreach (var obj in _objetosSelecionados)
                    {
                        if (_posicoesAnterioresGrupo.TryGetValue(obj, out var posAnterior) &&
                            posAnterior != obj.Posicao)
                        {
                            _undoRedo.RegistrarAcao(new MoveObjectAction(
                                obj, posAnterior, obj.Posicao));
                        }
                    }
                }
            }
            else if (_selecionandoArea && _selecaoRetangular != RectangleF.Empty)
            {
                var objetosNaArea = _doc.HitTestArea(_selecaoRetangular);
                bool ctrlPressed = Control.ModifierKeys.HasFlag(Keys.Control);

                if (!ctrlPressed)
                {
                    DesselcionarTodos();
                }

                foreach (var obj in objetosNaArea)
                {
                    AdicionarSelecionado(obj);
                }

                if (objetosNaArea.Count > 0)
                {
                    ObjetoSelecionado = objetosNaArea.Last();
                    var lista = _objetosSelecionados.ToList();
                    MultiSelectionChanged?.Invoke(this, lista);
                }
            }

            _arrastando = false;
            _redimensionando = false;
            _rotacionando = false;
            _alcaAtiva = -1;
            _objetoArrastando = null;
            _objetoTransformando = null;
            _selecionandoArea = false;
            _posicoesAnterioresGrupo.Clear();
        }

        private void AplicarRotacao(BaseSketchObject obj, PointF worldPos)
        {
            var bounds = obj.GetBounds();
            var centro = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);

            float anguloAnterior = (float)Math.Atan2(_ultimoPontoMouse.Y - centro.Y, _ultimoPontoMouse.X - centro.X);
            float anguloAtual = (float)Math.Atan2(worldPos.Y - centro.Y, worldPos.X - centro.X);
            float deltaGraus = (anguloAtual - anguloAnterior) * 180f / (float)Math.PI;

            if (Math.Abs(deltaGraus) > 0.01f)
            {
                obj.RotacionarAoRedor(centro, deltaGraus);
            }
        }

        private void AplicarRedimensionamento(BaseSketchObject obj, PointF worldPos)
        {
            var bounds = obj.GetBounds();
            if (bounds.Width < 0.001f || bounds.Height < 0.001f)
                return;

            float left = bounds.Left;
            float right = bounds.Right;
            float top = bounds.Top;
            float bottom = bounds.Bottom;

            switch (_alcaAtiva)
            {
                case 0: left = worldPos.X; top = worldPos.Y; break;
                case 1: right = worldPos.X; top = worldPos.Y; break;
                case 2: left = worldPos.X; bottom = worldPos.Y; break;
                case 3: right = worldPos.X; bottom = worldPos.Y; break;
                case 4: top = worldPos.Y; break;
                case 5: bottom = worldPos.Y; break;
                case 6: left = worldPos.X; break;
                case 7: right = worldPos.X; break;
                default: return;
            }

            var novo = RectangleF.FromLTRB(
                Math.Min(left, right),
                Math.Min(top, bottom),
                Math.Max(left, right),
                Math.Max(top, bottom));

            if (novo.Width < 5f || novo.Height < 5f)
                return;

            float fatorX = novo.Width / bounds.Width;
            float fatorY = novo.Height / bounds.Height;

            if (_alcaAtiva == 4 || _alcaAtiva == 5) fatorX = 1f;
            if (_alcaAtiva == 6 || _alcaAtiva == 7) fatorY = 1f;

            var centroAntigo = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
            var centroNovo = new PointF(novo.Left + novo.Width / 2f, novo.Top + novo.Height / 2f);

            obj.EscalarAoRedor(centroAntigo, fatorX, fatorY);
            obj.Mover(centroNovo.X - centroAntigo.X, centroNovo.Y - centroAntigo.Y);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_objetosSelecionados.Count == 0) return;

            float step = e.Shift ? 10f : 1f;

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    var aRemover = _objetosSelecionados.ToList();
                    foreach (var obj in aRemover)
                    {
                        _doc.RemoverObjeto(obj);
                    }
                    DesselcionarTodos();
                    SelectionChanged?.Invoke(this, null);
                    var lista = _objetosSelecionados.ToList();
                    MultiSelectionChanged?.Invoke(this, lista);
                    break;
                case Keys.Up:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(0, -step);
                    break;
                case Keys.Down:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(0, step);
                    break;
                case Keys.Left:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(-step, 0);
                    break;
                case Keys.Right:
                    foreach (var obj in _objetosSelecionados)
                        obj.Mover(step, 0);
                    break;
            }
        }

        public void Desenhar(Graphics g)
        {
            if (_selecionandoArea && _selecaoRetangular != RectangleF.Empty)
            {
                using (var pen = new Pen(Color.DodgerBlue, 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen,
                        _selecaoRetangular.X, _selecaoRetangular.Y,
                        _selecaoRetangular.Width, _selecaoRetangular.Height);
                }
                using (var brush = new SolidBrush(
                    Color.FromArgb(30, 30, 144, 255)))
                {
                    g.FillRectangle(brush, _selecaoRetangular);
                }
            }
        }

        public void Cancelar()
        {
            _arrastando = false;
            _redimensionando = false;
            _rotacionando = false;
            _alcaAtiva = -1;
            _objetoTransformando = null;
            _selecionandoArea = false;
            _arrastandoPontoCurva = false;
            _objetoComCurva = null;
            DesselcionarTodos();
        }

        /// <summary>
        /// Define um único objeto como selecionado (uso público)
        /// </summary>
        public void SelecionarObjeto(BaseSketchObject obj)
        {
            DesselcionarTodos();
            if (obj != null)
            {
                AdicionarSelecionado(obj);
                ObjetoSelecionado = obj;
                SelectionChanged?.Invoke(this, obj);
                var lista = _objetosSelecionados.ToList();
                MultiSelectionChanged?.Invoke(this, lista);
            }
        }

        /// <summary>
        /// Limpa toda a seleção (uso público)
        /// </summary>
        public void LimparSelecao()
        {
            DesselcionarTodos();
            SelectionChanged?.Invoke(this, null);
            var lista = _objetosSelecionados.ToList();
            MultiSelectionChanged?.Invoke(this, lista);
        }
    }
}
