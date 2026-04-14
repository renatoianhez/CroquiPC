// Tools/DimensionTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class DimensionTool : ITool
    {
        public string Nome => "Cota";
        public Cursor Cursor => Cursors.Cross;

        private readonly SketchDocument _doc;
        private readonly GridManager _grid;
        private readonly ScaleManager _scale;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public Color CorTexto { get; set; } = Color.FromArgb(200, 0, 0);
        public Color CorFundoTexto { get; set; } = Color.FromArgb(60, 255, 255, 255);

        public DimensionTool(SketchDocument doc, GridManager grid,
            ScaleManager scale)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _scale = scale ?? throw new ArgumentNullException(nameof(scale));
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                if (!_desenhando)
                {
                    _pontoInicial = snapped;
                    _pontoAtual = snapped;
                    _desenhando = true;
                }
                else
                {
                    snapped = AplicarSnapAngularSeNecessario(snapped);

                    var dim = new DimensionLine
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped,
                        Escala = _scale,
                        CorTexto = CorTexto,
                        CorFundoTexto = CorFundoTexto
                    };
                    _doc.AdicionarObjeto(dim);
                    _desenhando = false;
                    _pontoInicial = null;
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            var snapped = _grid.Snap(worldPos);
            if (_desenhando && _pontoInicial.HasValue)
            {
                snapped = AplicarSnapAngularSeNecessario(snapped);
            }
            _pontoAtual = snapped;
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
                Escala = _scale,
                CorTexto = CorTexto,
                CorFundoTexto = CorFundoTexto
            };
            preview.Desenhar(g);
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
            _pontoAtual = PointF.Empty;
        }

        private PointF AplicarSnapAngularSeNecessario(PointF ponto)
        {
            if (!_pontoInicial.HasValue)
                return ponto;

            float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
            return Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, ponto, passo);
        }
    }
}