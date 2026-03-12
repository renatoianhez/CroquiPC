// Objects/TextLabel.cs
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class TextLabel : BaseSketchObject
    {
        [Category("Conteúdo")]
        [DisplayName("Texto")]
        [Description("Conteúdo do texto a ser exibido")]
        public string Texto { get; set; } = "Texto";

        [Category("Fonte")]
        [DisplayName("Nome da Fonte")]
        [Description("Nome da família da fonte (ex: Segoe UI, Arial)")]
        public string FonteNome { get; set; } = "Segoe UI";

        [Category("Fonte")]
        [DisplayName("Tamanho da Fonte")]
        [Description("Tamanho da fonte em pontos")]
        public float FonteTamanho { get; set; } = 12f;

        [Category("Fonte")]
        [DisplayName("Negrito")]
        [Description("Aplica estilo negrito ao texto")]
        public bool Negrito { get; set; } = false;

        [Category("Fonte")]
        [DisplayName("Itálico")]
        [Description("Aplica estilo itálico ao texto")]
        public bool Italico { get; set; } = false;

        [Category("Aparência")]
        [DisplayName("Possui Fundo")]
        [Description("Desenha um retângulo de fundo atrás do texto")]
        public bool ComFundo { get; set; } = false;

        [Browsable(false)]
        public int CorFundoArgb { get; set; } = Color.White.ToArgb();

        [Category("Aparência")]
        [DisplayName("Cor do Fundo")]
        [Description("Cor do retângulo de fundo")]
        [JsonIgnore]
        public Color CorFundo
        {
            get => Color.FromArgb(CorFundoArgb);
            set => CorFundoArgb = value.ToArgb();
        }

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
            FontStyle style = FontStyle.Regular;
            if (Negrito) style |= FontStyle.Bold;
            if (Italico) style |= FontStyle.Italic;

            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            using (var font = new Font(FonteNome, FonteTamanho, style))
            {
                var size = g.MeasureString(Texto, font);

                float sx = EscalaX;
                float sy = EscalaY;
                if (Math.Abs(sx) < 0.0001f || Math.Abs(sy) < 0.0001f)
                    return false;

                float dx = ponto.X - Posicao.X;
                float dy = ponto.Y - Posicao.Y;

                double rad = -Rotacao * Math.PI / 180.0;
                float cos = (float)Math.Cos(rad);
                float sin = (float)Math.Sin(rad);

                float xr = dx * cos - dy * sin;
                float yr = dx * sin + dy * cos;

                float lx = xr / sx;
                float ly = yr / sy;

                var local = new RectangleF(0, 0, size.Width, size.Height);
                local.Inflate(tolerancia, tolerancia);
                return local.Contains(lx, ly);
            }
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

                float sx = EscalaX;
                float sy = EscalaY;
                double rad = Rotacao * Math.PI / 180.0;
                float cos = (float)Math.Cos(rad);
                float sin = (float)Math.Sin(rad);

                PointF[] local = new[]
                {
                    new PointF(0, 0),
                    new PointF(size.Width, 0),
                    new PointF(size.Width, size.Height),
                    new PointF(0, size.Height)
                };

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                foreach (var p in local)
                {
                    float x = p.X * sx;
                    float y = p.Y * sy;

                    float wx = Posicao.X + (x * cos - y * sin);
                    float wy = Posicao.Y + (x * sin + y * cos);

                    minX = Math.Min(minX, wx);
                    minY = Math.Min(minY, wy);
                    maxX = Math.Max(maxX, wx);
                    maxY = Math.Max(maxY, wy);
                }

                return RectangleF.FromLTRB(minX, minY, maxX, maxY);
            }
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            float media = (Math.Abs(fatorX) + Math.Abs(fatorY)) / 2f;
            FonteTamanho = Math.Max(6f, FonteTamanho * media);
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
            EscalaX *= fatorX;
            EscalaY *= fatorY;
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }
    }
}