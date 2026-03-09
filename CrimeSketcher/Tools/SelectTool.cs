// Tools/SelectTool.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class SelectTool : ITool
    {
        public string Nome => "Selecionar";
        public Cursor Cursor => Cursors.Default;

        private SketchDocument _doc;
        private BaseSketchObject _objetoArrastando;
        private PointF _offsetArraste;
        private PointF _posicaoAnterior;
        private bool _arrastando = false;
        private RectangleF _selecaoRetangular;
        private bool _selecionandoArea = false;
        private PointF _inicioSelecao;
        private UndoRedoManager _undoRedo;

        public BaseSketchObject ObjetoSelecionado { get; private set; }
        public event EventHandler<BaseSketchObject> SelectionChanged;

        public SelectTool(SketchDocument doc, UndoRedoManager undoRedo)
        {
            _doc = doc;
            _undoRedo = undoRedo;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button != MouseButtons.Left) return;

            var hit = _doc.HitTest(worldPos);

            if (hit != null)
            {
                // Desselecionar anterior
                if (ObjetoSelecionado != null)
                    ObjetoSelecionado.Selecionado = false;

                ObjetoSelecionado = hit;
                hit.Selecionado = true;
                _objetoArrastando = hit;
                _offsetArraste = new PointF(
                    worldPos.X - hit.Posicao.X,
                    worldPos.Y - hit.Posicao.Y);
                _posicaoAnterior = hit.Posicao;
                _arrastando = true;

                SelectionChanged?.Invoke(this, hit);
            }
            else
            {
                // Desselecionar
                if (ObjetoSelecionado != null)
                    ObjetoSelecionado.Selecionado = false;
                ObjetoSelecionado = null;
                SelectionChanged?.Invoke(this, null);

                // Iniciar seleção por área
                _selecionandoArea = true;
                _inicioSelecao = worldPos;
                _selecaoRetangular = RectangleF.Empty;
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            if (_arrastando && _objetoArrastando != null &&
                !_objetoArrastando.Bloqueado)
            {
                float dx = worldPos.X - _offsetArraste.X -
                    _objetoArrastando.Posicao.X;
                float dy = worldPos.Y - _offsetArraste.Y -
                    _objetoArrastando.Posicao.Y;
                _objetoArrastando.Mover(dx, dy);
                _objetoArrastando.Posicao = new PointF(
                    worldPos.X - _offsetArraste.X,
                    worldPos.Y - _offsetArraste.Y);
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
            if (_arrastando && _objetoArrastando != null)
            {
                // Registrar undo
                if (_posicaoAnterior != _objetoArrastando.Posicao)
                {
                    _undoRedo.RegistrarAcao(new MoveObjectAction(
                        _objetoArrastando, _posicaoAnterior,
                        _objetoArrastando.Posicao));
                }
            }

            _arrastando = false;
            _objetoArrastando = null;
            _selecionandoArea = false;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (ObjetoSelecionado == null) return;

            float step = e.Shift ? 10f : 1f;

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    _doc.RemoverObjeto(ObjetoSelecionado);
                    ObjetoSelecionado = null;
                    SelectionChanged?.Invoke(this, null);
                    break;
                case Keys.Up:
                    ObjetoSelecionado.Mover(0, -step);
                    break;
                case Keys.Down:
                    ObjetoSelecionado.Mover(0, step);
                    break;
                case Keys.Left:
                    ObjetoSelecionado.Mover(-step, 0);
                    break;
                case Keys.Right:
                    ObjetoSelecionado.Mover(step, 0);
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
            _selecionandoArea = false;
        }
    }
}