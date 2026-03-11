// Tools/IntersectionTool.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class IntersectionTool : ITool
    {
        public string Nome => "Cruzamento";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        public TipoCruzamento TipoCruzamento { get; set; } = TipoCruzamento.Cruz;
        public float LarguraRua { get; set; } = 80f;
        public bool TemCalcada { get; set; } = true;

        public IntersectionTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = _grid.Snap(worldPos);
                var intersection = new IntersectionObject
                {
                    Posicao = snapped,
                    TipoCruzamento = TipoCruzamento,
                    LarguraRua = LarguraRua,
                    TemCalcada = TemCalcada
                };
                _doc.AdicionarObjeto(intersection);
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
            var preview = new IntersectionObject
            {
                Posicao = _posAtual,
                TipoCruzamento = TipoCruzamento,
                LarguraRua = LarguraRua,
                TemCalcada = TemCalcada,
                Opacidade = 0.5f
            };
            preview.Desenhar(g);
        }

        public void Cancelar() { }
    }
}