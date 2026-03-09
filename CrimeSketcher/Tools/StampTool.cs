// Tools/StampTool.cs
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Library;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class StampTool : ITool
    {
        public string Nome => "Carimbo";
        public Cursor Cursor => Cursors.Hand;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF _posAtual;

        public SymbolItem SimboloAtual { get; set; }

        public StampTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left && SimboloAtual != null)
            {
                var snapped = _grid.Snap(worldPos);

                var stamp = new StampObject();
                stamp.CaminhoImagem = SimboloAtual.CaminhoImagem;
                stamp.Posicao = new PointF(
                    snapped.X - SimboloAtual.LarguraPadrao / 2,
                    snapped.Y - SimboloAtual.AlturaPadrao / 2);
                stamp.Largura = SimboloAtual.LarguraPadrao;
                stamp.Altura = SimboloAtual.AlturaPadrao;
                stamp.Nome = SimboloAtual.Nome;
                stamp.CategoriaOrigem = SimboloAtual.Categoria;
                stamp.Descricao = SimboloAtual.Nome;

                _doc.AdicionarObjeto(stamp);
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
            if (SimboloAtual?.Thumbnail == null) return;

            // Preview do símbolo no cursor
            float w = SimboloAtual.LarguraPadrao;
            float h = SimboloAtual.AlturaPadrao;

            g.DrawImage(SimboloAtual.Thumbnail,
                _posAtual.X - w / 2, _posAtual.Y - h / 2, w, h);

            using (var pen = new Pen(Color.FromArgb(100, 0, 120, 255), 1f))
            {
                pen.DashStyle = DashStyle.Dot;
                g.DrawRectangle(pen,
                    _posAtual.X - w / 2, _posAtual.Y - h / 2, w, h);
            }
        }

        public void Cancelar() { }
    }
}