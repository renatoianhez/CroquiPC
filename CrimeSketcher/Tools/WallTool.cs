// Tools/WallTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public enum ModoAberturaWall
    {
        Nenhum,
        PosicionandoPorta,
        PosicionandoJanela
    }

    public class WallTool : ITool
    {
        public string Nome => "Parede";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;

        // Positioning mode
        private ModoAberturaWall _modoAbertura = ModoAberturaWall.Nenhum;
        private WallObject _paredeEditando = null;
        private float _posicaoPreview = 0.5f;
        private Action _onPosicionamentoConcluido;

        public float Espessura { get; set; } = 8f;
        public bool ComPorta { get; set; } = false;
        public bool ComJanela { get; set; } = false;

        public bool EmModoPosicionamento => _modoAbertura != ModoAberturaWall.Nenhum;

        public WallTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void EntrarModoPosicionamento(WallObject parede, ModoAberturaWall modo, Action onConcluido = null)
        {
            _desenhando = false;
            _pontoInicial = null;
            _paredeEditando = parede;
            _modoAbertura = modo;
            _onPosicionamentoConcluido = onConcluido;
            _posicaoPreview = modo == ModoAberturaWall.PosicionandoPorta
                ? parede.PosicaoPorta
                : parede.PosicaoJanela;
        }

        private float ProjetarNaParede(PointF ponto, WallObject parede)
        {
            float dx = parede.PontoFinal.X - parede.PontoInicial.X;
            float dy = parede.PontoFinal.Y - parede.PontoInicial.Y;
            float comp2 = dx * dx + dy * dy;
            if (comp2 < 0.001f) return 0.5f;
            float t = ((ponto.X - parede.PontoInicial.X) * dx + (ponto.Y - parede.PontoInicial.Y) * dy) / comp2;
            return Math.Max(0.05f, Math.Min(0.95f, t));
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (_modoAbertura != ModoAberturaWall.Nenhum && e.Button == MouseButtons.Left && _paredeEditando != null)
            {
                float pos = ProjetarNaParede(worldPos, _paredeEditando);
                if (_modoAbertura == ModoAberturaWall.PosicionandoPorta)
                    _paredeEditando.PosicaoPorta = pos;
                else
                    _paredeEditando.PosicaoJanela = pos;

                _doc.NotificarAlteracao();

                var callback = _onPosicionamentoConcluido;
                _onPosicionamentoConcluido = null;
                _modoAbertura = ModoAberturaWall.Nenhum;
                _paredeEditando = null;
                callback?.Invoke();
                return;
            }

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
                    float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                    snapped = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, snapped, passo);

                    var wall = new WallObject
                    {
                        PontoInicial = _pontoInicial.Value,
                        PontoFinal = snapped,
                        Espessura = Espessura,
                        TemPorta = ComPorta,
                        TemJanela = ComJanela
                    };
                    _doc.AdicionarObjeto(wall);

                    _pontoInicial = snapped;
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            if (_modoAbertura != ModoAberturaWall.Nenhum && _paredeEditando != null)
            {
                _posicaoPreview = ProjetarNaParede(worldPos, _paredeEditando);
                return;
            }

            var snapped = _grid.Snap(worldPos);
            if (_desenhando && _pontoInicial.HasValue)
            {
                float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                snapped = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, snapped, passo);
            }
            _pontoAtual = snapped;
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Cancelar();
        }

        public void Desenhar(Graphics g)
        {
            if (_modoAbertura != ModoAberturaWall.Nenhum && _paredeEditando != null)
            {
                DesenharPreviewPosicionamento(g);
                return;
            }

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
                var esc = ScaleManager.Atual;
                string texto = esc != null ? esc.FormatarMedida(comp) : $"{comp:F0} px";
                var size = g.MeasureString(texto, font);
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 200)),
                    mid.X - size.Width / 2 - 2, mid.Y - 2,
                    size.Width + 4, size.Height + 4);
                g.DrawString(texto, font, Brushes.Black,
                    mid.X - size.Width / 2, mid.Y);
            }
        }

        private void DesenharPreviewPosicionamento(Graphics g)
        {
            float dx = _paredeEditando.PontoFinal.X - _paredeEditando.PontoInicial.X;
            float dy = _paredeEditando.PontoFinal.Y - _paredeEditando.PontoInicial.Y;
            float comp = (float)System.Math.Sqrt(dx * dx + dy * dy);
            if (comp < 0.1f) return;

            float largura = _modoAbertura == ModoAberturaWall.PosicionandoPorta
                ? _paredeEditando.LarguraPorta
                : _paredeEditando.LarguraJanela;

            float nx = dx / comp;
            float ny = dy / comp;
            float centro = _posicaoPreview * comp;
            float inicio = Math.Max(2f, centro - largura / 2f);
            float fim = Math.Min(comp - 2f, centro + largura / 2f);

            var p1 = new PointF(
                _paredeEditando.PontoInicial.X + nx * inicio,
                _paredeEditando.PontoInicial.Y + ny * inicio);
            var p2 = new PointF(
                _paredeEditando.PontoFinal.X - nx * (comp - fim),
                _paredeEditando.PontoFinal.Y - ny * (comp - fim));

            Color cor = _modoAbertura == ModoAberturaWall.PosicionandoPorta
                ? Color.FromArgb(200, Color.OrangeRed)
                : Color.FromArgb(200, Color.DodgerBlue);

            using (var pen = new Pen(cor, _paredeEditando.Espessura))
                g.DrawLine(pen, p1, p2);

            string label = _modoAbertura == ModoAberturaWall.PosicionandoPorta
                ? "Clique para posicionar a porta"
                : "Clique para posicionar a janela";

            var mid = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2 - 15);
            using (var font = new Font("Segoe UI", 8f))
            {
                var size = g.MeasureString(label, font);
                g.FillRectangle(new SolidBrush(Color.FromArgb(210, 255, 255, 200)),
                    mid.X - size.Width / 2 - 2, mid.Y - 2,
                    size.Width + 4, size.Height + 4);
                g.DrawString(label, font, Brushes.Black, mid.X - size.Width / 2, mid.Y);
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _pontoInicial = null;
            _modoAbertura = ModoAberturaWall.Nenhum;
            _paredeEditando = null;
            _onPosicionamentoConcluido = null;
        }
    }
}