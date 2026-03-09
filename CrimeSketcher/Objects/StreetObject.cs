// Objects/StreetObject.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class StreetObject : BaseSketchObject
    {
        public PointF PontoInicial { get; set; }
        public PointF PontoFinal { get; set; }
        public float Largura { get; set; } = 80f; // pixels
        public string NomeRua { get; set; } = "";
        public bool MostrarFaixas { get; set; } = true;
        public int NumeroFaixas { get; set; } = 2;
        public bool MaoUnica { get; set; } = false;
        public bool TemCalcada { get; set; } = true;
        public float LarguraCalcada { get; set; } = 15f;
        public bool TemMeioFio { get; set; } = true;

        public StreetObject()
        {
            Tipo = "Rua";
            CorPreenchimento = Color.FromArgb(180, 180, 180);
            CorContorno = Color.FromArgb(100, 100, 100);
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            float dx = PontoFinal.X - PontoInicial.X;
            float dy = PontoFinal.Y - PontoInicial.Y;
            float comp = (float)Math.Sqrt(dx * dx + dy * dy);
            if (comp == 0) return;

            float angulo = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            float perpX = -dy / comp;
            float perpY = dx / comp;

            // Calçadas
            if (TemCalcada)
            {
                float totalWidth = Largura + LarguraCalcada * 2;
                DesenharFaixa(g, perpX, perpY, totalWidth,
                    Color.FromArgb(210, 210, 200), angulo, comp);
            }

            // Asfalto
            DesenharFaixa(g, perpX, perpY, Largura,
                CorPreenchimento, angulo, comp);

            // Meio-fio
            if (TemMeioFio)
            {
                using (var pen = new Pen(Color.FromArgb(150, 150, 140), 2f))
                {
                    float offset = Largura / 2;
                    g.DrawLine(pen,
                        PontoInicial.X + perpX * offset,
                        PontoInicial.Y + perpY * offset,
                        PontoFinal.X + perpX * offset,
                        PontoFinal.Y + perpY * offset);
                    g.DrawLine(pen,
                        PontoInicial.X - perpX * offset,
                        PontoInicial.Y - perpY * offset,
                        PontoFinal.X - perpX * offset,
                        PontoFinal.Y - perpY * offset);
                }
            }

            // Faixas centrais
            if (MostrarFaixas)
            {
                using (var pen = new Pen(Color.Yellow, 2f))
                {
                    if (!MaoUnica)
                    {
                        pen.DashStyle = DashStyle.Custom;
                        pen.DashPattern = new float[] { 10, 8 };
                    }

                    g.DrawLine(pen, PontoInicial, PontoFinal);
                }

                // Faixas de divisão de pistas
                if (NumeroFaixas > 2)
                {
                    using (var pen = new Pen(Color.White, 1.5f))
                    {
                        pen.DashStyle = DashStyle.Custom;
                        pen.DashPattern = new float[] { 8, 12 };

                        float faixaLargura = Largura / NumeroFaixas;
                        for (int i = 1; i < NumeroFaixas; i++)
                        {
                            if (i == NumeroFaixas / 2) continue; // pula centro

                            float offset = -Largura / 2 + i * faixaLargura;
                            g.DrawLine(pen,
                                PontoInicial.X + perpX * offset,
                                PontoInicial.Y + perpY * offset,
                                PontoFinal.X + perpX * offset,
                                PontoFinal.Y + perpY * offset);
                        }
                    }
                }
            }

            // Nome da rua
            if (!string.IsNullOrEmpty(NomeRua))
            {
                var centro = new PointF(
                    (PontoInicial.X + PontoFinal.X) / 2,
                    (PontoInicial.Y + PontoFinal.Y) / 2);

                var state = g.Save();
                g.TranslateTransform(centro.X, centro.Y);

                float textAngle = angulo;
                if (textAngle > 90 || textAngle < -90) textAngle += 180;
                g.RotateTransform(textAngle);

                using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    var size = g.MeasureString(NomeRua, font);
                    using (var bg = new SolidBrush(
                        Color.FromArgb(200, 255, 255, 255)))
                    {
                        g.FillRectangle(bg,
                            -size.Width / 2 - 3, -size.Height / 2 - 1,
                            size.Width + 6, size.Height + 2);
                    }

                    g.DrawString(NomeRua, font, Brushes.DarkSlateGray,
                        0, 0, format);
                }

                g.Restore(state);
            }

            if (Selecionado) DesenharSelecao(g);
        }

        private void DesenharFaixa(Graphics g, float perpX, float perpY,
            float largura, Color cor, float angulo, float comprimento)
        {
            float halfW = largura / 2;

            var points = new PointF[]
            {
                new PointF(PontoInicial.X + perpX * halfW,
                           PontoInicial.Y + perpY * halfW),
                new PointF(PontoFinal.X + perpX * halfW,
                           PontoFinal.Y + perpY * halfW),
                new PointF(PontoFinal.X - perpX * halfW,
                           PontoFinal.Y - perpY * halfW),
                new PointF(PontoInicial.X - perpX * halfW,
                           PontoInicial.Y - perpY * halfW)
            };

            using (var brush = new SolidBrush(cor))
            {
                g.FillPolygon(brush, points);
            }
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            float totalW = Largura + (TemCalcada ? LarguraCalcada * 2 : 0);
            return Utils.GeometryHelper.DistanciaPontoSegmento(
                ponto, PontoInicial, PontoFinal) <= totalW / 2 + tolerancia;
        }

        public override RectangleF GetBounds()
        {
            float totalW = Largura + (TemCalcada ? LarguraCalcada * 2 : 0);
            float minX = Math.Min(PontoInicial.X, PontoFinal.X) - totalW;
            float minY = Math.Min(PontoInicial.Y, PontoFinal.Y) - totalW;
            float maxX = Math.Max(PontoInicial.X, PontoFinal.X) + totalW;
            float maxY = Math.Max(PontoInicial.Y, PontoFinal.Y) + totalW;
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override void Mover(float dx, float dy)
        {
            PontoInicial = new PointF(PontoInicial.X + dx, PontoInicial.Y + dy);
            PontoFinal = new PointF(PontoFinal.X + dx, PontoFinal.Y + dy);
        }
    }
}