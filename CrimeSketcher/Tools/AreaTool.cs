// Tools/AreaTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class AreaTool : ITool
    {
        public string Nome => "Área";
        public Cursor Cursor => Cursors.Cross;

        private readonly SketchDocument _doc;
        private readonly GridManager _grid;
        private readonly List<PointF> _vertices = new List<PointF>();

        private bool _desenhando = false;
        private PointF _pontoAtual;

        public AreaTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            var snapped = _grid.Snap(worldPos);

            if (e.Button == MouseButtons.Left)
            {
                if (!_desenhando)
                {
                    _vertices.Clear();
                    _vertices.Add(snapped);
                    _pontoAtual = snapped;
                    _desenhando = true;
                    return;
                }

                if (Distancia(_vertices[^1], snapped) > 0.5f)
                    _vertices.Add(snapped);

                if (e.Clicks > 1)
                    FinalizarPoligono();
            }
            else if (e.Button == MouseButtons.Right)
            {
                FinalizarPoligono();
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _pontoAtual = _grid.Snap(worldPos);
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Cancelar();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                FinalizarPoligono();
            }
        }

        public void Desenhar(Graphics g)
        {
            if (!_desenhando || _vertices.Count == 0) return;

            using var pen = new Pen(Color.FromArgb(180, 40, 120, 70), 2f);
            pen.DashStyle = DashStyle.Dash;

            if (_vertices.Count >= 2)
            {
                g.DrawLines(pen, _vertices.ToArray());
            }

            g.DrawLine(pen, _vertices[^1], _pontoAtual);

            if (_vertices.Count >= 2)
            {
                var preview = _vertices.Concat(new[] { _pontoAtual }).ToArray();
                using var brush = new SolidBrush(Color.FromArgb(55, 90, 170, 120));
                g.FillPolygon(brush, preview);
            }

            foreach (var p in _vertices)
            {
                g.FillEllipse(Brushes.White, p.X - 2.5f, p.Y - 2.5f, 5f, 5f);
                g.DrawEllipse(Pens.DarkGreen, p.X - 2.5f, p.Y - 2.5f, 5f, 5f);
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _vertices.Clear();
        }

        private void FinalizarPoligono()
        {
            if (!_desenhando)
                return;

            if (_vertices.Count >= 3)
            {
                _doc.AdicionarObjeto(AreaObject.Criar(_vertices));
            }

            _desenhando = false;
            _vertices.Clear();
        }

        private static float Distancia(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
