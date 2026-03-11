// Objects/RoomObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    public enum ParedeComodo
    {
        Superior,
        Direita,
        Inferior,
        Esquerda
    }

    [Serializable]
    public class RoomObject : BaseSketchObject
    {
        [Browsable(false)]
        public List<PointF> Vertices { get; set; } = new List<PointF>();

        [Category("Dimensões")]
        [DisplayName("Espessura da Parede")]
        [Description("Espessura das paredes do cômodo")]
        public float EspessuraParede { get; set; } = 8f;

        [Category("Identificação")]
        [DisplayName("Nome do Cômodo")]
        [Description("Nome ou identificação do cômodo")]
        public string NomeComodo { get; set; } = "";

        [Category("Aparência")]
        [DisplayName("Mostrar Nome")]
        [Description("Exibe o nome do cômodo no centro")]
        public bool MostrarNome { get; set; } = true;

        [Browsable(false)]
        public int CorPisoArgb { get; set; } = Color.FromArgb(30, 200, 200, 200).ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Piso")]
        [Description("Cor de preenchimento do piso")]
        [JsonIgnore]
        public Color CorPiso
        {
            get => Color.FromArgb(CorPisoArgb);
            set => CorPisoArgb = value.ToArgb();
        }

        [Category("Aberturas")]
        [DisplayName("Possui Porta")]
        [Description("Define se o cômodo possui porta")]
        public bool TemPorta { get; set; } = false;

        [Category("Aberturas")]
        [DisplayName("Parede da Porta")]
        [Description("Parede onde a porta será desenhada")]
        public ParedeComodo ParedePorta { get; set; } = ParedeComodo.Inferior;

        [Category("Aberturas")]
        [DisplayName("Posição da Porta")]
        [Description("Posição relativa da porta na parede (0.0 a 1.0)")]
        public float PosicaoPorta { get; set; } = 0.5f;

        [Category("Aberturas")]
        [DisplayName("Largura da Porta")]
        [Description("Largura da porta em pixels")]
        public float LarguraPorta { get; set; } = 30f;

        [Browsable(false)]
        public int CorPortaArgb { get; set; } = Color.DarkGray.ToArgb();

        [Category("Aberturas")]
        [DisplayName("Cor da Porta")]
        [Description("Cor usada na folha e arco da porta")]
        [JsonIgnore]
        public Color CorPorta
        {
            get => Color.FromArgb(CorPortaArgb);
            set => CorPortaArgb = value.ToArgb();
        }

        [Category("Aberturas")]
        [DisplayName("Espessura da Porta")]
        [Description("Espessura da linha da porta em pixels")]
        public float EspessuraPorta { get; set; } = 3f;

        [Category("Aberturas")]
        [DisplayName("Ângulo de Abertura da Porta")]
        [Description("Ângulo de abertura da porta em graus")]
        public float AnguloAberturaPorta { get; set; } = 90f;

        [Category("Aberturas")]
        [DisplayName("Possui Janela")]
        [Description("Define se o cômodo possui janela")]
        public bool TemJanela { get; set; } = false;

        [Category("Aberturas")]
        [DisplayName("Parede da Janela")]
        [Description("Parede onde a janela será desenhada")]
        public ParedeComodo ParedeJanela { get; set; } = ParedeComodo.Superior;

        [Category("Aberturas")]
        [DisplayName("Posição da Janela")]
        [Description("Posição relativa da janela na parede (0.0 a 1.0)")]
        public float PosicaoJanela { get; set; } = 0.5f;

        [Category("Aberturas")]
        [DisplayName("Largura da Janela")]
        [Description("Largura da janela em pixels")]
        public float LarguraJanela { get; set; } = 30f;

        [Browsable(false)]
        public int CorJanelaArgb { get; set; } = Color.FromArgb(150, 173, 216, 230).ToArgb();

        [Category("Aberturas")]
        [DisplayName("Cor da Janela")]
        [Description("Cor usada na moldura da janela")]
        [JsonIgnore]
        public Color CorJanela
        {
            get => Color.FromArgb(CorJanelaArgb);
            set => CorJanelaArgb = value.ToArgb();
        }

        public RoomObject()
        {
            Tipo = "Cômodo";
            CorContorno = Color.Black;
            EspessuraContorno = 2f;
        }

        public static RoomObject CriarRetangular(PointF posicao, float largura, float altura, string nome = "")
        {
            return new RoomObject
            {
                Posicao = posicao,
                NomeComodo = nome,
                Vertices = new List<PointF>
                {
                    new PointF(posicao.X, posicao.Y),
                    new PointF(posicao.X + largura, posicao.Y),
                    new PointF(posicao.X + largura, posicao.Y + altura),
                    new PointF(posicao.X, posicao.Y + altura)
                }
            };
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel || Vertices.Count < 3) return;

            var points = Vertices.ToArray();

            using (var brush = new SolidBrush(Color.FromArgb(CorPisoArgb)))
            {
                g.FillPolygon(brush, points);
            }

            using (var pen = new Pen(CorContorno, EspessuraParede))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;
                pen.LineJoin = LineJoin.Miter;
                g.DrawPolygon(pen, points);
            }

            using (var path = new GraphicsPath())
            {
                path.AddPolygon(points);
                using (var penExpand = new Pen(Color.Black, EspessuraParede))
                {
                    penExpand.LineJoin = LineJoin.Miter;
                    path.Widen(penExpand);
                }
                using (var hatch = new HatchBrush(HatchStyle.DiagonalCross,
                    Color.FromArgb(60, 0, 0, 0), Color.Transparent))
                {
                    g.FillPath(hatch, path);
                }
            }

            DesenharAberturas(g);

            if (MostrarNome && !string.IsNullOrEmpty(NomeComodo))
            {
                var bounds = GetBounds();
                var center = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);

                using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    var textSize = g.MeasureString(NomeComodo, font);
                    var textRect = new RectangleF(
                        center.X - textSize.Width / 2 - 4,
                        center.Y - textSize.Height / 2 - 2,
                        textSize.Width + 8,
                        textSize.Height + 4);

                    using (var bgBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                    {
                        g.FillRectangle(bgBrush, textRect);
                    }

                    g.DrawString(NomeComodo, font, Brushes.DarkSlateGray, center, format);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharAberturas(Graphics g)
        {
            if (!TemPorta && !TemJanela) return;
            if (Vertices.Count < 4) return;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var v in Vertices)
            {
                minX = Math.Min(minX, v.X);
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
                maxY = Math.Max(maxY, v.Y);
            }

            var rect = RectangleF.FromLTRB(minX, minY, maxX, maxY);

            if (TemPorta)
            {
                DesenharPortaNaParede(g, rect, ParedePorta);
            }

            if (TemJanela)
            {
                DesenharJanelaNaParede(g, rect, ParedeJanela);
            }
        }

        private void ObterSegmentoParede(RectangleF rect, ParedeComodo parede, out PointF pA, out PointF pB)
        {
            switch (parede)
            {
                case ParedeComodo.Superior:
                    pA = new PointF(rect.Left, rect.Top);
                    pB = new PointF(rect.Right, rect.Top);
                    break;
                case ParedeComodo.Direita:
                    pA = new PointF(rect.Right, rect.Top);
                    pB = new PointF(rect.Right, rect.Bottom);
                    break;
                case ParedeComodo.Inferior:
                    pA = new PointF(rect.Right, rect.Bottom);
                    pB = new PointF(rect.Left, rect.Bottom);
                    break;
                default:
                    pA = new PointF(rect.Left, rect.Bottom);
                    pB = new PointF(rect.Left, rect.Top);
                    break;
            }
        }

        private void DesenharPortaNaParede(Graphics g, RectangleF rect, ParedeComodo parede)
        {
            ObterSegmentoParede(rect, parede, out var pA, out var pB);

            float dx = pB.X - pA.X;
            float dy = pB.Y - pA.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp <= 0.1f) return;

            float largura = Math.Max(8f, Math.Min(LarguraPorta, comp - 4f));
            float posicao = Math.Max(0f, Math.Min(1f, PosicaoPorta));

            float inicio = Math.Max(2f, posicao * comp - largura / 2);
            float fim = Math.Min(comp - 2f, posicao * comp + largura / 2);
            if (fim <= inicio) return;

            float nx = dx / comp;
            float ny = dy / comp;

            var p2 = new PointF(pA.X + nx * inicio, pA.Y + ny * inicio);
            var p3 = new PointF(pA.X + nx * fim, pA.Y + ny * fim);

            using (var penRecote = new Pen(CorPiso, EspessuraParede + 2f))
            {
                penRecote.StartCap = LineCap.Square;
                penRecote.EndCap = LineCap.Square;
                g.DrawLine(penRecote, p2, p3);
            }

            float anguloAbertura = Math.Max(5f, Math.Min(170f, AnguloAberturaPorta));
            float doorLength = fim - inicio;
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

        private void DesenharJanelaNaParede(Graphics g, RectangleF rect, ParedeComodo parede)
        {
            ObterSegmentoParede(rect, parede, out var pA, out var pB);

            float dx = pB.X - pA.X;
            float dy = pB.Y - pA.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp <= 0.1f) return;

            float largura = Math.Max(8f, Math.Min(LarguraJanela, comp - 4f));
            float posicao = Math.Max(0f, Math.Min(1f, PosicaoJanela));

            float inicio = Math.Max(2f, posicao * comp - largura / 2);
            float fim = Math.Min(comp - 2f, posicao * comp + largura / 2);
            if (fim <= inicio) return;

            float nx = dx / comp;
            float ny = dy / comp;

            var p2 = new PointF(pA.X + nx * inicio, pA.Y + ny * inicio);
            var p3 = new PointF(pA.X + nx * fim, pA.Y + ny * fim);

            using (var penRecorte = new Pen(CorPiso, EspessuraParede + 2f))
            {
                penRecorte.StartCap = LineCap.Square;
                penRecorte.EndCap = LineCap.Square;
                g.DrawLine(penRecorte, p2, p3);
            }

            float perpX = -ny * EspessuraParede / 3f;
            float perpY = nx * EspessuraParede / 3f;

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

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            if (Vertices.Count < 3) return false;

            using (var path = new GraphicsPath())
            {
                path.AddPolygon(Vertices.ToArray());
                using (var pen = new Pen(Color.Black, EspessuraParede + tolerancia * 2))
                {
                    return path.IsVisible(ponto) || path.IsOutlineVisible(ponto, pen);
                }
            }
        }

        public override RectangleF GetBounds()
        {
            if (Vertices.Count == 0)
                return new RectangleF(Posicao, SizeF.Empty);

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var v in Vertices)
            {
                minX = Math.Min(minX, v.X);
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
                maxY = Math.Max(maxY, v.Y);
            }

            return new RectangleF(
                minX - EspessuraParede / 2,
                minY - EspessuraParede / 2,
                maxX - minX + EspessuraParede,
                maxY - minY + EspessuraParede);
        }

        public override void Mover(float dx, float dy)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new PointF(Vertices[i].X + dx, Vertices[i].Y + dy);
            }

            base.Mover(dx, dy);
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = EscalarPonto(Vertices[i], centro, fatorX, fatorY);
            }

            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            EspessuraParede = Math.Max(2f, EspessuraParede * media);
            LarguraPorta *= media;
            LarguraJanela *= media;
            base.EscalarAoRedor(centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = RotacionarPonto(Vertices[i], centro, deltaGraus);
            }

            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }
    }
}