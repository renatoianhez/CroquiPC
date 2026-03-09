// Objects/ArrowObject.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class ArrowObject : BaseSketchObject
    {
        public PointF PontoInicial { get; set; }
        public PointF PontoFinal { get; set; }
        public float TamanhoSeta { get; set; } = 12f;
        public bool SetaInicio { get; set; } = false;
        public bool SetaFim { get; set; } = true;
        public string Rotulo { get; set; } = ""; // ex: "Norte", "Entrada"

        public ArrowObject()
        {
            Tipo = "Seta";
            CorContorno = Color.DarkBlue;
            EspessuraContorno = 2f;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

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

                g.DrawLine(pen, PontoInicial, PontoFinal);
            }

            if (!string.IsNullOrEmpty(Rotulo))
            {
                var centro = new PointF(
                    (PontoInicial.X + PontoFinal.X) / 2,
                    (PontoInicial.Y + PontoFinal.Y) / 2);

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
            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, PontoInicial, PontoFinal) <= tolerancia + 5;
        }

        public override RectangleF GetBounds()
        {
            float margin = TamanhoSeta;
            float minX = Math.Min(PontoInicial.X, PontoFinal.X) - margin;
            float minY = Math.Min(PontoInicial.Y, PontoFinal.Y) - margin;
            float maxX = Math.Max(PontoInicial.X, PontoFinal.X) + margin;
            float maxY = Math.Max(PontoInicial.Y, PontoFinal.Y) + margin;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
        }
    }
}