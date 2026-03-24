// Tools/StickFigureTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class StickFigureTool : ITool
    {
        public string Nome => "Corpo";
        public Cursor Cursor => Cursors.Hand;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        public PoseCorpo Pose { get; set; } = PoseCorpo.EmPe;
        public GeneroCorpo Genero { get; set; } = GeneroCorpo.Masculino;
        public string Rotulo { get; set; } = "Vítima";
        public int NumeroMarcador { get; set; } = 0;

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
                    Pose = Pose,
                    Genero = Genero,
                    Rotulo = Rotulo,
                    NumeroMarcador = NumeroMarcador,
                    BracosEstendidos = Pose == PoseCorpo.VistaAerea
                };
                figure.AplicarProporcoesGenero();

                _doc.AdicionarObjeto(figure);

                // Incrementar marcador para próximo corpo
                if (NumeroMarcador > 0)
                    NumeroMarcador++;
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
                Pose = Pose,
                Genero = Genero,
                Rotulo = Rotulo,
                NumeroMarcador = NumeroMarcador,
                BracosEstendidos = Pose == PoseCorpo.VistaAerea,
                Opacidade = 0.6f
            };
            preview.AplicarProporcoesGenero();
            preview.Desenhar(g);
        }

        public void Cancelar() { }
    }
}