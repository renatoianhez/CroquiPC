// Tools/StreetTool.cs
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class StreetTool : ITool
    {
        public string Nome => "Rua";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public float Largura { get; set; } = 80f;
        public string NomeRua { get; set; } = "";
        public int NumeroFaixas { get; set; } = 2;
        public bool TemCalcada { get; set; } = true;

        public StreetTool(SketchDocument doc, GridManager grid)
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
                    var street = new StreetObject
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped,
                        Largura = Largura,
                        NomeRua = NomeRua,
                        NumeroFaixas = NumeroFaixas,
                        TemCalcada = TemCalcada
                    };
                    _doc.AdicionarObjeto(street);
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

            // Preview da rua
            var preview = new StreetObject
            {
                PontoInicial = _pontoInicial.Value,
                PontoFinal = _pontoAtual,
                Largura = Largura,
                NomeRua = NomeRua,
                NumeroFaixas = NumeroFaixas,
                TemCalcada = TemCalcada,
                Opacidade = 0.5f
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