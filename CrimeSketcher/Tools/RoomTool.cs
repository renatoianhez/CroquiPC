// Tools/RoomTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class RoomTool : ITool
    {
        public string Nome => "Cômodo";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        public string NomeComodo { get; set; } = "Cômodo";
        public float EspessuraParede { get; set; } = 8f;

        public RoomTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                _pontoInicial = snapped;
                _desenhando = true;
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _pontoAtual = _grid.Snap(worldPos);
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (!_desenhando || !_pontoInicial.HasValue) return;

            var snapped = _grid.Snap(worldPos);
            float w = snapped.X - _pontoInicial.Value.X;
            float h = snapped.Y - _pontoInicial.Value.Y;

            if (System.Math.Abs(w) > 10 && System.Math.Abs(h) > 10)
            {
                float x = System.Math.Min(_pontoInicial.Value.X, snapped.X);
                float y = System.Math.Min(_pontoInicial.Value.Y, snapped.Y);

                var room = RoomObject.CriarRetangular(
                    new PointF(x, y),
                    System.Math.Abs(w),
                    System.Math.Abs(h),
                    NomeComodo);
                room.EspessuraParede = EspessuraParede;
                _doc.AdicionarObjeto(room);
            }

            _desenhando = false;
            _pontoInicial = null;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Cancelar();
        }

        public void Desenhar(Graphics g)
        {
            if (!_desenhando || !_pontoInicial.HasValue) return;

            float x = System.Math.Min(_pontoInicial.Value.X, _pontoAtual.X);
            float y = System.Math.Min(_pontoInicial.Value.Y, _pontoAtual.Y);
            float w = System.Math.Abs(_pontoAtual.X - _pontoInicial.Value.X);
            float h = System.Math.Abs(_pontoAtual.Y - _pontoInicial.Value.Y);

            using (var pen = new Pen(Color.FromArgb(128, 0, 0, 0), 2f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, x, y, w, h);
            }
            using (var brush = new SolidBrush(
                Color.FromArgb(30, 100, 100, 100)))
            {
                g.FillRectangle(brush, x, y, w, h);
            }

            // Dimensões
            using (var font = new Font("Segoe UI", 8f))
            {
                string texto = $"{w:F0} x {h:F0}";
                g.DrawString(texto, font, Brushes.Black,
                    x + w / 2 - 20, y + h / 2 - 8);
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
        }
    }
}