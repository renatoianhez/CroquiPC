// Objects/TextLabel.cs
using System;
using System.Drawing;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class TextLabel : BaseSketchObject
    {
        public string Texto { get; set; } = "Texto";
        public string FonteNome { get; set; } = "Segoe UI";
        public float FonteTamanho { get; set; } = 12f;
        public bool Negrito { get; set; } = false;
        public bool Italico { get; set; } = false;
        public bool ComFundo { get; set; } = false;
        public int CorFundoArgb { get; set; } = Color.White.ToArgb();

        public TextLabel()
        {
            Tipo = "Texto";
            CorContorno = Color.Black;
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            FontStyle style = FontStyle.Regular;
            if (Negrito) style |= FontStyle.Bold;
            if (Italico) style |= FontStyle.Italic;

            using (var font = new Font(FonteNome, FonteTamanho, style))
            {
                var size = g.MeasureString(Texto, font);

                if (Rotacao != 0)
                {
                    var state = g.Save();
                    g.TranslateTransform(Posicao.X, Posicao.Y);
                    g.RotateTransform(Rotacao);

                    if (ComFundo)
                    {
                        using (var bg = new SolidBrush(
                            Color.FromArgb(CorFundoArgb)))
                        {
                            g.FillRectangle(bg, -2, -2,
                                size.Width + 4, size.Height + 4);
                        }
                    }

                    g.DrawString(Texto, font,
                        new SolidBrush(CorContorno), 0, 0);
                    g.Restore(state);
                }
                else
                {
                    if (ComFundo)
                    {
                        using (var bg = new SolidBrush(
                            Color.FromArgb(CorFundoArgb)))
                        {
                            g.FillRectangle(bg,
                                Posicao.X - 2, Posicao.Y - 2,
                                size.Width + 4, size.Height + 4);
                        }
                    }

                    g.DrawString(Texto, font,
                        new SolidBrush(CorContorno), Posicao);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            var bounds = GetBounds();
            bounds.Inflate(tolerancia, tolerancia);
            return bounds.Contains(ponto);
        }

        public override RectangleF GetBounds()
        {
            FontStyle style = FontStyle.Regular;
            if (Negrito) style |= FontStyle.Bold;
            if (Italico) style |= FontStyle.Italic;

            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            using (var font = new Font(FonteNome, FonteTamanho, style))
            {
                var size = g.MeasureString(Texto, font);
                return new RectangleF(Posicao, size);
            }
        }
    }
}