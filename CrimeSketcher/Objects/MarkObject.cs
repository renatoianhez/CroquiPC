// Objects/MarkObject.cs - Objeto de Marca com Curva Bézier
using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class MarkObject : BaseSketchObject
    {
        private const int SEGMENTOS_CURVA = 30;
        private const int SEGMENTOS_BOUNDS = 20;
        private float _largura = 15f;

        #region Propriedades Básicas

        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Browsable(false)]
        public PointF? PontoCurva { get; set; } = null;

        [Category("Curvatura")]
        [DisplayName("Curva Circular")]
        [Description("Quando habilitado, a curvatura passa a ser tratada como arco circular.")]
        public bool CurvaCircular { get; set; } = false;

        [Category("Curvatura")]
        [DisplayName("Raio da Curva (m)")]
        [Description("Ajusta o raio quando a curva circular está habilitada.")]
        [TypeConverter(typeof(MetrosTransitoTypeConverter))]
        public float RaioCurva
        {
            get
            {
                if (!TemCurva || !CurvaCircular || !PontoCurva.HasValue)
                    return 0f;

                return GeometryHelper.TryGetCircunferenciaPorTresPontos(PontoInicial, PontoCurva.Value, PontoFinal, out _, out var raio)
                    ? raio
                    : 0f;
            }
            set
            {
                if (value <= 0f)
                    return;

                if (!TemCurva)
                    TemCurva = true;

                var referencia = PontoCurva ?? ObterReferenciaCurvaCircular();
                float? sweepPreferido = null;
                if (CurvaCircular && PontoCurva.HasValue && GeometryHelper.TryGetArcoCircular(
                    PontoInicial,
                    PontoCurva.Value,
                    PontoFinal,
                    out _,
                    out _,
                    out _,
                    out var varreduraAtual))
                {
                    sweepPreferido = varreduraAtual;
                }

                if (GeometryHelper.TryGetPontoCurvaArcoPorRaio(PontoInicial, PontoFinal, value, referencia, sweepPreferido, out var pontoCurva))
                {
                    PontoCurva = pontoCurva;
                    CurvaCircular = true;
                }
            }
        }

        [Category("Curvatura")]
        [DisplayName("Tem Curva")]
        [Description("Define se a marca possui curvatura")]
        public bool TemCurva
        {
            get => PontoCurva.HasValue;
            set
            {
                if (value && !PontoCurva.HasValue)
                {
                    PontoCurva = ObterReferenciaCurvaCircular();
                }
                else if (!value)
                {
                    PontoCurva = null;
                    CurvaCircular = false;
                }
            }
        }

        #endregion

        #region Características da Marca

        [Category("Tipo")]
        [DisplayName("Tipo de Marca")]
        [Description("Tipo de marca a ser representada")]
        public TipoMarca TipoMarca { get; set; } = TipoMarca.Frenagem;

        [Category("Aparência")]
        [DisplayName("Largura (m)")]
        [Description("Largura da marca em metros")]
        [TypeConverter(typeof(MetrosTransitoTypeConverter))]
        public float Largura
        {
            get => _largura;
            set => _largura = Math.Max(2f, value);
        }

        [Category("Aparência")]
        [DisplayName("Intensidade")]
        [Description("Intensidade da marca")]
        public IntensidadeMarca Intensidade { get; set; } = IntensidadeMarca.Media;

        [Category("Identificação")]
        [DisplayName("Descrição")]
        [Description("Descrição da marca (ex: 'Marca de frenagem do veículo 1')")]
        public string Descricao { get; set; } = "";

        [Category("Aparência")]
        [DisplayName("Mostrar Descrição")]
        [Description("Exibe a descrição ao lado da marca")]
        public bool MostrarDescricao { get; set; } = true;

        [Browsable(false)]
        public int CorMarcaArgb { get; set; } = Color.FromArgb(40, 40, 40).ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor da Marca")]
        [Description("Cor principal da marca")]
        [JsonIgnore]
        public Color CorMarca
        {
            get => Color.FromArgb(CorMarcaArgb);
            set => CorMarcaArgb = value.ToArgb();
        }

        #endregion

        #region Propriedades Calculadas

        [Category("Dimensões")]
        [DisplayName("Comprimento (m)")]
        [Description("Comprimento total da marca em metros")]
        [ReadOnly(true)]
        [JsonIgnore]
        [TypeConverter(typeof(MetrosTransitoTypeConverter))]
        public float Comprimento
        {
            get
            {
                if (!TemCurva || !PontoCurva.HasValue)
                {
                    float dx = PontoFinal.X - PontoInicial.X;
                    float dy = PontoFinal.Y - PontoInicial.Y;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                else
                {
                    // Aproximar comprimento da curva
                    float comprimento = 0;
                    int segmentos = SEGMENTOS_CURVA;
                    for (int i = 0; i < segmentos; i++)
                    {
                        float t1 = i / (float)segmentos;
                        float t2 = (i + 1) / (float)segmentos;
                        var p1 = GetPontoNaCurva(t1);
                        var p2 = GetPontoNaCurva(t2);
                        float dx = p2.X - p1.X;
                        float dy = p2.Y - p1.Y;
                        comprimento += (float)Math.Sqrt(dx * dx + dy * dy);
                    }
                    return comprimento;
                }
            }
        }

        #endregion

        public MarkObject()
        {
            Tipo = "Marca";
            CorMarca = Color.FromArgb(40, 40, 40);
        }

        #region Métodos de Curva Bézier

        private PointF GetPontoNaCurva(float t)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                return new PointF(
                    PontoInicial.X + (PontoFinal.X - PontoInicial.X) * t,
                    PontoInicial.Y + (PontoFinal.Y - PontoInicial.Y) * t);
            }

            if (CurvaCircular && GeometryHelper.TryGetArcoCircular(
                PontoInicial,
                PontoCurva.Value,
                PontoFinal,
                out var centro,
                out var raio,
                out var anguloInicial,
                out var varredura))
            {
                return GeometryHelper.ObterPontoArcoCircular(centro, raio, anguloInicial, varredura, t);
            }

            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ut2 = 2 * u * t;

            PointF p = new PointF(
                uu * PontoInicial.X + ut2 * PontoCurva.Value.X + tt * PontoFinal.X,
                uu * PontoInicial.Y + ut2 * PontoCurva.Value.Y + tt * PontoFinal.Y);

            return p;
        }

        private PointF GetTangenteNaCurva(float t)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                float deltaX = PontoFinal.X - PontoInicial.X;
                float deltaY = PontoFinal.Y - PontoInicial.Y;
                float length = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                return length > 0.0001f ? new PointF(deltaX / length, deltaY / length) : new PointF(1, 0);
            }

            if (CurvaCircular && GeometryHelper.TryGetArcoCircular(
                PontoInicial,
                PontoCurva.Value,
                PontoFinal,
                out _,
                out _,
                out var anguloInicial,
                out var varredura))
            {
                float angulo = anguloInicial + varredura * t;
                return GeometryHelper.ObterTangenteArcoCircular(angulo, varredura >= 0f);
            }

            float u = 1 - t;
            float tangDx = 2 * u * (PontoCurva.Value.X - PontoInicial.X) + 2 * t * (PontoFinal.X - PontoCurva.Value.X);
            float tangDy = 2 * u * (PontoCurva.Value.Y - PontoInicial.Y) + 2 * t * (PontoFinal.Y - PontoCurva.Value.Y);

            float tangLen = (float)Math.Sqrt(tangDx * tangDx + tangDy * tangDy);
            if (tangLen > 0)
            {
                return new PointF(tangDx / tangLen, tangDy / tangLen);
            }
            return new PointF(1, 0);
        }

        private PointF GetPerpendicularNaCurva(float t)
        {
            var tangente = GetTangenteNaCurva(t);
            return new PointF(-tangente.Y, tangente.X);
        }

        public bool ContemPontoCurva(PointF ponto, float tolerancia = 10f)
        {
            if (!TemCurva || !PontoCurva.HasValue) return false;

            float dx = ponto.X - PontoCurva.Value.X;
            float dy = ponto.Y - PontoCurva.Value.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            return dist <= tolerancia;
        }

        public void MoverPontoCurva(PointF novaPosicao, bool curvaCircular = false)
        {
            if (TemCurva)
            {
                PontoCurva = novaPosicao;
                CurvaCircular = curvaCircular;
            }
        }

        private PointF ObterReferenciaCurvaCircular()
        {
            var meio = GeometryHelper.PontoMedio(PontoInicial, PontoFinal);
            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len <= 0.0001f)
                return meio;

            float offset = Math.Max(len / 4f, 1f);
            return new PointF(meio.X - dy / len * offset, meio.Y + dx / len * offset);
        }

        #endregion

        #region Desenho

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            float comp = Comprimento;
            if (comp < 1) return;

            // Aplicar opacidade baseada na intensidade
            float opacidadeBase = Opacidade;
            switch (Intensidade)
            {
                case IntensidadeMarca.Leve:
                    opacidadeBase *= 0.4f;
                    break;
                case IntensidadeMarca.Media:
                    opacidadeBase *= 0.7f;
                    break;
                case IntensidadeMarca.Forte:
                    opacidadeBase *= 0.9f;
                    break;
                case IntensidadeMarca.MuitoForte:
                    opacidadeBase *= 1.0f;
                    break;
            }

            // Desenhar marca baseado no tipo
            switch (TipoMarca)
            {
                case TipoMarca.Frenagem:
                    DesenharMarcaFrenagem(g, opacidadeBase);
                    break;
                case TipoMarca.Derrapagem:
                    DesenharMarcaDerrapagem(g, opacidadeBase);
                    break;
                case TipoMarca.Sulco:
                    DesenharMarcaSulco(g, opacidadeBase);
                    break;
                case TipoMarca.Arranhao:
                    DesenharMarcaArranhao(g, opacidadeBase);
                    break;
                case TipoMarca.Rastro:
                    DesenharMarcaRastro(g, opacidadeBase);
                    break;
                case TipoMarca.Impacto:
                    DesenharMarcaImpacto(g, opacidadeBase);
                    break;
                case TipoMarca.Personalizada:
                    DesenharMarcaPersonalizada(g, opacidadeBase);
                    break;
                case TipoMarca.Risco:
                    DesenharMarcaRisco(g, opacidadeBase);
                    break;
                case TipoMarca.Cerca:
                    DesenharMarcaCerca(g, opacidadeBase);
                    break;
                case TipoMarca.Muro:
                    DesenharMarcaMuro(g, opacidadeBase);
                    break;
                case TipoMarca.Canaleta:
                    DesenharMarcaCanaleta(g, opacidadeBase);
                    break;
                case TipoMarca.MeioFio:
                    DesenharMarcaMeioFio(g, opacidadeBase);
                    break;
            }

            // Descrição
            if (MostrarDescricao && !string.IsNullOrEmpty(Descricao))
            {
                DesenharDescricao(g);
            }

            // Seleção
            if (Selecionado)
            {
                DesenharSelecao(g);
                DesenharPontosControle(g);
            }
        }

        private void DesenharMarcaFrenagem(Graphics g, float opacidade)
        {
            // Marca de frenagem: linhas paralelas escuras
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);

            int numLinhas = 3; // 3 linhas paralelas
            float espacamento = Largura / (numLinhas + 1);

            for (int i = 0; i < numLinhas; i++)
            {
                float offset = -Largura / 2 + (i + 1) * espacamento;
                DesenharLinhaParalela(g, offset, corComAlpha, 2f, DashStyle.Solid);
            }
        }

        private void DesenharMarcaDerrapagem(Graphics g, float opacidade)
        {
            // Marca de derrapagem: faixa larga com gradiente
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);

            List<PointF> pontosSuperiores = new List<PointF>();
            List<PointF> pontosInferiores = new List<PointF>();

            int segmentos = SEGMENTOS_CURVA;
            for (int i = 0; i <= segmentos; i++)
            {
                float t = i / (float)segmentos;
                var ponto = GetPontoNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);
                float halfW = Largura / 2;

                pontosSuperiores.Add(new PointF(
                    ponto.X + perp.X * halfW,
                    ponto.Y + perp.Y * halfW));
                pontosInferiores.Add(new PointF(
                    ponto.X - perp.X * halfW,
                    ponto.Y - perp.Y * halfW));
            }

            using (var path = new GraphicsPath())
            {
                path.AddLines(pontosSuperiores.ToArray());
                pontosInferiores.Reverse();
                path.AddLines(pontosInferiores.ToArray());
                path.CloseFigure();

                using (var brush = new SolidBrush(corComAlpha))
                {
                    g.FillPath(brush, path);
                }

                // Textura de derrapagem (linhas irregulares)
                using (var pen = new Pen(Color.FromArgb((int)(128 * opacidade), CorMarca), 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    for (int i = 0; i < segmentos; i += 2)
                    {
                        if (i < pontosSuperiores.Count - 2)
                        {
                            g.DrawLine(pen, pontosSuperiores[i], pontosInferiores[i]);
                        }
                    }
                }
            }
        }

        private void DesenharMarcaSulco(Graphics g, float opacidade)
        {
            // Sulco: linha escura e profunda com bordas
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);
            Color corBorda = Color.FromArgb((int)(180 * opacidade), CorMarca);

            // Linha central (sulco principal)
            DesenharLinhaParalela(g, 0, corComAlpha, Largura, DashStyle.Solid);

            // Bordas do sulco
            DesenharLinhaParalela(g, -Largura / 2, corBorda, 1.5f, DashStyle.Solid);
            DesenharLinhaParalela(g, Largura / 2, corBorda, 1.5f, DashStyle.Solid);
        }

        private void DesenharMarcaArranhao(Graphics g, float opacidade)
        {
            // Arranhão: linha fina e irregular
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);

            using (var pen = new Pen(corComAlpha, Math.Max(1f, Largura / 3)))
            {
                pen.DashStyle = DashStyle.Solid;
                DesenharLinhaCentral(g, pen);
            }

            // Adicionar irregularidade (pequenos riscos)
            using (var pen = new Pen(Color.FromArgb((int)(128 * opacidade), CorMarca), 0.5f))
            {
                Random rnd = new Random(PontoInicial.GetHashCode());
                int segmentos = SEGMENTOS_BOUNDS;
                for (int i = 0; i < segmentos; i++)
                {
                    if (rnd.Next(100) < 40) // 40% de chance
                    {
                        float t = i / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perp = GetPerpendicularNaCurva(t);
                        float offset = (float)rnd.NextDouble() * Largura - Largura / 2;

                        var p1 = new PointF(ponto.X + perp.X * offset, ponto.Y + perp.Y * offset);
                        var p2 = new PointF(p1.X + perp.X * 2, p1.Y + perp.Y * 2);
                        g.DrawLine(pen, p1, p2);
                    }
                }
            }
        }

        private void DesenharMarcaRastro(Graphics g, float opacidade)
        {
            // Rastro: marca de arrasto com textura
            Color corComAlpha = Color.FromArgb((int)(200 * opacidade), CorMarca);

            List<PointF> pontosSuperiores = new List<PointF>();
            List<PointF> pontosInferiores = new List<PointF>();

            int segmentos = SEGMENTOS_CURVA;
            for (int i = 0; i <= segmentos; i++)
            {
                float t = i / (float)segmentos;
                var ponto = GetPontoNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);
                float halfW = Largura / 2;

                pontosSuperiores.Add(new PointF(ponto.X + perp.X * halfW, ponto.Y + perp.Y * halfW));
                pontosInferiores.Add(new PointF(ponto.X - perp.X * halfW, ponto.Y - perp.Y * halfW));
            }

            using (var path = new GraphicsPath())
            {
                path.AddLines(pontosSuperiores.ToArray());
                pontosInferiores.Reverse();
                path.AddLines(pontosInferiores.ToArray());
                path.CloseFigure();

                using (var brush = new HatchBrush(HatchStyle.DarkDownwardDiagonal,
                    corComAlpha, Color.FromArgb((int)(50 * opacidade), CorMarca)))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DesenharMarcaImpacto(Graphics g, float opacidade)
        {
            // Marca de impacto: padrão irregular e disperso
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);

            // Base da marca
            DesenharLinhaParalela(g, 0, Color.FromArgb((int)(150 * opacidade), CorMarca), Largura * 1.5f, DashStyle.Solid);

            // Marcas irregulares ao redor
            Random rnd = new Random(PontoInicial.GetHashCode());
            int segmentos = 15;
            for (int i = 0; i < segmentos; i++)
            {
                float t = i / (float)segmentos;
                var ponto = GetPontoNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);

                int numMarcas = rnd.Next(2, 5);
                for (int j = 0; j < numMarcas; j++)
                {
                    float offset = (float)rnd.NextDouble() * Largura * 2 - Largura;
                    float tamanho = (float)rnd.NextDouble() * 5 + 2;

                    var centro = new PointF(ponto.X + perp.X * offset, ponto.Y + perp.Y * offset);
                    using (var brush = new SolidBrush(Color.FromArgb((int)(rnd.Next(100, 200) * opacidade), CorMarca)))
                    {
                        g.FillEllipse(brush, centro.X - tamanho / 2, centro.Y - tamanho / 2, tamanho, tamanho);
                    }
                }
            }
        }

        private void DesenharMarcaPersonalizada(Graphics g, float opacidade)
        {
            // Marca personalizada: linha simples
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);
            DesenharLinhaParalela(g, 0, corComAlpha, Largura, DashStyle.Solid);
        }

        private void DesenharMarcaRisco(Graphics g, float opacidade)
        {
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);
            DesenharLinhaParalela(g, 0f, corComAlpha, Math.Max(1f, Largura / 6f), DashStyle.Solid);
        }

        private void DesenharMarcaCerca(Graphics g, float opacidade)
        {
            Color corPrincipal = Color.FromArgb((int)(240 * opacidade), CorMarca);
            Color corSecundaria = Color.FromArgb((int)(200 * opacidade), CorMarca);

            DesenharLinhaParalela(g, 0f, corPrincipal, Math.Max(1.5f, Largura / 8f), DashStyle.Dash);
            float larguraX = Math.Max(4f, Largura / 4f);
            int segmentos = Math.Max(4, (int)(Comprimento / 40f));
            using var penX = new Pen(corSecundaria, Math.Max(1.2f, Largura / 10f));

            for (int i = 1; i < segmentos; i++)
            {
                float t = i / (float)segmentos;
                var centro = GetPontoNaCurva(t);
                var tang = GetTangenteNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);

                var p1 = new PointF(centro.X + tang.X * larguraX + perp.X * larguraX, centro.Y + tang.Y * larguraX + perp.Y * larguraX);
                var p2 = new PointF(centro.X - tang.X * larguraX - perp.X * larguraX, centro.Y - tang.Y * larguraX - perp.Y * larguraX);
                var p3 = new PointF(centro.X + tang.X * larguraX - perp.X * larguraX, centro.Y + tang.Y * larguraX - perp.Y * larguraX);
                var p4 = new PointF(centro.X - tang.X * larguraX + perp.X * larguraX, centro.Y - tang.Y * larguraX + perp.Y * larguraX);

                g.DrawLine(penX, p1, p2);
                g.DrawLine(penX, p3, p4);
            }
        }

        private void DesenharMarcaMuro(Graphics g, float opacidade)
        {
            Color corBase = Color.FromArgb((int)(220 * opacidade), CorMarca);
            Color corJunta = Color.FromArgb((int)(250 * opacidade), ClarearCor(CorMarca, 40));

            float espessuraFaixa = Math.Max(5f, Largura * 0.8f);
            float halfW = espessuraFaixa / 2f;

            DesenharLinhaParalela(g, 0f, corBase, espessuraFaixa, DashStyle.Solid);
            DesenharLinhaParalela(g, -halfW, corJunta, 1.4f, DashStyle.Solid);
            DesenharLinhaParalela(g, halfW, corJunta, 1.4f, DashStyle.Solid);

            using var penJunta = new Pen(corJunta, 1.2f);
            int segmentos = Math.Max(6, (int)(Comprimento / 16f));
            for (int i = 1; i < segmentos; i++)
            {
                float t = i / (float)segmentos;
                var centro = GetPontoNaCurva(t);
                var perp = GetPerpendicularNaCurva(t);

                var p1 = new PointF(centro.X - perp.X * halfW, centro.Y - perp.Y * halfW);
                var p2 = new PointF(centro.X + perp.X * halfW, centro.Y + perp.Y * halfW);
                g.DrawLine(penJunta, p1, p2);
            }
        }

        private void DesenharMarcaCanaleta(Graphics g, float opacidade)
        {
            Color corBase = Color.FromArgb((int)(255 * opacidade), CorMarca);
            Color corClara = ClarearCor(CorMarca, 100);

            float espessuraBase = Math.Max(6f, Largura);
            DesenharLinhaParalela(g, 0f, corBase, espessuraBase, DashStyle.Solid);

            // Gradiente interno TRANSVERSAL: mais claro no centro e mais escuro nas bordas
            int amostras = 8;
            float half = espessuraBase / 2f;
            float passo = half / amostras;
            float espFaixa = Math.Max(1f, passo * 1.4f);

            for (int i = -amostras; i <= amostras; i++)
            {
                float offset = i * passo;
                float fatorCentro = 1f - (Math.Abs(offset) / half);
                fatorCentro = Math.Clamp(fatorCentro, 0f, 1f);

                int alpha = (int)(fatorCentro * 190f * opacidade);
                if (alpha <= 0) continue;

                Color corFaixa = Color.FromArgb(alpha, corClara);
                DesenharLinhaParalela(g, offset, corFaixa, espFaixa, DashStyle.Solid);
            }
        }

        private void DesenharMarcaMeioFio(Graphics g, float opacidade)
        {
            Color corComAlpha = Color.FromArgb((int)(255 * opacidade), CorMarca);
            DesenharLinhaParalela(g, 0f, corComAlpha, 2.5f, DashStyle.Solid);
        }

        private static Color ClarearCor(Color cor, int acrescimo)
        {
            return Color.FromArgb(
                Math.Min(255, cor.R + acrescimo),
                Math.Min(255, cor.G + acrescimo),
                Math.Min(255, cor.B + acrescimo));
        }

        private void DesenharLinhaCentral(Graphics g, Pen pen)
        {
            if (!TemCurva || !PontoCurva.HasValue)
            {
                g.DrawLine(pen, PontoInicial, PontoFinal);
            }
            else
            {
                List<PointF> pontos = new List<PointF>();
                int segmentos = SEGMENTOS_CURVA;
                for (int i = 0; i <= segmentos; i++)
                {
                    float t = i / (float)segmentos;
                    pontos.Add(GetPontoNaCurva(t));
                }
                g.DrawLines(pen, pontos.ToArray());
            }
        }

        private void DesenharLinhaParalela(Graphics g, float offset, Color cor, float espessura, DashStyle estilo)
        {
            using (var pen = new Pen(cor, espessura))
            {
                pen.DashStyle = estilo;

                if (!TemCurva || !PontoCurva.HasValue)
                {
                    var dir = GetTangenteNaCurva(0);
                    var perp = new PointF(-dir.Y, dir.X);

                    var p1 = new PointF(PontoInicial.X + perp.X * offset, PontoInicial.Y + perp.Y * offset);
                    var p2 = new PointF(PontoFinal.X + perp.X * offset, PontoFinal.Y + perp.Y * offset);
                    g.DrawLine(pen, p1, p2);
                }
                else
                {
                    List<PointF> pontos = new List<PointF>();
                    int segmentos = SEGMENTOS_CURVA;
                    for (int i = 0; i <= segmentos; i++)
                    {
                        float t = i / (float)segmentos;
                        var ponto = GetPontoNaCurva(t);
                        var perp = GetPerpendicularNaCurva(t);
                        pontos.Add(new PointF(ponto.X + perp.X * offset, ponto.Y + perp.Y * offset));
                    }
                    g.DrawLines(pen, pontos.ToArray());
                }
            }
        }

        private void DesenharDescricao(Graphics g)
        {
            var centro = GetPontoNaCurva(0.5f);

            using (var font = new Font("Segoe UI", 8f, FontStyle.Italic))
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                var size = g.MeasureString(Descricao, font);

                // Fundo semi-transparente
                using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 200)))
                {
                    g.FillRectangle(brush,
                        centro.X - size.Width / 2 - 3,
                        centro.Y - Largura - size.Height - 5,
                        size.Width + 6, size.Height + 4);
                }

                // Texto
                using (var brush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                {
                    g.DrawString(Descricao, font, brush,
                        centro.X, centro.Y - Largura - size.Height / 2 - 3, sf);
                }
            }
        }

        private void DesenharPontosControle(Graphics g)
        {
            var elements = g.Transform.Elements;
            float zoomX = (float)Math.Sqrt(elements[0] * elements[0] + elements[1] * elements[1]);
            float zoomY = (float)Math.Sqrt(elements[2] * elements[2] + elements[3] * elements[3]);
            float zoom = Math.Max(0.0001f, (zoomX + zoomY) * 0.5f);

            float radius = 6f / zoom;

            using var penContornoBranco = new Pen(Color.White, 1f / zoom);

            // Ponto inicial
            using (var brush = new SolidBrush(Color.Orange))
            {
                g.FillEllipse(brush,
                    PontoInicial.X - radius, PontoInicial.Y - radius,
                    radius * 2, radius * 2);
            }
            g.DrawEllipse(penContornoBranco,
                PontoInicial.X - radius, PontoInicial.Y - radius,
                radius * 2, radius * 2);

            // Ponto final
            using (var brush = new SolidBrush(Color.Orange))
            {
                g.FillEllipse(brush,
                    PontoFinal.X - radius, PontoFinal.Y - radius,
                    radius * 2, radius * 2);
            }
            g.DrawEllipse(penContornoBranco,
                PontoFinal.X - radius, PontoFinal.Y - radius,
                radius * 2, radius * 2);

            // Ponto de controle de curva
            if (TemCurva && PontoCurva.HasValue)
            {
                var pc = PontoCurva.Value;

                // Linhas de controle tracejadas
                using (var pen = new Pen(Color.DodgerBlue, 1f / zoom))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLine(pen, PontoInicial, pc);
                    g.DrawLine(pen, pc, PontoFinal);
                }

                // Ponto de controle (diamante)
                float curveRadius = 7f / zoom;
                PointF[] diamond = new PointF[]
                {
                    new PointF(pc.X, pc.Y - curveRadius),
                    new PointF(pc.X + curveRadius, pc.Y),
                    new PointF(pc.X, pc.Y + curveRadius),
                    new PointF(pc.X - curveRadius, pc.Y)
                };

                using (var brush = new SolidBrush(Color.Cyan))
                {
                    g.FillPolygon(brush, diamond);
                }
                using (var pen = new Pen(Color.DodgerBlue, 2f / zoom))
                {
                    g.DrawPolygon(pen, diamond);
                }
            }
        }

        #endregion

        #region Hit Test e Bounds

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            int segmentos = SEGMENTOS_CURVA;
            for (int i = 0; i < segmentos; i++)
            {
                float t1 = i / (float)segmentos;
                float t2 = (i + 1) / (float)segmentos;
                var p1 = GetPontoNaCurva(t1);
                var p2 = GetPontoNaCurva(t2);

                if (GeometryHelper.DistanciaPontoSegmento(ponto, p1, p2) <= Largura / 2 + tolerancia)
                    return true;
            }

            return false;
        }

        public override RectangleF GetBounds()
        {
            float margin = Math.Max(3f, Largura / 2f + 2f);

            if (!TemCurva || !PontoCurva.HasValue)
            {
                float minX = Math.Min(PontoInicial.X, PontoFinal.X) - margin;
                float minY = Math.Min(PontoInicial.Y, PontoFinal.Y) - margin;
                float maxX = Math.Max(PontoInicial.X, PontoFinal.X) + margin;
                float maxY = Math.Max(PontoInicial.Y, PontoFinal.Y) + margin;
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
            else
            {
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                int segmentos = SEGMENTOS_BOUNDS;
                for (int i = 0; i <= segmentos; i++)
                {
                    float t = i / (float)segmentos;
                    var ponto = GetPontoNaCurva(t);

                    minX = Math.Min(minX, ponto.X);
                    minY = Math.Min(minY, ponto.Y);
                    maxX = Math.Max(maxX, ponto.X);
                    maxY = Math.Max(maxY, ponto.Y);
                }

                return new RectangleF(minX - margin, minY - margin,
                    maxX - minX + 2 * margin, maxY - minY + 2 * margin);
            }
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy); // Atualizar posição também
            if (PontoCurva.HasValue)
            {
                PontoCurva = new PointF(PontoCurva.Value.X + dx, PontoCurva.Value.Y + dy);
            }
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
            if (PontoCurva.HasValue)
            {
                PontoCurva = EscalarPonto(PontoCurva.Value, centro, fatorX, fatorY);
            }

            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            Largura = Math.Max(2f, Largura * media);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            if (PontoCurva.HasValue)
            {
                PontoCurva = RotacionarPonto(PontoCurva.Value, centro, deltaGraus);
            }

            Rotacao += deltaGraus;
        }

        #endregion
    }
}
