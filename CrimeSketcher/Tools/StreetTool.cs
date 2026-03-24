// Tools/StreetTool.cs
using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public class StreetTool : ITool
    {
        public string Nome => "Rua";
        public Cursor Cursor => Cursors.Cross;

        private SketchDocument _doc;
        private GridManager _grid;
        private PointF? _pontoInicial;
        private PointF _pontoAtual;
        private bool _desenhando = false;
        private bool _arrastandoComMouse = false;
        private bool _houveArraste = false;

        // Configurações da rua
        public float Largura { get; set; } = 80f;
        public string NomeRua { get; set; } = "";
        public int NumeroFaixas { get; set; } = 2;
        public bool TemCalcada { get; set; } = true;
        public float LarguraCalcada { get; set; } = 15f;
        public TipoFaixaCentral TipoFaixa { get; set; } = TipoFaixaCentral.TracejadaSimples;
        public bool MaoUnica { get; set; } = false;

        // Snap de conexão
        private const float TOLERANCIA_CONEXAO = 20f;
        private PointF? _pontoSnapConexao;
        private BaseSketchObject _objetoSnapConexao;
        private int _extremidadeSnapConexao = -1;

        public StreetTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                var snapped = ProcessarSnapConexao(worldPos);

                if (!_desenhando)
                {
                    _pontoInicial = snapped;
                    _pontoAtual = snapped;
                    _desenhando = true;
                    _arrastandoComMouse = true;
                    _houveArraste = false;
                }
                else
                {
                    FinalizarRua(snapped);
                    _arrastandoComMouse = false;
                    _houveArraste = false;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                Cancelar();
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos)
        {
            _pontoAtual = ProcessarSnapConexao(worldPos);

            if (_arrastandoComMouse && _desenhando && _pontoInicial.HasValue && e.Button == MouseButtons.Left)
            {
                _houveArraste = Utils.GeometryHelper.Distancia(_pontoInicial.Value, _pontoAtual) > 5f;
            }
        }

        private void FinalizarRua(PointF pontoFinal)
        {
            if (!_pontoInicial.HasValue) return;

            var street = new StreetObject
            {
                PontoInicial = _pontoInicial.Value,
                PontoFinal = pontoFinal,
                Largura = Largura,
                NomeRua = NomeRua,
                NumeroFaixas = NumeroFaixas,
                TemCalcada = TemCalcada,
                LarguraCalcada = LarguraCalcada,
                TipoFaixaCentral = TipoFaixa,
                MaoUnica = MaoUnica
            };

            if (street.Comprimento <= 5f)
                return;

            VerificarConexoes(street);
            _doc.AdicionarObjeto(street);
            CriarCruzamentoSeNecessario(pontoFinal);

            _desenhando = false;
            _pontoInicial = null;
        }

        private PointF ProcessarSnapConexao(PointF ponto)
        {
            _pontoSnapConexao = null;
            _objetoSnapConexao = null;
            _extremidadeSnapConexao = -1;

            // 1. Verificar snap a extremidades de ruas existentes
            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject rua)
                {
                    int ext = rua.GetExtremidadeProxima(ponto, TOLERANCIA_CONEXAO);
                    if (ext >= 0)
                    {
                        _pontoSnapConexao = rua.GetPontoExtremidade(ext);
                        _objetoSnapConexao = rua;
                        _extremidadeSnapConexao = ext;
                        return _pontoSnapConexao.Value;
                    }
                }
                else if (obj is IntersectionObject cruzamento)
                {
                    foreach (var pc in cruzamento.GetPontosConexao())
                    {
                        float dist = Utils.GeometryHelper.Distancia(ponto, pc);
                        if (dist <= TOLERANCIA_CONEXAO)
                        {
                            _pontoSnapConexao = pc;
                            _objetoSnapConexao = cruzamento;
                            return pc;
                        }
                    }
                }
                else if (obj is RoundaboutObject rotatoria)
                {
                    foreach (var pc in rotatoria.GetPontosConexao())
                    {
                        float dist = Utils.GeometryHelper.Distancia(ponto, pc);
                        if (dist <= TOLERANCIA_CONEXAO)
                        {
                            _pontoSnapConexao = pc;
                            _objetoSnapConexao = rotatoria;
                            return pc;
                        }
                    }
                }
            }

            // 2. Verificar se está criando cruzamento (perto do meio de outra rua)
            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject rua)
                {
                    // Projetar ponto na linha da rua
                    var proj = ProjetarPontoNaRua(ponto, rua);
                    if (proj.HasValue)
                    {
                        float dist = Utils.GeometryHelper.Distancia(ponto, proj.Value);
                        if (dist <= TOLERANCIA_CONEXAO)
                        {
                            _pontoSnapConexao = proj;
                            _objetoSnapConexao = rua;
                            _extremidadeSnapConexao = -2; // Indica criação de cruzamento
                            return proj.Value;
                        }
                    }
                }
            }

            // 3. Snap normal ao grid
            return _grid.Snap(ponto);
        }

        private PointF? ProjetarPontoNaRua(PointF ponto, StreetObject rua)
        {
            var p1 = rua.PontoInicial;
            var p2 = rua.PontoFinal;

            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float comp2 = dx * dx + dy * dy;

            if (comp2 == 0) return null;

            float t = ((ponto.X - p1.X) * dx + (ponto.Y - p1.Y) * dy) / comp2;

            // Ignorar extremidades (já tratadas)
            if (t <= 0.1f || t >= 0.9f) return null;

            return new PointF(p1.X + t * dx, p1.Y + t * dy);
        }

        private void VerificarConexoes(StreetObject novaRua)
        {
            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject ruaExistente && obj != novaRua)
                {
                    // Verificar conexão no ponto inicial
                    int extInicial = ruaExistente.GetExtremidadeProxima(
                        novaRua.PontoInicial, TOLERANCIA_CONEXAO);
                    if (extInicial >= 0)
                    {
                        novaRua.ExtremidadeInicial = TipoExtremidade.Conectada;
                        novaRua.IdConexaoInicial = ruaExistente.Id;

                        if (extInicial == 0)
                            ruaExistente.ExtremidadeInicial = TipoExtremidade.Conectada;
                        else
                            ruaExistente.ExtremidadeFinal = TipoExtremidade.Conectada;
                    }

                    // Verificar conexão no ponto final
                    int extFinal = ruaExistente.GetExtremidadeProxima(
                        novaRua.PontoFinal, TOLERANCIA_CONEXAO);
                    if (extFinal >= 0)
                    {
                        novaRua.ExtremidadeFinal = TipoExtremidade.Conectada;
                        novaRua.IdConexaoFinal = ruaExistente.Id;

                        if (extFinal == 0)
                            ruaExistente.ExtremidadeInicial = TipoExtremidade.Conectada;
                        else
                            ruaExistente.ExtremidadeFinal = TipoExtremidade.Conectada;
                    }
                }
            }
        }

        private void CriarCruzamentoSeNecessario(PointF ponto)
        {
            // Encontrar ruas que passam por este ponto
            var ruasNoPonto = new List<StreetObject>();

            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject rua)
                {
                    float dist = Utils.GeometryHelper.DistanciaPontoSegmento(
                        ponto, rua.PontoInicial, rua.PontoFinal);
                    if (dist < 5f)
                    {
                        ruasNoPonto.Add(rua);
                    }
                }
            }

            // Se há 2 ou mais ruas, pode criar cruzamento
            if (ruasNoPonto.Count >= 2)
            {
                // Detectar tipo de cruzamento baseado nos ângulos
                // (implementação simplificada - pode ser expandida)
            }
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left && _arrastandoComMouse && _desenhando && _pontoInicial.HasValue)
            {
                var snapped = ProcessarSnapConexao(worldPos);
                _pontoAtual = snapped;

                if (_houveArraste)
                {
                    FinalizarRua(snapped);
                }

                _arrastandoComMouse = false;
                _houveArraste = false;
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Cancelar();
        }

        public void Desenhar(Graphics g)
        {
            // Preview da rua sendo desenhada
            if (_desenhando && _pontoInicial.HasValue)
            {
                var preview = new StreetObject
                {
                    PontoInicial = _pontoInicial.Value,
                    PontoFinal = _pontoAtual,
                    Largura = Largura,
                    NomeRua = NomeRua,
                    NumeroFaixas = NumeroFaixas,
                    TemCalcada = TemCalcada,
                    LarguraCalcada = LarguraCalcada,
                    TipoFaixaCentral = TipoFaixa,
                    Opacidade = 0.6f
                };
                preview.Desenhar(g);

                // Mostrar comprimento
                float comp = preview.Comprimento;
                var centro = preview.Centro;

                using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    string texto = $"{comp:F0} px";
                    var size = g.MeasureString(texto, font);

                    using (var bg = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                    {
                        g.FillRectangle(bg,
                            centro.X - size.Width / 2 - 4,
                            centro.Y - size.Height / 2 - 20,
                            size.Width + 8, size.Height + 4);
                    }
                    g.DrawString(texto, font, Brushes.White,
                        centro.X - size.Width / 2,
                        centro.Y - size.Height / 2 - 18);
                }
            }

            // Indicador de snap de conexão
            if (_pontoSnapConexao.HasValue)
            {
                float radius = 10f;
                Color cor = _extremidadeSnapConexao == -2
                    ? Color.Orange   // Criar cruzamento
                    : Color.Lime;    // Conectar extremidade

                using (var pen = new Pen(cor, 3f))
                {
                    g.DrawEllipse(pen,
                        _pontoSnapConexao.Value.X - radius,
                        _pontoSnapConexao.Value.Y - radius,
                        radius * 2, radius * 2);
                }

                // Texto indicativo
                string textoSnap = _extremidadeSnapConexao == -2
                    ? "Criar cruzamento"
                    : "Conectar";

                using (var font = new Font("Segoe UI", 8f))
                using (var bg = new SolidBrush(Color.FromArgb(200, cor)))
                {
                    var size = g.MeasureString(textoSnap, font);
                    g.FillRectangle(bg,
                        _pontoSnapConexao.Value.X - size.Width / 2 - 2,
                        _pontoSnapConexao.Value.Y + radius + 2,
                        size.Width + 4, size.Height + 2);
                    g.DrawString(textoSnap, font, Brushes.Black,
                        _pontoSnapConexao.Value.X - size.Width / 2,
                        _pontoSnapConexao.Value.Y + radius + 3);
                }
            }
        }

        public void Cancelar()
        {
            _desenhando = false;
            _arrastandoComMouse = false;
            _houveArraste = false;
            _pontoInicial = null;
            _pontoSnapConexao = null;
            _objetoSnapConexao = null;
        }
    }
}