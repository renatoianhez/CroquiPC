// Objects/ArrowObject.cs
using CrimeSketcher.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

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

        public ArrowObject()
        {
            Tipo = "Seta";
            CorContorno = Color.DarkBlue;
            EspessuraContorno = 2f;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal, out var pontoCurva);

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

                using (var font = new Font("Segoe UI", 8f))
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

                    using (var brushTexto = new SolidBrush(CorContorno))
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
            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal, out var pontoCurva);

            using (var path = CriarCaminhoSeta(pontoInicial, pontoFinal, pontoCurva))
            using (var pen = new Pen(Color.Black, EspessuraContorno + tolerancia + 5f))
            {
                return path.IsOutlineVisible(ponto, pen);
            }
        }

        public override RectangleF GetBounds()
        {
            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal, out var pontoCurva);

            using (var path = CriarCaminhoSeta(pontoInicial, pontoFinal, pontoCurva))
            {
                var bounds = path.GetBounds();
                bounds.Inflate(TamanhoSeta + 8f, TamanhoSeta + 8f);
                return bounds;
            }
        }

        public bool ContemPontoCurva(PointF ponto, float tolerancia = 10f)
        {
            if (!TemCurva || !PontoCurva.HasValue) return false;

            float dx = ponto.X - PontoCurva.Value.X;
            float dy = ponto.Y - PontoCurva.Value.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= tolerancia;
        }

        public void MoverPontoCurva(PointF novaPosicao)
        {
            if (TemCurva)
            {
                PontoCurva = novaPosicao;
            }
        }

        private void DesenharPontoCurva(Graphics g, PointF pontoInicial, PointF pontoFinal, PointF pontoCurva)
        {
            using (var pen = new Pen(Color.DodgerBlue, 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, pontoInicial, pontoCurva);
                g.DrawLine(pen, pontoCurva, pontoFinal);
            }

            float curveRadius = 7f;
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
            using (var pen = new Pen(Color.DodgerBlue, 2f))
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

            // Quadrática em t=0.5
            float t = 0.5f;
            float u = 1f - t;
            return new PointF(
                u * u * pontoInicial.X + 2f * u * t * pontoCurva.Value.X + t * t * pontoFinal.X,
                u * u * pontoInicial.Y + 2f * u * t * pontoCurva.Value.Y + t * t * pontoFinal.Y);
        }

        private void ObterPontosRotacionados(out PointF pInicial, out PointF pFinal, out PointF? pCurva)
        {
            if (Math.Abs(Rotacao) < 0.001f)
            {
                pInicial = PontoInicial;
                pFinal = PontoFinal;
                pCurva = PontoCurva;
                return;
            }

            var centro = new PointF(
                (PontoInicial.X + PontoFinal.X) / 2f,
                (PontoInicial.Y + PontoFinal.Y) / 2f);

            pInicial = RotacionarPonto(PontoInicial, centro, Rotacao);
            pFinal = RotacionarPonto(PontoFinal, centro, Rotacao);
            pCurva = PontoCurva.HasValue
                ? RotacionarPonto(PontoCurva.Value, centro, Rotacao)
                : null;
        }

        private static PointF RotacionarPonto(PointF ponto, PointF centro, float anguloGraus)
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
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);

            if (PontoCurva.HasValue)
            {
                PontoCurva = RotacionarPonto(PontoCurva.Value, centro, deltaGraus);
            }

            Rotacao += deltaGraus;
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
        }
    }
}