// Objects/WallObject.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class WallObject : BaseSketchObject
    {
        public PointF PontoInicial { get; set; }
        public PointF PontoFinal { get; set; }
        public float Espessura { get; set; } = 8f; // pixels
        public bool TemPorta { get; set; } = false;
        public bool TemJanela { get; set; } = false;
        public float PosicaoAbertura { get; set; } = 0.5f; // 0-1
        public float LarguraAbertura { get; set; } = 30f;

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

            float inicioAbertura = PosicaoAbertura * comp - LarguraAbertura / 2;
            float fimAbertura = PosicaoAbertura * comp + LarguraAbertura / 2;

            float nx = dx / comp;
            float ny = dy / comp;

            // Segmento antes
            var p1 = PontoInicial;
            var p2 = new PointF(
                PontoInicial.X + nx * inicioAbertura,
                PontoInicial.Y + ny * inicioAbertura);
            g.DrawLine(pen, p1, p2);

            // Segmento depois
            var p3 = new PointF(
                PontoInicial.X + nx * fimAbertura,
                PontoInicial.Y + ny * fimAbertura);
            g.DrawLine(pen, p3, PontoFinal);

            if (TemPorta)
            {
                // Arco de porta
                float raio = LarguraAbertura;
                using (var penPorta = new Pen(Color.DarkGray, 1.5f))
                {
                    penPorta.DashStyle = DashStyle.Dash;
                    float startAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                    g.DrawArc(penPorta,
                        p2.X - raio, p2.Y - raio,
                        raio * 2, raio * 2,
                        startAngle - 90, 90);
                }
            }
            else if (TemJanela)
            {
                // Linhas paralelas para janela
                float perpX = -ny * Espessura / 2;
                float perpY = nx * Espessura / 2;

                using (var penJanela = new Pen(Color.LightBlue, 2f))
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
        }
    }
}