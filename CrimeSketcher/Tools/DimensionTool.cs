// Tools/DimensionTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class DimensionTool : ITool
    {
        public string Nome => "Cota";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private ScaleManager _scale;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public DimensionTool(SketchDocument doc, GridManager grid,
            ScaleManager scale)
        {
            _doc = doc;
            _grid = grid;
            _scale = scale;
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
                    var dim = new DimensionLine
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped,
                        Escala = _scale
                    };
                    _doc.AdicionarObjeto(dim);
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

            var preview = new DimensionLine
            {
                PontoInicial = _pontoInicial.Value,
                PontoFinal = _pontoAtual,
                Escala = _scale
            };
            preview.Desenhar(g);
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
        }
    }
}