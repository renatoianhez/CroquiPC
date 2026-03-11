// Objects/ArrowObject.cs
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class ArrowObject : BaseSketchObject
    {
        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Category("Aparência")]
        [DisplayName("Tamanho da Seta")]
        [Description("Tamanho da ponta da seta em pixels")]
        public float TamanhoSeta { get; set; } = 12f;

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

            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal);

            using (var pen = new Pen(CorContorno, EspessuraContorno))
            {
                if (SetaFim)
                {
                    pen.CustomEndCap = new AdjustableArrowCap(
                        TamanhoSeta / 2, TamanhoSeta / 2);
                }
                if (SetaInicio)
                {
                    pen.CustomStartCap = new AdjustableArrowCap(
                        TamanhoSeta / 2, TamanhoSeta / 2);
                }

                g.DrawLine(pen, pontoInicial, pontoFinal);
            }

            if (!string.IsNullOrEmpty(Rotulo))
            {
                var centro = new PointF(
                    (pontoInicial.X + pontoFinal.X) / 2,
                    (pontoInicial.Y + pontoFinal.Y) / 2);

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

                    g.DrawString(Rotulo, font,
                        new SolidBrush(CorContorno), centro, sf);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal);
            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, pontoInicial, pontoFinal) <= tolerancia + 5;
        }

        public override RectangleF GetBounds()
        {
            ObterPontosRotacionados(out var pontoInicial, out var pontoFinal);

            float margin = TamanhoSeta;
            float minX = Math.Min(pontoInicial.X, pontoFinal.X) - margin;
            float minY = Math.Min(pontoInicial.Y, pontoFinal.Y) - margin;
            float maxX = Math.Max(pontoInicial.X, pontoFinal.X) + margin;
            float maxY = Math.Max(pontoInicial.Y, pontoFinal.Y) + margin;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private void ObterPontosRotacionados(out PointF pInicial, out PointF pFinal)
        {
            if (Math.Abs(Rotacao) < 0.001f)
            {
                pInicial = PontoInicial;
                pFinal = PontoFinal;
                return;
            }

            var centro = new PointF(
                (PontoInicial.X + PontoFinal.X) / 2f,
                (PontoInicial.Y + PontoFinal.Y) / 2f);

            pInicial = RotacionarPonto(PontoInicial, centro, Rotacao);
            pFinal = RotacionarPonto(PontoFinal, centro, Rotacao);
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
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            PontoInicial = EscalarPonto(PontoInicial, centro, fatorX, fatorY);
            PontoFinal = EscalarPonto(PontoFinal, centro, fatorX, fatorY);
            base.EscalarAoRedor(centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            PontoInicial = RotacionarPonto(PontoInicial, centro, deltaGraus);
            PontoFinal = RotacionarPonto(PontoFinal, centro, deltaGraus);
            Rotacao += deltaGraus;
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
        }
    }
}