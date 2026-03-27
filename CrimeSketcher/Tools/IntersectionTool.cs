// Tools/IntersectionTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class IntersectionTool : ITool
    {
        public string Nome => "Cruzamento";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        private float _larguraRua = 80f;
        private bool _temCanteiroCentral = false;
        private float _larguraCanteiroCentral = 12f;

        public TipoCruzamento TipoCruzamento { get; set; } = TipoCruzamento.Cruz;

        public float LarguraRua
        {
            get => _larguraRua;
            set => _larguraRua = Math.Max(10f, value);
        }

        public bool TemCalcada { get; set; } = true;

        public bool TemCanteiroCentral
        {
            get => _temCanteiroCentral;
            set
            {
                float larguraUtil = ObterLarguraUtilPista();
                _temCanteiroCentral = value;
                _larguraRua = Math.Max(10f, larguraUtil + (_temCanteiroCentral ? _larguraCanteiroCentral : 0f));
            }
        }

        public float LarguraCanteiroCentral
        {
            get => _larguraCanteiroCentral;
            set
            {
                float larguraUtil = ObterLarguraUtilPista();
                _larguraCanteiroCentral = Math.Max(2f, value);
                if (_temCanteiroCentral)
                    _larguraRua = Math.Max(10f, larguraUtil + _larguraCanteiroCentral);
            }
        }

        private float ObterLarguraUtilPista()
        {
            float canteiro = _temCanteiroCentral ? _larguraCanteiroCentral : 0f;
            return Math.Max(6f, _larguraRua - canteiro);
        }

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
                    TemCalcada = TemCalcada,
                    TemCanteiroCentral = TemCanteiroCentral,
                    LarguraCanteiroCentral = LarguraCanteiroCentral
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
                TemCanteiroCentral = TemCanteiroCentral,
                LarguraCanteiroCentral = LarguraCanteiroCentral,
                Opacidade = 0.5f
            };
            preview.Desenhar(g);
        }

        public void Cancelar() { }
    }
}