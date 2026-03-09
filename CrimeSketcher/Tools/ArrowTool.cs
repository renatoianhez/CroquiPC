// Tools/ArrowTool.cs
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class ArrowTool : ITool
    {
        public string Nome => "Seta";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public ArrowTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                if (!_desenhando)
                {
                    _pontoInicial = snapped;
                    _desenhando = true;
                }
                else
                {
                    var arrow = new ArrowObject
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped
                    };
                    _doc.AdicionarObjeto(arrow);
                    _desenhando = false;
                    _pontoInicial = null;
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _pontoAtual = _grid.Snap(worldPos);
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Cancelar();
        }

        public void Desenhar(Graphics g)
        {
            if (!_desenhando || !_pontoInicial.HasValue) return;

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 180), 2f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pen.CustomEndCap = new System.Drawing.Drawing2D
                    .AdjustableArrowCap(5, 5);
                g.DrawLine(pen, _pontoInicial.Value, _pontoAtual);
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
        }
    }
}