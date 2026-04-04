// Tools/RoundaboutTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class RoundaboutTool : ITool
    {
        public string Nome => "Rotatória";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        public float RaioExterno { get; set; } = 120f;
        public float RaioInterno { get; set; } = 80f;
        public int NumeroSaidas { get; set; } = 4;
        public float LarguraRua { get; set; } = 80f;
        public bool TemCalcada { get; set; } = true;

        public RoundaboutTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                var roundabout = new RoundaboutObject
                {
                    Posicao = snapped,
                    RaioExterno = RaioExterno,
                    RaioInterno = RaioInterno,
                    NumeroSaidas = NumeroSaidas,
                    LarguraRua = LarguraRua,
                    TemCalcada = TemCalcada
                };
                _doc.AdicionarObjeto(roundabout);
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _posAtual = _grid.Snap(worldPos);
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }
        public void OnKeyDown(KeyEventArgs e) { }

        public void Desenhar(Graphics g)
        {
            var preview = new RoundaboutObject
            {
                Posicao = _posAtual,
                RaioExterno = RaioExterno,
                RaioInterno = RaioInterno,
                NumeroSaidas = NumeroSaidas,
                LarguraRua = LarguraRua,
                TemCalcada = TemCalcada,
                Opacidade = 0.5f
            };
            preview.Desenhar(g);
        }

        public void Cancelar() { }
    }
}