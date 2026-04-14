// Tools/MarkTool.cs - Ferramenta de Marcas
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class MarkTool : ITool
    {
        public string Nome => "Marca";
        public Cursor Cursor => Cursors.Cross;

        private readonly SketchDocument _doc;
        private readonly UndoRedoManager _undoRedo;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private MarkObject _marcaPreview;
        private bool _desenhando;

        // Configurações padrão
        public TipoMarca TipoMarcaPadrao { get; set; } = TipoMarca.Frenagem;
        public float LarguraPadrao { get; set; } = 15f;
        public IntensidadeMarca IntensidadePadrao { get; set; } = IntensidadeMarca.Media;
        public Color CorPadrao { get; set; } = Color.FromArgb(40, 40, 40);

        public MarkTool(SketchDocument doc, UndoRedoManager undoRedo)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Right)
            {
                Cancelar();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            if (!_desenhando)
            {
                _pontoInicial = worldPos;
                _pontoAtual = worldPos;
                _marcaPreview = new MarkObject
                {
                    PontoInicial = worldPos,
                    PontoFinal = worldPos,
                    Posicao = worldPos,
                    TipoMarca = TipoMarcaPadrao,
                    Largura = LarguraPadrao,
                    Intensidade = IntensidadePadrao,
                    CorMarca = CorPadrao
                };
                _desenhando = true;
                return;
            }

            if (!_pontoInicial.HasValue || _marcaPreview == null)
                return;

            var ponto = AplicarSnapAngularSeNecessario(worldPos);
            _marcaPreview.PontoFinal = ponto;

            float dx = _marcaPreview.PontoFinal.X - _marcaPreview.PontoInicial.X;
            float dy = _marcaPreview.PontoFinal.Y - _marcaPreview.PontoInicial.Y;
            float comprimento = (float)Math.Sqrt(dx * dx + dy * dy);

            if (comprimento > 5f)
            {
                _doc.AdicionarObjeto(_marcaPreview);
                _undoRedo.RegistrarAcao(new AddObjectAction(_doc, _marcaPreview));
            }

            Cancelar();
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _pontoAtual = worldPos;

            if (!_desenhando || !_pontoInicial.HasValue || _marcaPreview == null)
                return;

            _marcaPreview.PontoFinal = AplicarSnapAngularSeNecessario(worldPos);
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Cancelar();
            }
        }

        public void Desenhar(Graphics g)
        {
            if (_marcaPreview != null && _pontoInicial.HasValue)
            {
                _marcaPreview.Desenhar(g);

                using (var pen = new Pen(Color.FromArgb(150, 0, 122, 204), 1f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(pen, _marcaPreview.PontoInicial, _marcaPreview.PontoFinal);
                }
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
            _pontoAtual = PointF.Empty;
            _marcaPreview = null;
        }

        private PointF AplicarSnapAngularSeNecessario(PointF ponto)
        {
            if (!_pontoInicial.HasValue)
                return ponto;

            float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
            return Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, ponto, passo);
        }
    }
}
