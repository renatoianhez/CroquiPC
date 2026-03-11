// Objects/WallObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class WallObject : BaseSketchObject
    {
        [Browsable(false)]
        public PointF PontoInicial { get; set; }

        [Browsable(false)]
        public PointF PontoFinal { get; set; }

        [Category("Dimensões")]
        [DisplayName("Espessura da Parede")]
        [Description("Espessura da parede em pixels")]
        public float Espessura { get; set; } = 8f;

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
        [DisplayName("Largura da Porta")]
        [Description("Largura da porta em pixels")]
        public float LarguraPorta { get; set; } = 30f;

        [Category("Aberturas")]
        [DisplayName("Posição da Janela")]
        [Description("Posição relativa da janela na parede (0.0 a 1.0)")]
        public float PosicaoJanela { get; set; } = 0.65f;

        [Category("Aberturas")]
        [DisplayName("Largura da Janela")]
        [Description("Largura da janela em pixels")]
        public float LarguraJanela { get; set; } = 30f;

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
        [DisplayName("Espessura da Porta")]
        [Description("Espessura da linha da porta em pixels")]
        public float EspessuraPorta { get; set; } = 3f;

        [Category("Aberturas")]
        [DisplayName("Ângulo de Abertura")]
        [Description("Ângulo de abertura da porta em graus")]
        public float AnguloAberturaPorta { get; set; } = 90f;

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
                path.AddRectangle(new RectangleF(0, -halfW,
                    comprimento, Espessura));

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
                    float paredeAngulo = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                    float folhaAngulo = paredeAngulo - anguloAbertura;

                    float folhaDx = (float)(Math.Cos(folhaAngulo * Math.PI / 180.0) * doorLength);
                    float folhaDy = (float)(Math.Sin(folhaAngulo * Math.PI / 180.0) * doorLength);
                    var pFolha = new PointF(p2.X + folhaDx, p2.Y + folhaDy);

                    using (var penPorta = new Pen(CorPorta, Math.Max(1f, EspessuraPorta)))
                    {
                        penPorta.DashStyle = DashStyle.Dash;
                        g.DrawArc(penPorta,
                            p2.X - doorLength, p2.Y - doorLength,
                            doorLength * 2, doorLength * 2,
                            paredeAngulo - anguloAbertura, anguloAbertura);

                        penPorta.DashStyle = DashStyle.Solid;
                        g.DrawLine(penPorta, p2, pFolha);
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
    }
}