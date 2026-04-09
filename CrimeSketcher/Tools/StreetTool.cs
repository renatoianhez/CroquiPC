using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private float _largura = 80f;
        private int _numeroFaixas = 2;
        private bool _temCanteiroCentral = false;
        private float _larguraCanteiroCentral = 12f;
        private bool _temCiclofaixa = false;
        private bool _temFaixaEstacionamento = false;
        private const float LARGURA_FAIXA_PADRAO = 40f;
        private const float LARGURA_CICLOFAIXA = 15f;
        private const float LARGURA_ESTACIONAMENTO = 30f;
        private const float TOLERANCIA_CONEXAO = 20f;

        public static float FatorEscalaLarguras { get; set; } = 1f;

        private static float EscalaLarguras => Math.Max(0.1f, FatorEscalaLarguras);
        private static float LarguraFaixaPadraoEscalada => LARGURA_FAIXA_PADRAO * EscalaLarguras;
        private static float LarguraCiclofaixaEscalada => LARGURA_CICLOFAIXA * EscalaLarguras;
        private static float LarguraEstacionamentoEscalada => LARGURA_ESTACIONAMENTO * EscalaLarguras;

        private PointF? _pontoSnapConexao;
        private BaseSketchObject _objetoSnapConexao;
        private int _extremidadeSnapConexao = -1;
        private PointF? _direcaoSnapConexaoAtual;
        private bool _snapTemCanteiroAtual = false;
        private float _larguraCanteiroSnapAtual = 0f;
        private PointF? _direcaoSnapInicial;
        private bool _snapTemCanteiroInicial = false;
        private float _larguraCanteiroSnapInicial = 0f;

        public float Largura
        {
            get => _largura;
            set => _largura = Math.Max(30f, value);
        }

        public string NomeRua { get; set; } = "";
        public string FonteNomeRua { get; set; } = "Segoe UI";
        public float TamanhoFonteNomeRua { get; set; } = 9f;
        public float DeslocamentoNomeRuaX { get; set; } = 0f;
        public float DeslocamentoNomeRuaY { get; set; } = 0f;

        public int NumeroFaixas
        {
            get => _numeroFaixas;
            set
            {
                int novoNumero = NormalizarNumeroFaixas(value, _temCanteiroCentral);
                AjustarLarguraParaManterFaixas(
                    novoNumero,
                    _temCanteiroCentral,
                    _larguraCanteiroCentral,
                    _temCiclofaixa,
                    _temFaixaEstacionamento);
                _numeroFaixas = novoNumero;
            }
        }

        public bool TemCalcada { get; set; } = true;
        public float LarguraCalcada { get; set; } = 15f;
        public TipoFaixaCentral TipoFaixa { get; set; } = TipoFaixaCentral.TracejadaSimples;
        public bool MaoUnica { get; set; } = false;
        public TipoLinhaEstacionamento TipoLinhaEstacionamento { get; set; } = TipoLinhaEstacionamento.Tracejada;
        public Color CorEstacionamento { get; set; } = Color.Transparent;
        public Color CorLinhaEstacionamento { get; set; } = Color.White;

        public bool TemCanteiroCentral
        {
            get => _temCanteiroCentral;
            set
            {
                int novoNumeroFaixas = NormalizarNumeroFaixas(_numeroFaixas, value);
                AjustarLarguraParaManterFaixas(
                    novoNumeroFaixas,
                    value,
                    _larguraCanteiroCentral,
                    _temCiclofaixa,
                    _temFaixaEstacionamento);
                _temCanteiroCentral = value;
                _numeroFaixas = novoNumeroFaixas;
            }
        }

        public float LarguraCanteiroCentral
        {
            get => _larguraCanteiroCentral;
            set
            {
                float novaLargura = Math.Max(2f, value);
                int novoNumeroFaixas = NormalizarNumeroFaixas(_numeroFaixas, _temCanteiroCentral);
                AjustarLarguraParaManterFaixas(
                    novoNumeroFaixas,
                    _temCanteiroCentral,
                    novaLargura,
                    _temCiclofaixa,
                    _temFaixaEstacionamento);
                _larguraCanteiroCentral = novaLargura;
                _numeroFaixas = novoNumeroFaixas;
            }
        }

        public bool TemCiclofaixa
        {
            get => _temCiclofaixa;
            set
            {
                int novoNumeroFaixas = NormalizarNumeroFaixas(_numeroFaixas, _temCanteiroCentral);
                AjustarLarguraParaManterFaixas(
                    novoNumeroFaixas,
                    _temCanteiroCentral,
                    _larguraCanteiroCentral,
                    value,
                    _temFaixaEstacionamento);
                _temCiclofaixa = value;
                _numeroFaixas = novoNumeroFaixas;
            }
        }

        public bool TemFaixaEstacionamento
        {
            get => _temFaixaEstacionamento;
            set
            {
                int novoNumeroFaixas = NormalizarNumeroFaixas(_numeroFaixas, _temCanteiroCentral);
                AjustarLarguraParaManterFaixas(
                    novoNumeroFaixas,
                    _temCanteiroCentral,
                    _larguraCanteiroCentral,
                    _temCiclofaixa,
                    value);
                _temFaixaEstacionamento = value;
                _numeroFaixas = novoNumeroFaixas;
            }
        }

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
                    _direcaoSnapInicial = _objetoSnapConexao is IntersectionObject
                        ? _direcaoSnapConexaoAtual
                        : null;
                    _snapTemCanteiroInicial = _objetoSnapConexao is IntersectionObject && _snapTemCanteiroAtual;
                    _larguraCanteiroSnapInicial = _objetoSnapConexao is IntersectionObject ? _larguraCanteiroSnapAtual : 0f;
                    _desenhando = true;
                    _arrastandoComMouse = true;
                    _houveArraste = false;
                }
                else
                {
                    if (!_pontoSnapConexao.HasValue && _pontoInicial.HasValue)
                    {
                        float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                        snapped = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, snapped, passo);
                    }

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

            if (_desenhando && _pontoInicial.HasValue && !_pontoSnapConexao.HasValue)
            {
                float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                _pontoAtual = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, _pontoAtual, passo);
            }

            if (_arrastandoComMouse && _desenhando && _pontoInicial.HasValue && e.Button == MouseButtons.Left)
            {
                _houveArraste = Utils.GeometryHelper.Distancia(_pontoInicial.Value, _pontoAtual) > 5f;
            }
        }

        private void FinalizarRua(PointF pontoFinal)
        {
            if (!_pontoInicial.HasValue) return;

            PointF pontoInicialAjustado = _pontoInicial.Value;
            PointF pontoFinalAjustado = pontoFinal;
            PointF? pontoCurva = null;

            PointF? direcaoFinal = _objetoSnapConexao is IntersectionObject
                ? _direcaoSnapConexaoAtual
                : null;

            bool snapFinalTemCanteiro = _objetoSnapConexao is IntersectionObject && _snapTemCanteiroAtual;
            float larguraCanteiroConexao = Math.Max(
                _snapTemCanteiroInicial ? _larguraCanteiroSnapInicial : 0f,
                snapFinalTemCanteiro ? _larguraCanteiroSnapAtual : 0f);

            bool usarCanteiro = TemCanteiroCentral || _snapTemCanteiroInicial || snapFinalTemCanteiro;
            float larguraCanteiroRua = usarCanteiro
                ? Math.Max(LarguraCanteiroCentral, larguraCanteiroConexao)
                : LarguraCanteiroCentral;

            AjustarGeometriaConexao(
                ref pontoInicialAjustado,
                ref pontoFinalAjustado,
                _direcaoSnapInicial,
                direcaoFinal,
                out pontoCurva);

            var street = new StreetObject
            {
                PontoInicial = pontoInicialAjustado,
                PontoFinal = pontoFinalAjustado,
                PontoCurva = pontoCurva,
                NomeRua = NomeRua,
                FonteNomeRua = FonteNomeRua,
                TamanhoFonteNomeRua = TamanhoFonteNomeRua,
                DeslocamentoNomeRuaX = DeslocamentoNomeRuaX,
                DeslocamentoNomeRuaY = DeslocamentoNomeRuaY,
                TemCalcada = TemCalcada,
                LarguraCalcada = LarguraCalcada,
                TipoFaixaCentral = TipoFaixa,
                MaoUnica = MaoUnica
            };

            street.TemCanteiroCentral = usarCanteiro;
            street.LarguraCanteiroCentral = larguraCanteiroRua;
            street.TemCiclofaixa = TemCiclofaixa;
            street.TemFaixaEstacionamento = TemFaixaEstacionamento;
            street.TipoLinhaEstacionamento = TipoLinhaEstacionamento;
            street.CorEstacionamento = CorEstacionamento;
            street.CorLinhaEstacionamento = CorLinhaEstacionamento;
            street.NumeroFaixas = NumeroFaixas;
            street.Largura = Largura;

            if (street.Comprimento <= 5f)
                return;

            VerificarConexoes(street);
            _doc.AdicionarObjeto(street);
            AtualizarCruzamentosConectados(street);
            CriarCruzamentoSeNecessario();

            _desenhando = false;
            _pontoInicial = null;
            _direcaoSnapInicial = null;
            _snapTemCanteiroInicial = false;
            _larguraCanteiroSnapInicial = 0f;
        }

        private PointF ProcessarSnapConexao(PointF ponto)
        {
            _pontoSnapConexao = null;
            _objetoSnapConexao = null;
            _extremidadeSnapConexao = -1;
            _direcaoSnapConexaoAtual = null;
            _snapTemCanteiroAtual = false;
            _larguraCanteiroSnapAtual = 0f;

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
                    if (cruzamento.TryObterConexaoProxima(
                        ponto,
                        TOLERANCIA_CONEXAO,
                        out var pontoConexao,
                        out var direcaoSaida,
                        out var temCanteiroNoBraco))
                    {
                        _pontoSnapConexao = pontoConexao;
                        _objetoSnapConexao = cruzamento;
                        _direcaoSnapConexaoAtual = direcaoSaida;
                        _snapTemCanteiroAtual = temCanteiroNoBraco;
                        _larguraCanteiroSnapAtual = cruzamento.LarguraCanteiroCentral;
                        return pontoConexao;
                    }
                }
                else if (obj is RoundaboutObject rotatoria)
                {
                    if (TryObterPontoConexaoRotatoria(rotatoria, ponto, out var pontoConexao))
                    {
                        _pontoSnapConexao = pontoConexao;
                        _objetoSnapConexao = rotatoria;
                        return pontoConexao;
                    }
                }
            }

            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject rua)
                {
                    var proj = ProjetarPontoNaRua(ponto, rua);
                    if (proj.HasValue)
                    {
                        float dist = Utils.GeometryHelper.Distancia(ponto, proj.Value);
                        if (dist <= TOLERANCIA_CONEXAO)
                        {
                            _pontoSnapConexao = proj;
                            _objetoSnapConexao = rua;
                            _extremidadeSnapConexao = -2;
                            return proj.Value;
                        }
                    }
                }
            }

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

            if (t <= 0.1f || t >= 0.9f) return null;

            return new PointF(p1.X + t * dx, p1.Y + t * dy);
        }

        private void VerificarConexoes(StreetObject novaRua)
        {
            foreach (var obj in _doc.Objetos)
            {
                if (obj is StreetObject ruaExistente && obj != novaRua)
                {
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
                else if (obj is RoundaboutObject rotatoria)
                {
                    if (TryObterPontoConexaoRotatoria(rotatoria, novaRua.PontoInicial, out var conexaoInicial))
                    {
                        novaRua.PontoInicial = conexaoInicial;
                        novaRua.ExtremidadeInicial = TipoExtremidade.Conectada;
                        novaRua.IdConexaoInicial = rotatoria.Id;
                        RegistrarRuaConectadaRotatoria(rotatoria, novaRua.Id);
                    }

                    if (TryObterPontoConexaoRotatoria(rotatoria, novaRua.PontoFinal, out var conexaoFinal))
                    {
                        novaRua.PontoFinal = conexaoFinal;
                        novaRua.ExtremidadeFinal = TipoExtremidade.Conectada;
                        novaRua.IdConexaoFinal = rotatoria.Id;
                        RegistrarRuaConectadaRotatoria(rotatoria, novaRua.Id);
                    }
                }
            }
        }

        private bool TryObterPontoConexaoRotatoria(RoundaboutObject rotatoria, PointF ponto, out PointF pontoConexao)
        {
            foreach (var pc in rotatoria.GetPontosConexao())
            {
                if (Utils.GeometryHelper.Distancia(ponto, pc) <= TOLERANCIA_CONEXAO)
                {
                    pontoConexao = pc;
                    return true;
                }
            }

            float dx = ponto.X - rotatoria.Posicao.X;
            float dy = ponto.Y - rotatoria.Posicao.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float raioConexao = rotatoria.RaioExterno + rotatoria.LarguraRua;
            float toleranciaRaio = Math.Max(TOLERANCIA_CONEXAO, rotatoria.LarguraRua * 0.2f);

            if (dist > 0.0001f && Math.Abs(dist - raioConexao) <= toleranciaRaio)
            {
                float nx = dx / dist;
                float ny = dy / dist;
                pontoConexao = new PointF(
                    rotatoria.Posicao.X + nx * raioConexao,
                    rotatoria.Posicao.Y + ny * raioConexao);
                return true;
            }

            pontoConexao = PointF.Empty;
            return false;
        }

        private static void RegistrarRuaConectadaRotatoria(RoundaboutObject rotatoria, string idRua)
        {
            if (string.IsNullOrWhiteSpace(idRua))
                return;

            if (!rotatoria.RuasConectadas.Contains(idRua))
                rotatoria.RuasConectadas.Add(idRua);
        }

        private void CriarCruzamentoSeNecessario()
        {
            var ruas = _doc.Objetos.OfType<StreetObject>().ToList();
            
            var todosOsCruzamentos = new List<(StreetObject rua1, StreetObject rua2, PointF ponto, IntersectionType tipo)>();
            
            for (int i = 0; i < ruas.Count; i++)
            {
                for (int j = i + 1; j < ruas.Count; j++)
                {
                    var cruzamentos = IntersectionDetector.DetectarCruzamentos(ruas[i], _doc.Objetos);
                    todosOsCruzamentos.AddRange(cruzamentos);
                }
            }

            foreach (var (rua1, rua2, ponto, tipo) in todosOsCruzamentos)
            {
                bool jaExiste = _doc.Objetos.OfType<AutoIntersectionObject>()
                    .Any(a => Utils.GeometryHelper.Distancia(a.Posicao, ponto) < 10f);

                if (!jaExiste)
                {
                    float angulo1 = CalcularAngulo(rua1.PontoInicial, rua1.PontoFinal);
                    float angulo2 = CalcularAngulo(rua2.PontoInicial, rua2.PontoFinal);

                    var autoInterseccao = new AutoIntersectionObject
                    {
                        Posicao = ponto,
                        TipoCruzamento = tipo,
                        LarguraRua1 = rua1.Largura,
                        LarguraRua2 = rua2.Largura,
                        AnguloRua1 = angulo1,
                        AnguloRua2 = angulo2,
                        IdRua1 = rua1.Id,
                        IdRua2 = rua2.Id,
                        TemCanteiroCentral = rua1.TemCanteiroCentral || rua2.TemCanteiroCentral,
                        LarguraCanteiroCentral = Math.Max(rua1.LarguraCanteiroCentral, rua2.LarguraCanteiroCentral),
                        TemCalcadaRua1 = rua1.TemCalcada,
                        TemCalcadaRua2 = rua2.TemCalcada,
                        LarguraCalcadaRua1 = rua1.LarguraCalcada,
                        LarguraCalcadaRua2 = rua2.LarguraCalcada,
                        TemCiclofaixaRua1 = rua1.TemCiclofaixa,
                        TemCiclofaixaRua2 = rua2.TemCiclofaixa,
                        TemEstacionamentoRua1 = rua1.TemFaixaEstacionamento,
                        TemEstacionamentoRua2 = rua2.TemFaixaEstacionamento
                    };

                    _doc.AdicionarObjeto(autoInterseccao);
                }
            }
        }

        private float CalcularAngulo(PointF inicio, PointF fim)
        {
            float dx = fim.X - inicio.X;
            float dy = fim.Y - inicio.Y;
            double angulo = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            return (float)angulo;
        }

        public void OnMouseUp(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left && _arrastandoComMouse && _desenhando && _pontoInicial.HasValue)
            {
                var snapped = ProcessarSnapConexao(worldPos);

                if (!_pontoSnapConexao.HasValue)
                {
                    float passo = (Control.ModifierKeys & Keys.Shift) != 0 ? 15f : 5f;
                    snapped = Utils.GeometryHelper.SnapAngulo(_pontoInicial.Value, snapped, passo);
                }

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
            if (_desenhando && _pontoInicial.HasValue)
            {
                var preview = new StreetObject
                {
                    PontoInicial = _pontoInicial.Value,
                    PontoFinal = _pontoAtual,
                    NomeRua = NomeRua,
                    FonteNomeRua = FonteNomeRua,
                    TamanhoFonteNomeRua = TamanhoFonteNomeRua,
                    DeslocamentoNomeRuaX = DeslocamentoNomeRuaX,
                    DeslocamentoNomeRuaY = DeslocamentoNomeRuaY,
                    TemCalcada = TemCalcada,
                    LarguraCalcada = LarguraCalcada,
                    TipoFaixaCentral = TipoFaixa,
                    Opacidade = 0.6f
                };

                preview.TemCanteiroCentral = TemCanteiroCentral;
                preview.LarguraCanteiroCentral = LarguraCanteiroCentral;
                preview.TemCiclofaixa = TemCiclofaixa;
                preview.TemFaixaEstacionamento = TemFaixaEstacionamento;
                preview.TipoLinhaEstacionamento = TipoLinhaEstacionamento;
                preview.CorEstacionamento = CorEstacionamento;
                preview.CorLinhaEstacionamento = CorLinhaEstacionamento;
                preview.NumeroFaixas = NumeroFaixas;
                preview.Largura = Largura;

                preview.Desenhar(g);

                float comp = preview.Comprimento;
                var centro = preview.Centro;

                using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    var esc = ScaleManager.Atual;
                    string texto = esc != null ? esc.FormatarMedida(comp) : $"{comp:F0} px";
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

            if (_pontoSnapConexao.HasValue)
            {
                float radius = 10f;
                Color cor = _extremidadeSnapConexao == -2
                    ? Color.Orange
                    : Color.Lime;

                using (var pen = new Pen(cor, 3f))
                {
                    g.DrawEllipse(pen,
                        _pontoSnapConexao.Value.X - radius,
                        _pontoSnapConexao.Value.Y - radius,
                        radius * 2, radius * 2);
                }

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

        private void AtualizarCruzamentosConectados(StreetObject street)
        {
            foreach (var obj in _doc.Objetos)
            {
                if (obj is not IntersectionObject cruzamento)
                    continue;

                foreach (var pc in cruzamento.GetPontosConexao())
                {
                    if (Utils.GeometryHelper.Distancia(street.PontoInicial, pc) <= TOLERANCIA_CONEXAO ||
                        Utils.GeometryHelper.Distancia(street.PontoFinal, pc) <= TOLERANCIA_CONEXAO)
                    {
                        cruzamento.LarguraRua = Math.Max(cruzamento.LarguraRua, street.Largura);
                        break;
                    }
                }
            }
        }

        private void AjustarGeometriaConexao(
            ref PointF pontoInicial,
            ref PointF pontoFinal,
            PointF? direcaoInicio,
            PointF? direcaoFinal,
            out PointF? pontoCurva)
        {
            pontoCurva = null;

            if (direcaoInicio.HasValue && direcaoFinal.HasValue)
            {
                if (SaoParalelas(direcaoInicio.Value, direcaoFinal.Value))
                {
                    pontoFinal = ProjetarNoEixo(pontoInicial, pontoFinal, direcaoInicio.Value);
                }
                else
                {
                    if (TryCalcularPontoCurvaEntreEixos(
                        pontoInicial,
                        pontoFinal,
                        direcaoInicio.Value,
                        direcaoFinal.Value,
                        out var controle))
                    {
                        pontoCurva = controle;
                    }
                }

                return;
            }

            if (direcaoInicio.HasValue)
            {
                pontoFinal = ProjetarNoEixo(pontoInicial, pontoFinal, direcaoInicio.Value);
            }
            else if (direcaoFinal.HasValue)
            {
                pontoInicial = ProjetarNoEixo(pontoFinal, pontoInicial, direcaoFinal.Value);
            }
        }

        private static PointF ProjetarNoEixo(PointF origem, PointF ponto, PointF eixo)
        {
            var dir = Normalizar(eixo);
            float dx = ponto.X - origem.X;
            float dy = ponto.Y - origem.Y;
            float t = dx * dir.X + dy * dir.Y;

            return new PointF(
                origem.X + dir.X * t,
                origem.Y + dir.Y * t);
        }

        private static bool SaoParalelas(PointF a, PointF b)
        {
            var na = Normalizar(a);
            var nb = Normalizar(b);
            float dot = Math.Abs(na.X * nb.X + na.Y * nb.Y);
            return dot >= 0.98f;
        }

        private static bool TryCalcularPontoCurvaEntreEixos(
            PointF p0,
            PointF p2,
            PointF eixoInicio,
            PointF eixoFinal,
            out PointF controle)
        {
            var d0 = Normalizar(eixoInicio);
            var d2 = Normalizar(eixoFinal);

            var v02 = new PointF(p2.X - p0.X, p2.Y - p0.Y);
            if (v02.X * d0.X + v02.Y * d0.Y < 0)
                d0 = new PointF(-d0.X, -d0.Y);

            var v20 = new PointF(p0.X - p2.X, p0.Y - p2.Y);
            if (v20.X * d2.X + v20.Y * d2.Y < 0)
                d2 = new PointF(-d2.X, -d2.Y);

            if (TryInterseccaoRetas(p0, d0, p2, d2, out controle))
                return true;

            controle = new PointF((p0.X + p2.X) / 2f, (p0.Y + p2.Y) / 2f);
            return true;
        }

        private static bool TryInterseccaoRetas(
            PointF p1,
            PointF d1,
            PointF p2,
            PointF d2,
            out PointF intersecao)
        {
            float det = d1.X * d2.Y - d1.Y * d2.X;
            if (Math.Abs(det) < 0.0001f)
            {
                intersecao = PointF.Empty;
                return false;
            }

            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float t = (dx * d2.Y - dy * d2.X) / det;

            intersecao = new PointF(
                p1.X + d1.X * t,
                p1.Y + d1.Y * t);
            return true;
        }

        private static PointF Normalizar(PointF v)
        {
            float len = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            if (len <= 0.0001f)
                return new PointF(1f, 0f);

            return new PointF(v.X / len, v.Y / len);
        }

        private int NormalizarNumeroFaixas(int num, bool temCanteiro)
        {
            int n = Math.Max(1, num);
            if (temCanteiro)
            {
                if (n < 2) n = 2;
                if ((n % 2) != 0) n += 1;
            }
            return n;
        }

        private float ObterLarguraExtras(bool temCiclofaixa, bool temEstacionamento)
        {
            float porLado = 0f;
            if (temEstacionamento)
                porLado += LarguraEstacionamentoEscalada;
            if (temCiclofaixa)
                porLado += LarguraCiclofaixaEscalada;
            return porLado * 2f;
        }

        private void AjustarLarguraParaManterFaixas(
            int numeroFaixas,
            bool temCanteiro,
            float larguraCanteiroDesejada,
            bool temCiclofaixa,
            bool temEstacionamento)
        {
            int numeroFaixasNormalizado = NormalizarNumeroFaixas(numeroFaixas, temCanteiro);
            float larguraCanteiro = temCanteiro ? Math.Max(2f, larguraCanteiroDesejada) : 0f;
            float larguraExtras = ObterLarguraExtras(temCiclofaixa, temEstacionamento);
            _largura = Math.Max(10f, LarguraFaixaPadraoEscalada * numeroFaixasNormalizado + larguraCanteiro + larguraExtras);
        }

        public void Cancelar()
        {
            _desenhando = false;
            _arrastandoComMouse = false;
            _houveArraste = false;
            _pontoInicial = null;
            _pontoSnapConexao = null;
            _objetoSnapConexao = null;
            _direcaoSnapConexaoAtual = null;
            _direcaoSnapInicial = null;
            _snapTemCanteiroAtual = false;
            _snapTemCanteiroInicial = false;
            _larguraCanteiroSnapAtual = 0f;
            _larguraCanteiroSnapInicial = 0f;
        }
    }
}
