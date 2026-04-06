// Objects/AreaObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    public enum TexturaArea
    {
        Nenhuma,
        Diagonal,
        Cruzada,
        Pontilhada,
        Grade
    }

    [Serializable]
    public class AreaObject : BaseSketchObject
    {
        [Browsable(false)]
        public List<PointF> Vertices { get; set; } = new List<PointF>();

        [Browsable(false)]
        public int CorTexturaArgb { get; set; } = Color.FromArgb(90, 40, 40, 40).ToArgb();

        [Category("Aparência")]
        [DisplayName("Textura")]
        [Description("Tipo de textura aplicada sobre a área")]
        public TexturaArea Textura { get; set; } = TexturaArea.Diagonal;

        [Category("Aparência")]
        [DisplayName("Cor da Textura")]
        [Description("Cor da textura da área")]
        [JsonIgnore]
        public Color CorTextura
        {
            get => Color.FromArgb(CorTexturaArgb);
            set => CorTexturaArgb = value.ToArgb();
        }

        public AreaObject()
        {
            Tipo = "Área";
            CorPreenchimento = Color.FromArgb(110, 90, 170, 120);
            CorContorno = Color.FromArgb(45, 90, 110, 90);
            EspessuraContorno = 2f;
        }

        public static AreaObject Criar(IEnumerable<PointF> vertices)
        {
            var lista = vertices.ToList();
            var area = new AreaObject { Vertices = lista };
            area.Posicao = area.ObterCentro();
            return area;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel || Vertices.Count < 3) return;

            var pontos = Vertices.ToArray();

            using (var brush = new SolidBrush(Color.FromArgb(CorPreenchimentoArgb)))
            {
                g.FillPolygon(brush, pontos);
            }

            if (Textura != TexturaArea.Nenhuma)
            {
                using var brushTextura = new HatchBrush(ObterHatchStyle(Textura), Color.FromArgb(CorTexturaArgb), Color.Transparent);
                g.FillPolygon(brushTextura, pontos);
            }

            using (var pen = new Pen(Color.FromArgb(CorContornoArgb), Math.Max(1f, EspessuraContorno)))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPolygon(pen, pontos);
            }

            if (Selecionado) DesenharSelecao(g);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            if (Vertices.Count < 3) return false;

            using var path = new GraphicsPath();
            path.AddPolygon(Vertices.ToArray());
            using var pen = new Pen(Color.Black, Math.Max(1f, EspessuraContorno + tolerancia * 2f));
            return path.IsVisible(ponto) || path.IsOutlineVisible(ponto, pen);
        }

        public override RectangleF GetBounds()
        {
            if (Vertices.Count == 0)
                return new RectangleF(Posicao, SizeF.Empty);

            float minX = Vertices.Min(v => v.X);
            float minY = Vertices.Min(v => v.Y);
            float maxX = Vertices.Max(v => v.X);
            float maxY = Vertices.Max(v => v.Y);
            float extra = Math.Max(1f, EspessuraContorno);

            return RectangleF.FromLTRB(minX - extra, minY - extra, maxX + extra, maxY + extra);
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

        private PointF ObterCentro()
        {
            if (Vertices.Count == 0)
                return PointF.Empty;

            return new PointF(Vertices.Average(v => v.X), Vertices.Average(v => v.Y));
        }

        private static HatchStyle ObterHatchStyle(TexturaArea textura)
        {
            return textura switch
            {
                TexturaArea.Diagonal => HatchStyle.ForwardDiagonal,
                TexturaArea.Cruzada => HatchStyle.DiagonalCross,
                TexturaArea.Pontilhada => HatchStyle.DottedGrid,
                TexturaArea.Grade => HatchStyle.LargeGrid,
                _ => HatchStyle.ForwardDiagonal
            };
        }
    }
}
