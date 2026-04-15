// Objects/WallObject.cs
using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    public enum LadoAberturaPorta { Direita, Esquerda }
    public enum SentidoAberturaPorta { Dentro, Fora }

    public enum WallHandleType
    {
        PosicaoPorta,
        AnguloPorta,
        PosicaoJanela
    }

    [Serializable]
    public class WallObject : BaseSketchObject
    {
        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Category("Dimensões")]
        [DisplayName("Espessura da Parede (m)")]
        [Description("Espessura da parede em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float Espessura { get; set; } = 8f;

        [Category("Dimensões")]
        [DisplayName("Comprimento (m)")]
        [Description("Comprimento da parede em metros. Ao alterar, reposiciona o ponto final mantendo a direção")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float Comprimento
        {
            get
            {
                float dx = PontoFinal.X - PontoInicial.X;
                float dy = PontoFinal.Y - PontoInicial.Y;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }
            set
            {
                float novoComprimento = Math.Max(1f, value);
                float dx = PontoFinal.X - PontoInicial.X;
                float dy = PontoFinal.Y - PontoInicial.Y;
                float comp = (float)Math.Sqrt(dx * dx + dy * dy);
                if (comp < 0.001f) return;
                float nx = dx / comp;
                float ny = dy / comp;
                PontoFinal = new PointF(
                    PontoInicial.X + nx * novoComprimento,
                    PontoInicial.Y + ny * novoComprimento);
            }
        }

        [Category("Aberturas")]
        [DisplayName("Possui Porta")]
        [Description("Define se a parede possui uma porta")]
        public bool TemPorta { get; set; } = false;

        [Category("Aberturas")]
        [DisplayName("Possui Janela")]
        [Description("Define se a parede possui uma janela")]
        public bool TemJanela { get; set; } = false;

        [Category("Aberturas")]
        [DisplayName("Posição da Porta")]
        [Description("Posição relativa da porta na parede (0.0 a 1.0)")]
        public float PosicaoPorta { get; set; } = 0.35f;

        [Category("Aberturas")]
        [DisplayName("Largura da Porta (m)")]
        [Description("Largura da porta em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraPorta { get; set; } = 50f;

        [Category("Aberturas")]
        [DisplayName("Posição da Janela")]
        [Description("Posição relativa da janela na parede (0.0 a 1.0)")]
        public float PosicaoJanela { get; set; } = 0.65f;

        [Category("Aberturas")]
        [DisplayName("Largura da Janela (m)")]
        [Description("Largura da janela em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float LarguraJanela { get; set; } = 50f;

        [Category("Aberturas")]
        [DisplayName("Cor da Porta")]
        [Description("Cor usada para desenhar o arco de abertura da porta")]
        public Color CorPorta { get; set; } = Color.DarkGray;

        [Category("Aberturas")]
        [DisplayName("Mostrar Folha da Porta")]
        [Description("Quando desativado, mantém apenas o vão da porta sem desenhar folha/arco")]
        public bool MostrarFolhaPorta { get; set; } = true;

        [Category("Aberturas")]
        [DisplayName("Cor da Janela")]
        [Description("Cor usada para desenhar a janela")]
        public Color CorJanela { get; set; } = Color.LightBlue;

        [Category("Aberturas")]
        [DisplayName("Espessura da Porta (m)")]
        [Description("Espessura da linha da porta em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float EspessuraPorta { get; set; } = 3f;

        [Category("Aberturas")]
        [DisplayName("Ângulo de Abertura")]
        [Description("Ângulo de abertura da porta em graus")]
        public float AnguloAberturaPorta { get; set; } = 90f;

        [Category("Aberturas")]
        [DisplayName("Lado da Porta")]
        [Description("Direita/Esquerda: faz o flip horizontal da porta (troca a ponta da dobradiça no vão)")]
        public LadoAberturaPorta LadoAberturaPorta { get; set; } = LadoAberturaPorta.Direita;

        [Category("Aberturas")]
        [DisplayName("Sentido de Abertura")]
        [Description("Dentro/Fora: faz o flip vertical da abertura da porta")]
        public SentidoAberturaPorta SentidoAberturaPorta { get; set; } = SentidoAberturaPorta.Fora;
        public Color CorArcoPorta { get; private set; }

        public WallObject()
        {
            Tipo = "Parede";
            CorPreenchimento = Color.FromArgb(80, 80, 80);
            CorContorno = Color.Black;
            EspessuraContorno = 1f;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            using (var pen = new Pen(CorContorno, Espessura))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;

                if (TemPorta || TemJanela)
                {
                    DesenharComAbertura(g, pen);
                }
                else
                {
                    g.DrawLine(pen, PontoInicial, PontoFinal);
                }
            }

            // Preenchimento da parede (hachura)
            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comprimento = (float)Math.Sqrt(dx * dx + dy * dy);
            float angulo = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);

            using (var path = new GraphicsPath())
            {
                float halfW = Espessura / 2;

                if (TemPorta || TemJanela)
                {
                    var aberturas = new List<(float Inicio, float Fim)>();

                    if (TemPorta && TentarCalcularAbertura(PosicaoPorta, LarguraPorta, comprimento, out var inicioPorta, out var fimPorta))
                    {
                        aberturas.Add((inicioPorta, fimPorta));
                    }

                    if (TemJanela && TentarCalcularAbertura(PosicaoJanela, LarguraJanela, comprimento, out var inicioJanela, out var fimJanela))
                    {
                        aberturas.Add((inicioJanela, fimJanela));
                    }

                    if (aberturas.Count == 0)
                    {
                        path.AddRectangle(new RectangleF(0, -halfW, comprimento, Espessura));
                    }
                    else
                    {
                        aberturas.Sort((a, b) => a.Inicio.CompareTo(b.Inicio));

                        float cursor = 0f;
                        foreach (var abertura in aberturas)
                        {
                            float ini = Math.Max(0f, abertura.Inicio);
                            float fim = Math.Min(comprimento, abertura.Fim);

                            if (ini > cursor)
                            {
                                path.AddRectangle(new RectangleF(cursor, -halfW, ini - cursor, Espessura));
                            }

                            cursor = Math.Max(cursor, fim);
                        }

                        if (cursor < comprimento)
                        {
                            path.AddRectangle(new RectangleF(cursor, -halfW, comprimento - cursor, Espessura));
                        }
                    }
                }
                else
                {
                    path.AddRectangle(new RectangleF(0, -halfW, comprimento, Espessura));
                }

                var matrix = new Matrix();
                matrix.Translate(PontoInicial.X, PontoInicial.Y);
                matrix.Rotate(angulo);
                path.Transform(matrix);

                using (var brush = new HatchBrush(HatchStyle.DiagonalCross,
                    Color.FromArgb(100, CorPreenchimento),
                    Color.FromArgb(50, CorPreenchimento)))
                {
                    g.FillPath(brush, path);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharComAbertura(Graphics g, Pen pen)
        {
            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp <= 0.1f)
            {
                g.DrawLine(pen, PontoInicial, PontoFinal);
                return;
            }

            float nx = dx / comp;
            float ny = dy / comp;

            var aberturas = new List<(float Inicio, float Fim, bool Porta)>();

            if (TemPorta && TentarCalcularAbertura(PosicaoPorta, LarguraPorta, comp, out var inicioPorta, out var fimPorta))
            {
                aberturas.Add((inicioPorta, fimPorta, true));
            }

            if (TemJanela && TentarCalcularAbertura(PosicaoJanela, LarguraJanela, comp, out var inicioJanela, out var fimJanela))
            {
                aberturas.Add((inicioJanela, fimJanela, false));
            }

            if (aberturas.Count == 0)
            {
                g.DrawLine(pen, PontoInicial, PontoFinal);
                return;
            }

            aberturas.Sort((a, b) => a.Inicio.CompareTo(b.Inicio));

            float cursor = 0f;
            foreach (var abertura in aberturas)
            {
                float ini = Math.Max(0f, abertura.Inicio);
                float fim = Math.Min(comp, abertura.Fim);

                if (ini > cursor)
                {
                    var s = new PointF(PontoInicial.X + nx * cursor, PontoInicial.Y + ny * cursor);
                    var e = new PointF(PontoInicial.X + nx * ini, PontoInicial.Y + ny * ini);
                    g.DrawLine(pen, s, e);
                }

                cursor = Math.Max(cursor, fim);
            }

            if (cursor < comp)
            {
                var s = new PointF(PontoInicial.X + nx * cursor, PontoInicial.Y + ny * cursor);
                g.DrawLine(pen, s, PontoFinal);
            }

            foreach (var abertura in aberturas)
            {
                var p2 = new PointF(PontoInicial.X + nx * abertura.Inicio, PontoInicial.Y + ny * abertura.Inicio);
                var p3 = new PointF(PontoInicial.X + nx * abertura.Fim, PontoInicial.Y + ny * abertura.Fim);

                if (abertura.Porta)
                {
                    if (!MostrarFolhaPorta)
                    {
                        continue;
                    }

                    float anguloAbertura = Math.Max(5f, Math.Min(170f, AnguloAberturaPorta));
                    float doorLength = abertura.Fim - abertura.Inicio;

                    bool abreParaFora = SentidoAberturaPorta == SentidoAberturaPorta.Fora;

                    // Direita/Esquerda = escolhe a dobradiça pelo lado horizontal da tela
                    PointF pontoDobradica;
                    PointF pontoLivre;

                    if (LadoAberturaPorta == LadoAberturaPorta.Direita)
                    {
                        pontoDobradica = p2.X >= p3.X ? p2 : p3;
                    }
                    else
                    {
                        pontoDobradica = p2.X <= p3.X ? p2 : p3;
                    }

                    pontoLivre = pontoDobradica == p2 ? p3 : p2;

                    // Vetor base da folha fechada (dobradiça -> ponta livre)
                    float vx = pontoLivre.X - pontoDobradica.X;
                    float vy = pontoLivre.Y - pontoDobradica.Y;

                    // Dentro/Fora só altera o lado de giro (flip vertical)
                    // Direita/Esquerda só altera a dobradiça (flip horizontal)
                    bool dobradicaNoInicio = pontoDobradica == p2;
                    float fatorDobradica = dobradicaNoInicio ? 1f : -1f;
                    float fatorSentido = abreParaFora ? -1f : 1f;
                    float thetaGraus = fatorDobradica * fatorSentido * anguloAbertura;
                    float thetaRad = thetaGraus * (float)Math.PI / 180f;

                    float cosT = (float)Math.Cos(thetaRad);
                    float sinT = (float)Math.Sin(thetaRad);

                    float rx = vx * cosT - vy * sinT;
                    float ry = vx * sinT + vy * cosT;

                    var pFolha = new PointF(pontoDobradica.X + rx, pontoDobradica.Y + ry);

                    using (var penPorta = new Pen(CorPorta, Math.Max(1f, EspessuraPorta)))
                    {
                        penPorta.DashStyle = DashStyle.Solid;
                        g.DrawLine(penPorta, pontoDobradica, pFolha);
                    }
                    CorArcoPorta = Color.FromArgb(100, Color.Black);
                    using (var arcPen = new Pen(CorArcoPorta, Math.Max(1f, EspessuraPorta * 0.3f)))
                    {
                        arcPen.DashStyle = DashStyle.Dash;
                        var rect = new RectangleF(
                            pontoDobradica.X - doorLength, pontoDobradica.Y - doorLength,
                            2 * doorLength, 2 * doorLength);
                        float startAngle = (float)(Math.Atan2(vy, vx) * 180 / Math.PI);
                        g.DrawArc(arcPen, rect, startAngle, thetaGraus);
                    }
                }
                else
                {
                    float perpX = -ny * Espessura / 2;
                    float perpY = nx * Espessura / 2;

                    using (var penJanela = new Pen(CorJanela, 2f))
                    {
                        g.DrawLine(penJanela,
                            p2.X + perpX, p2.Y + perpY,
                            p3.X + perpX, p3.Y + perpY);
                        g.DrawLine(penJanela,
                            p2.X - perpX, p2.Y - perpY,
                            p3.X - perpX, p3.Y - perpY);
                    }
                }
            }
        }

        private static bool TentarCalcularAbertura(float posicao, float largura, float comp, out float inicio, out float fim)
        {
            float p = Math.Max(0f, Math.Min(1f, posicao));
            float larg = Math.Max(8f, Math.Min(largura, comp - 4f));

            inicio = Math.Max(2f, p * comp - larg / 2f);
            fim = Math.Min(comp - 2f, p * comp + larg / 2f);
            return fim > inicio;
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, PontoInicial, PontoFinal) <= Espessura / 2 + tolerancia;
        }

        public override RectangleF GetBounds()
        {
            float minX = Math.Min(PontoInicial.X, PontoFinal.X) - Espessura;
            float minY = Math.Min(PontoInicial.Y, PontoFinal.Y) - Espessura;
            float maxX = Math.Max(PontoInicial.X, PontoFinal.X) + Espessura;
            float maxY = Math.Max(PontoInicial.Y, PontoFinal.Y) + Espessura;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);
            Espessura = Math.Max(1f, Espessura * (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f);
            base.EscalarAoRedor(centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }

        // ── Handle support ────────────────────────────────────────────────────

        /// <summary>Returns world-space positions for all interactive handles.</summary>
        public Dictionary<WallHandleType, PointF> GetHandlesMundo()
        {
            var result = new Dictionary<WallHandleType, PointF>();

            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp < 0.1f) return result;

            float nx = dx / comp;
            float ny = dy / comp;

            if (TemPorta && TentarCalcularAbertura(PosicaoPorta, LarguraPorta, comp, out float iniP, out float fimP))
            {
                float centro = (iniP + fimP) / 2f;
                result[WallHandleType.PosicaoPorta] = new PointF(
                    PontoInicial.X + nx * centro,
                    PontoInicial.Y + ny * centro);

                if (MostrarFolhaPorta && TentarObterGeometriaPorta(comp, nx, ny,
                    out var pontoDobradica, out _,
                    out float vx, out float vy,
                    out float fatorDobradica, out float fatorSentido))
                {
                    float angAbertura = Math.Max(5f, Math.Min(170f, AnguloAberturaPorta));
                    float thetaRad = fatorDobradica * fatorSentido * angAbertura * (float)Math.PI / 180f;
                    float cosT = (float)Math.Cos(thetaRad);
                    float sinT = (float)Math.Sin(thetaRad);
                    result[WallHandleType.AnguloPorta] = new PointF(
                        pontoDobradica.X + vx * cosT - vy * sinT,
                        pontoDobradica.Y + vx * sinT + vy * cosT);
                }
            }

            if (TemJanela && TentarCalcularAbertura(PosicaoJanela, LarguraJanela, comp, out float iniJ, out float fimJ))
            {
                float centro = (iniJ + fimJ) / 2f;
                result[WallHandleType.PosicaoJanela] = new PointF(
                    PontoInicial.X + nx * centro,
                    PontoInicial.Y + ny * centro);
            }

            return result;
        }

        /// <summary>Updates the property controlled by <paramref name="tipo"/> from a world drag position.</summary>
        public bool AtualizarPorHandle(WallHandleType tipo, PointF worldPos)
        {
            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp < 0.1f) return false;

            float nx = dx / comp;
            float ny = dy / comp;

            switch (tipo)
            {
                case WallHandleType.PosicaoPorta:
                {
                    float t = ((worldPos.X - PontoInicial.X) * nx + (worldPos.Y - PontoInicial.Y) * ny) / comp;
                    PosicaoPorta = Math.Max(0.05f, Math.Min(0.95f, t));
                    return true;
                }
                case WallHandleType.PosicaoJanela:
                {
                    float t = ((worldPos.X - PontoInicial.X) * nx + (worldPos.Y - PontoInicial.Y) * ny) / comp;
                    PosicaoJanela = Math.Max(0.05f, Math.Min(0.95f, t));
                    return true;
                }
                case WallHandleType.AnguloPorta:
                {
                    if (!TentarObterGeometriaPorta(comp, nx, ny,
                        out var pontoDobradica, out _,
                        out float vx, out float vy,
                        out float fatorDobradica, out float fatorSentido))
                        return false;

                    float baseAngle = (float)Math.Atan2(vy, vx);
                    float mouseAngle = (float)Math.Atan2(
                        worldPos.Y - pontoDobradica.Y,
                        worldPos.X - pontoDobradica.X);
                    float thetaGraus = (mouseAngle - baseAngle) * 180f / (float)Math.PI;

                    while (thetaGraus > 180f) thetaGraus -= 360f;
                    while (thetaGraus < -180f) thetaGraus += 360f;

                    float anguloAbertura = thetaGraus / (fatorDobradica * fatorSentido);
                    AnguloAberturaPorta = Math.Max(5f, Math.Min(170f, anguloAbertura));
                    return true;
                }
            }
            return false;
        }

        private bool TentarObterGeometriaPorta(float comp, float nx, float ny,
            out PointF pontoDobradica, out PointF pontoLivre,
            out float vx, out float vy,
            out float fatorDobradica, out float fatorSentido)
        {
            pontoDobradica = default;
            pontoLivre = default;
            vx = vy = fatorDobradica = fatorSentido = 0;

            if (!TentarCalcularAbertura(PosicaoPorta, LarguraPorta, comp, out float ini, out float fim))
                return false;

            var p2 = new PointF(PontoInicial.X + nx * ini, PontoInicial.Y + ny * ini);
            var p3 = new PointF(PontoInicial.X + nx * fim, PontoInicial.Y + ny * fim);

            bool dobradicaEhP2 = LadoAberturaPorta == LadoAberturaPorta.Direita
                ? p2.X >= p3.X
                : p2.X <= p3.X;

            pontoDobradica = dobradicaEhP2 ? p2 : p3;
            pontoLivre = dobradicaEhP2 ? p3 : p2;
            vx = pontoLivre.X - pontoDobradica.X;
            vy = pontoLivre.Y - pontoDobradica.Y;
            fatorDobradica = dobradicaEhP2 ? 1f : -1f;
            fatorSentido = SentidoAberturaPorta == SentidoAberturaPorta.Fora ? -1f : 1f;
            return true;
        }
    }
}