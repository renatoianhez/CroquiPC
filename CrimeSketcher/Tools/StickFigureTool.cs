// Tools/StickFigureTool.cs
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class StickFigureTool : ITool
    {
        public string Nome => "Corpo";
        public Cursor Cursor => Cursors.Hand;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        public string PoseInicial { get; set; } = "EmPe";
        public string Rotulo { get; set; } = "Vítima";

        public StickFigureTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                var figure = new StickFigure
                {
                    Posicao = snapped,
                    Rotulo = Rotulo
                };
                figure.DefinirPose(PoseInicial);
                _doc.AdicionarObjeto(figure);
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
            // Preview
            var preview = new StickFigure
            {
                Posicao = _posAtual,
                Opacidade = 0.5f,
                CorContorno = Color.FromArgb(128, 139, 0, 0)
            };
            preview.DefinirPose(PoseInicial);
            preview.Desenhar(g);
        }

        public void Cancelar() { }
    }
}