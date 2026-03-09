// Objects/RoomObject.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class RoomObject : BaseSketchObject
    {
        public List<PointF> Vertices { get; set; } = new List<PointF>();
        public float EspessuraParede { get; set; } = 8f;
        public string NomeComodo { get; set; } = "";
        public bool MostrarNome { get; set; } = true;
        public int CorPisoArgb { get; set; } = Color.FromArgb(30, 200, 200, 200).ToArgb();

        public RoomObject()
        {
            Tipo = "Cômodo";
            CorContorno = Color.Black;
            EspessuraContorno = 2f;
        }

        /// <summary>
        /// Cria um cômodo retangular
        /// </summary>
        public static RoomObject CriarRetangular(PointF posicao,
            float largura, float altura, string nome = "")
        {
            var room = new RoomObject
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
            return room;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel || Vertices.Count < 3) return;

            var points = Vertices.ToArray();

            // Piso
            using (var brush = new SolidBrush(Color.FromArgb(CorPisoArgb)))
            {
                g.FillPolygon(brush, points);
            }

            // Paredes
            using (var pen = new Pen(CorContorno, EspessuraParede))
            {
                pen.StartCap = LineCap.Square;
                pen.EndCap = LineCap.Square;
                pen.LineJoin = LineJoin.Miter;
                g.DrawPolygon(pen, points);
            }

            // Hachura nas paredes
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(points);
                // Contorno externo expandido
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

            // Nome do cômodo
            if (MostrarNome && !string.IsNullOrEmpty(NomeComodo))
            {
                var bounds = GetBounds();
                var center = new PointF(
                    bounds.X + bounds.Width / 2,
                    bounds.Y + bounds.Height / 2);

                using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Fundo semi-transparente
                    var textSize = g.MeasureString(NomeComodo, font);
                    var textRect = new RectangleF(
                        center.X - textSize.Width / 2 - 4,
                        center.Y - textSize.Height / 2 - 2,
                        textSize.Width + 8, textSize.Height + 4);

                    using (var bgBrush = new SolidBrush(
                        Color.FromArgb(180, 255, 255, 255)))
                    {
                        g.FillRectangle(bgBrush, textRect);
                    }

                    g.DrawString(NomeComodo, font, Brushes.DarkSlateGray,
                        center, format);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            if (Vertices.Count < 3) return false;

            // Verifica se está dentro do polígono
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(Vertices.ToArray());
                // Expande para incluir a espessura da parede
                using (var pen = new Pen(Color.Black, EspessuraParede + tolerancia * 2))
                {
                    return path.IsVisible(ponto) ||
                           path.IsOutlineVisible(ponto, pen);
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

            return new RectangleF(minX - EspessuraParede / 2,
                minY - EspessuraParede / 2,
                maxX - minX + EspessuraParede,
                maxY - minY + EspessuraParede);
        }

        public override void Mover(float dx, float dy)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = new PointF(
                    Vertices[i].X + dx,
                    Vertices[i].Y + dy);
            }
            base.Mover(dx, dy);
        }
    }
}