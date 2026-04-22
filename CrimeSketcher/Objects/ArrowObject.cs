// Objects/ArrowObject.cs
using CrimeSketcher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    public enum TipoLinhaSeta
    {
        Solida,
        Tracejada,
        Pontilhada,
        TracoPonto
    }

    public enum TipoPontaSeta
    {
        Fechada,
        Aberta
    }

    [Serializable]
    public class ArrowObject : BaseSketchObject
    {
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
        [TypeConverter(typeof(MetrosTypeConverter))]
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
                if (GeometryHelper.TryGetPontoCurvaArcoPorRaio(PontoInicial, PontoFinal, value, referencia, out var pontoCurva))
                {
                    PontoCurva = pontoCurva;
                    CurvaCircular = true;
                }
            }
        }

        [Category("Curvatura")]
        [DisplayName("Tem Curva")]
        [Description("Define se a seta possui curvatura")]
        public bool TemCurva
        {
            get => PontoCurva.HasValue;
            set
            {
                if (value && !PontoCurva.HasValue)
                {
                    PontoCurva = new PointF(
                        (PontoInicial.X + PontoFinal.X) / 2f,
                        (PontoInicial.Y + PontoFinal.Y) / 2f);
                }
                else if (!value)
                {
                    PontoCurva = null;
                    CurvaCircular = false;
                }
            }
        }

        [Category("Aparência")]
        [DisplayName("Tamanho da Seta (m)")]
        [Description("Tamanho da ponta da seta em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float TamanhoSeta { get; set; } = 12f;

        [Category("Aparência")]
        [DisplayName("Tipo de Linha")]
        [Description("Define o estilo da linha da seta")]
        public TipoLinhaSeta TipoLinha { get; set; } = TipoLinhaSeta.Solida;

        [Category("Aparência")]
        [DisplayName("Tipo de Ponta")]
        [Description("Define o tipo da ponta da seta")]
        public TipoPontaSeta TipoPonta { get; set; } = TipoPontaSeta.Fechada;

        [Category("Aparência")]
        [DisplayName("Seta no Início")]
        [Description("Desenha uma seta no ponto inicial")]
        public bool SetaInicio { get; set; } = false;

        [Category("Aparência")]
        [DisplayName("Seta no Fim")]
        [Description("Desenha uma seta no ponto final")]
        public bool SetaFim { get; set; } = true;

        [Category("Texto")]
        [DisplayName("Rótulo")]
        [Description("Texto descritivo para a seta (ex: Norte, Entrada)")]
        public string Rotulo { get; set; } = "";

        [Category("Texto")]
        [DisplayName("Fonte do Rótulo")]
        [Description("Nome da família da fonte do rótulo")]
        [TypeConverter(typeof(FontConverter.FontNameConverter))]
        public string FonteRotulo { get; set; } = "Segoe UI";

        [Category("Texto")]
        [DisplayName("Tamanho da Fonte do Rótulo")]
        [Description("Tamanho da fonte do rótulo em pontos")]
        public float TamanhoFonteRotulo { get; set; } = 8f;

        [Browsable(false)]
        public int CorRotuloArgb { get; set; } = Color.DarkBlue.ToArgb();

        [Category("Texto")]
        [DisplayName("Cor do Rótulo")]
        [Description("Cor do texto do rótulo")]
        [JsonIgnore]
        public Color CorRotulo
        {
            get => Color.FromArgb(CorRotuloArgb);
            set => CorRotuloArgb = value.ToArgb();
        }

        public ArrowObject()
        {
            Tipo = "Seta";
            CorContorno = Color.DarkBlue;
            EspessuraContorno = 2f;
            CorRotulo = CorContorno;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            ObterPontosMundo(out var pontoInicial, out var pontoFinal, out var pontoCurva);

            using (var pen = new Pen(CorContorno, EspessuraContorno))
            {
                pen.DashStyle = TipoLinha switch
                {
                    TipoLinhaSeta.Tracejada => DashStyle.Dash,
                    TipoLinhaSeta.Pontilhada => DashStyle.Dot,
                    TipoLinhaSeta.TracoPonto => DashStyle.DashDot,
                    _ => DashStyle.Solid
                };

                bool pontaFechada = TipoPonta == TipoPontaSeta.Fechada;

                if (SetaFim)
                {
                    pen.CustomEndCap = new AdjustableArrowCap(
                        TamanhoSeta / 2, TamanhoSeta / 2, pontaFechada);
                }
                if (SetaInicio)
                {
                    pen.CustomStartCap = new AdjustableArrowCap(
                        TamanhoSeta / 2, TamanhoSeta / 2, pontaFechada);
                }

                using (var path = CriarCaminhoSeta(pontoInicial, pontoFinal, pontoCurva))
                {
                    g.DrawPath(pen, path);
                }
            }

            if (!string.IsNullOrEmpty(Rotulo))
            {
                var centro = ObterPontoRotulo(pontoInicial, pontoFinal, pontoCurva);

                string nomeFonte = string.IsNullOrWhiteSpace(FonteRotulo) ? "Segoe UI" : FonteRotulo.Trim();
                float tamanhoFonte = Math.Max(6f, TamanhoFonteRotulo);

                using (var font = new Font(nomeFonte, tamanhoFonte))
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    var size = g.MeasureString(Rotulo, font);
                    using (var bg = new SolidBrush(
                        Color.FromArgb(200, 255, 255, 255)))
                    {
                        g.FillRectangle(bg,
                            centro.X - size.Width / 2 - 2,
                            centro.Y - size.Height / 2 - 1,
                            size.Width + 4, size.Height + 2);
                    }

                    using (var brushTexto = new SolidBrush(CorRotulo))
                    {
                        g.DrawString(Rotulo, font, brushTexto, centro, sf);
                    }
                }
            }

            if (Selecionado)
            {
                if (TemCurva && pontoCurva.HasValue)
                {
                    DesenharPontoCurva(g, pontoInicial, pontoFinal, pontoCurva.Value);
                }

                DesenharSelecao(g);
            }
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            ObterPontosMundo(out var pontoInicial, out var pontoFinal, out var pontoCurva);

            using (var path = CriarCaminhoSeta(pontoInicial, pontoFinal, pontoCurva))
            using (var pen = new Pen(Color.Black, EspessuraContorno + tolerancia + 5f))
            {
                return path.IsOutlineVisible(ponto, pen);
            }
        }

        public override RectangleF GetBounds()
        {
            ObterPontosMundo(out var pontoInicial, out var pontoFinal, out var pontoCurva);

            using (var path = CriarCaminhoSeta(pontoInicial, pontoFinal, pontoCurva))
            {
                var bounds = path.GetBounds();
                float margem = Math.Max(2f, Math.Max(EspessuraContorno, TamanhoSeta * 0.5f));
                bounds.Inflate(margem, margem);
                return bounds;
            }
        }

        public bool ContemPontoCurva(PointF ponto, float tolerancia = 10f)
        {
            if (!TemCurva || !PontoCurva.HasValue) return false;

            ObterPontosMundo(out _, out _, out var pontoCurva);
            if (!pontoCurva.HasValue) return false;

            float dx = ponto.X - pontoCurva.Value.X;
            float dy = ponto.Y - pontoCurva.Value.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= tolerancia;
        }

        public void MoverPontoCurva(PointF novaPosicao, bool curvaCircular = false)
        {
            if (TemCurva)
            {
                PontoCurva = novaPosicao;
                CurvaCircular = curvaCircular;
            }
        }

        private void DesenharPontoCurva(Graphics g, PointF pontoInicial, PointF pontoFinal, PointF pontoCurva)
        {
            var elements = g.Transform.Elements;
            float zoomX = (float)Math.Sqrt(elements[0] * elements[0] + elements[1] * elements[1]);
            float zoomY = (float)Math.Sqrt(elements[2] * elements[2] + elements[3] * elements[3]);
            float zoom = Math.Max(0.0001f, (zoomX + zoomY) * 0.5f);

            using (var pen = new Pen(Color.DodgerBlue, 1f / zoom))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, pontoInicial, pontoCurva);
                g.DrawLine(pen, pontoCurva, pontoFinal);
            }

            float curveRadius = 7f / zoom;
            PointF[] diamond =
            {
                new PointF(pontoCurva.X, pontoCurva.Y - curveRadius),
                new PointF(pontoCurva.X + curveRadius, pontoCurva.Y),
                new PointF(pontoCurva.X, pontoCurva.Y + curveRadius),
                new PointF(pontoCurva.X - curveRadius, pontoCurva.Y)
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

        private GraphicsPath CriarCaminhoSeta(PointF pontoInicial, PointF pontoFinal, PointF? pontoCurva)
        {
            var path = new GraphicsPath();

            if (!TemCurva || !pontoCurva.HasValue)
            {
                path.AddLine(pontoInicial, pontoFinal);
                return path;
            }

            if (CurvaCircular && GeometryHelper.TryGetArcoCircular(
                pontoInicial,
                pontoCurva.Value,
                pontoFinal,
                out var centro,
                out var raio,
                out var anguloInicial,
                out var varredura))
            {
                var pontos = new List<PointF>();
                const int segmentos = 30;
                for (int i = 0; i <= segmentos; i++)
                {
                    pontos.Add(GeometryHelper.ObterPontoArcoCircular(
                        centro,
                        raio,
                        anguloInicial,
                        varredura,
                        i / (float)segmentos));
                }

                if (pontos.Count > 1)
                    path.AddLines(pontos.ToArray());

                return path;
            }

            path.AddBezier(pontoInicial, pontoCurva.Value, pontoCurva.Value, pontoFinal);
            return path;
        }

        private PointF ObterPontoRotulo(PointF pontoInicial, PointF pontoFinal, PointF? pontoCurva)
        {
            if (!TemCurva || !pontoCurva.HasValue)
            {
                return new PointF(
                    (pontoInicial.X + pontoFinal.X) / 2f,
                    (pontoInicial.Y + pontoFinal.Y) / 2f);
            }

            if (CurvaCircular && GeometryHelper.TryGetArcoCircular(
                pontoInicial,
                pontoCurva.Value,
                pontoFinal,
                out var centro,
                out var raio,
                out var anguloInicial,
                out var varredura))
            {
                return GeometryHelper.ObterPontoArcoCircular(centro, raio, anguloInicial, varredura, 0.5f);
            }

            // Quadrática em t=0.5
            float t = 0.5f;
            float u = 1f - t;
            return new PointF(
                u * u * pontoInicial.X + 2f * u * t * pontoCurva.Value.X + t * t * pontoFinal.X,
                u * u * pontoInicial.Y + 2f * u * t * pontoCurva.Value.Y + t * t * pontoFinal.Y);
        }

        private void ObterPontosMundo(out PointF pInicial, out PointF pFinal, out PointF? pCurva)
        {
            // A geometria da seta já é mantida em coordenadas de mundo
            // pelos métodos de transformação (Mover/Escalar/RotacionarAoRedor).
            // Portanto, não reaplicamos Rotacao aqui para evitar rotação dupla.
            pInicial = PontoInicial;
            pFinal = PontoFinal;
            pCurva = PontoCurva;
        }

        private static PointF RotacionarPontoLocal(PointF ponto, PointF centro, float anguloGraus)
        {
            double rad = anguloGraus * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            float dx = ponto.X - centro.X;
            float dy = ponto.Y - centro.Y;

            return new PointF(
                centro.X + (float)(dx * cos - dy * sin),
                centro.Y + (float)(dx * sin + dy * cos));
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);

            if (PontoCurva.HasValue)
            {
                PontoCurva = new PointF(PontoCurva.Value.X + dx, PontoCurva.Value.Y + dy);
            }
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);

            if (PontoCurva.HasValue)
            {
                PontoCurva = EscalarPonto(PontoCurva.Value, centro, fatorX, fatorY);
            }

            base.EscalarAoRedor(centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPontoLocal(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPontoLocal(PontoFinal, centro, deltaGraus);

            if (PontoCurva.HasValue)
            {
                PontoCurva = RotacionarPontoLocal(PontoCurva.Value, centro, deltaGraus);
            }

            Rotacao += deltaGraus;
            Posicao = RotacionarPontoLocal(Posicao, centro, deltaGraus);
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
    }
}