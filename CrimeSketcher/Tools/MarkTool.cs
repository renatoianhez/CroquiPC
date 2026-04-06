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

        private SketchDocument _doc;
        private UndoRedoManager _undoRedo;
        private PointF? _pontoInicial;
        private MarkObject _marcaPreview;

        // Configurações padrão
        public TipoMarca TipoMarcaPadrao { get; set; } = TipoMarca.Frenagem;
        public float LarguraPadrao { get; set; } = 15f;
        public IntensidadeMarca IntensidadePadrao { get; set; } = IntensidadeMarca.Media;
        public Color CorPadrao { get; set; } = Color.FromArgb(40, 40, 40);

        public MarkTool(SketchDocument doc, UndoRedoManager undoRedo)
        {
            _doc = doc;
            _undoRedo = undoRedo;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Iniciar desenho da marca
                _pontoInicial = worldPos;
                _marcaPreview = new MarkObject
                {
                    PontoInicial = worldPos,
                    PontoFinal = worldPos,
                    Posicao = worldPos, // Define posição para hit test e seleção
                    TipoMarca = TipoMarcaPadrao,
                    Largura = LarguraPadrao,
                    Intensidade = IntensidadePadrao,
                    CorMarca = CorPadrao
                };
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Cancelar
                _pontoInicial = null;
                _marcaPreview = null;
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            if (_pontoInicial.HasValue && _marcaPreview != null)
            {
                float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                var ponto = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, worldPos, passo);
                _marcaPreview.PontoFinal = ponto;
            }
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left && _pontoInicial.HasValue && _marcaPreview != null)
            {
                float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                var ponto = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, worldPos, passo);

                // Finalizar marca
                _marcaPreview.PontoFinal = ponto;

                // Verificar se tem tamanho mínimo
                float dx = _marcaPreview.PontoFinal.X - _marcaPreview.PontoInicial.X;
                float dy = _marcaPreview.PontoFinal.Y - _marcaPreview.PontoInicial.Y;
                float comprimento = (float)Math.Sqrt(dx * dx + dy * dy);

                if (comprimento > 5)
                {
                    // Adicionar ao documento
                    _doc.AdicionarObjeto(_marcaPreview);
                    _undoRedo.RegistrarAcao(new AddObjectAction(_doc, _marcaPreview));
                }

                // Resetar
                _pontoInicial = null;
                _marcaPreview = null;
            }
        }

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
                // Desenhar preview da marca
                _marcaPreview.Desenhar(g);

                // Desenhar linha guia
                using (var pen = new Pen(Color.FromArgb(150, 0, 122, 204), 1f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(pen, _marcaPreview.PontoInicial, _marcaPreview.PontoFinal);
                }
            }
        }

        public void Cancelar()
        {
            _pontoInicial = null;
            _marcaPreview = null;
        }
    }
}
