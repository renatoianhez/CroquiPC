// Tools/WallTool.cs
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class WallTool : ITool
    {
        public string Nome => "Parede";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public float Espessura { get; set; } = 8f;
        public bool ComPorta { get; set; } = false;
        public bool ComJanela { get; set; } = false;

        public WallTool(SketchDocument doc, GridManager grid)
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
                    // Finalizar parede
                    var wall = new WallObject
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped,
                        Espessura = Espessura,
                        TemPorta = ComPorta,
                        TemJanela = ComJanela
                    };
                    _doc.AdicionarObjeto(wall);

                    // Continuar a partir deste ponto
                    _pontoInicial = snapped;
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

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 0), Espessura))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(pen, _pontoInicial.Value, _pontoAtual);
            }

            // Mostrar comprimento em tempo real
            float dx = _pontoAtual.X - _pontoInicial.Value.X;
            float dy = _pontoAtual.Y - _pontoInicial.Value.Y;
            float comp = (float)System.Math.Sqrt(dx * dx + dy * dy);

            var mid = new PointF(
                (_pontoInicial.Value.X + _pontoAtual.X) / 2,
                (_pontoInicial.Value.Y + _pontoAtual.Y) / 2 - 15);

            using (var font = new Font("Segoe UI", 8f))
            {
                string texto = $"{comp:F0} px";
                var size = g.MeasureString(texto, font);
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 200)),
                    mid.X - size.Width / 2 - 2, mid.Y - 2,
                    size.Width + 4, size.Height + 4);
                g.DrawString(texto, font, Brushes.Black,
                    mid.X - size.Width / 2, mid.Y);
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
        }
    }
}