// Objects/StampObject.cs
using CrimeSketcher.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class StampObject : BaseSketchObject
    {
        // Propriedades específicas do StampObject
        [Browsable(false)]
        public string CaminhoImagem { get; set; }

        [Category("Identificação")]
        [DisplayName("Categoria")]
        [Description("Categoria de origem do símbolo")]
        [ReadOnly(true)]
        public string CategoriaOrigem { get; set; }

        [Category("Dimensões")]
        [DisplayName("Largura (m)")]
        [Description("Largura do símbolo em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float Largura { get; set; } = 40f;

        [Category("Dimensões")]
        [DisplayName("Altura (m)")]
        [Description("Altura do símbolo em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float Altura { get; set; } = 40f;

        [Category("Dimensões")]
        [DisplayName("Manter Proporção")]
        [Description("Mantém a proporção original da imagem ao redimensionar")]
        public bool ManterProporcao { get; set; } = true;

        [Category("Descrição")]
        [DisplayName("Descrição")]
        [Description("Texto descritivo do símbolo")]
        public string Descricao { get; set; } = "";

        [Category("Descrição")]
        [DisplayName("Mostrar Descrição")]
        [Description("Exibe a descrição abaixo do símbolo")]
        public bool MostrarDescricao { get; set; } = false;

        [Browsable(false)]
        [JsonIgnore]
        private Image _imagemCache;

        public StampObject()
        {
            Tipo = "Símbolo";
        }

        [JsonIgnore]
        public Image Imagem
        {
            get
            {
                if (_imagemCache == null && !string.IsNullOrEmpty(CaminhoImagem))
                {
                    try
                    {
                        if (File.Exists(CaminhoImagem))
                        {
                            _imagemCache = Image.FromFile(CaminhoImagem);
                        }
                    }
                    catch
                    {
                        _imagemCache = null;
                    }
                }
                return _imagemCache;
            }
        }

        public override void Desenhar(Graphics g)
        {
            if (!Visivel) return;

            var state = g.Save();
            g.TranslateTransform(Posicao.X + Largura / 2, Posicao.Y + Altura / 2);
            g.RotateTransform(Rotacao);
            g.ScaleTransform(EscalaX, EscalaY);

            var drawRect = new RectangleF(-Largura / 2, -Altura / 2, Largura, Altura);

            if (Imagem != null)
            {
                // Ajustar opacidade
                if (Opacidade < 1f)
                {
                    var cm = new ColorMatrix();
                    cm.Matrix33 = Opacidade;
                    var ia = new ImageAttributes();
                    ia.SetColorMatrix(cm);
                    g.DrawImage(Imagem,
                        new Rectangle((int)drawRect.X, (int)drawRect.Y,
                            (int)drawRect.Width, (int)drawRect.Height),
                        0, 0, Imagem.Width, Imagem.Height,
                        GraphicsUnit.Pixel, ia);
                }
                else
                {
                    g.DrawImage(Imagem, drawRect);
                }
            }
            else
            {
                // Placeholder se imagem não disponível
                using (var pen = new Pen(Color.Gray, 1f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(pen, drawRect.X, drawRect.Y,
                        drawRect.Width, drawRect.Height);
                    g.DrawLine(pen, drawRect.Location,
                        new PointF(drawRect.Right, drawRect.Bottom));
                    g.DrawLine(pen,
                        new PointF(drawRect.Right, drawRect.Y),
                        new PointF(drawRect.X, drawRect.Bottom));
                }

                using (var font = new Font("Segoe UI", 7f))
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(Nome ?? "?", font, Brushes.Gray, drawRect, sf);
                }
            }

            g.Restore(state);

            // Descrição abaixo
            if (MostrarDescricao && !string.IsNullOrEmpty(Descricao))
            {
                using (var font = new Font("Segoe UI", 7f))
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    g.DrawString(Descricao, font, Brushes.DarkSlateGray,
                        Posicao.X + Largura / 2,
                        Posicao.Y + Altura + 3, sf);
                }
            }

            if (Selecionado) DesenharSelecao(g);
        }

        public override bool ContemPonto(PointF ponto, float tolerancia)
        {
            float cx = Posicao.X + Largura / 2f;
            float cy = Posicao.Y + Altura / 2f;

            float sx = EscalaX;
            float sy = EscalaY;
            if (Math.Abs(sx) < 0.0001f || Math.Abs(sy) < 0.0001f)
                return false;

            float dx = ponto.X - cx;
            float dy = ponto.Y - cy;

            double rad = -Rotacao * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float xr = dx * cos - dy * sin;
            float yr = dx * sin + dy * cos;

            float lx = xr / sx;
            float ly = yr / sy;

            var local = new RectangleF(-Largura / 2f, -Altura / 2f, Largura, Altura);
            local.Inflate(tolerancia, tolerancia);
            return local.Contains(lx, ly);
        }

        public override RectangleF GetBounds()
        {
            float cx = Posicao.X + Largura / 2f;
            float cy = Posicao.Y + Altura / 2f;

            float sx = EscalaX;
            float sy = EscalaY;
            double rad = Rotacao * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            PointF[] local = new[]
            {
                new PointF(-Largura / 2f, -Altura / 2f),
                new PointF(Largura / 2f, -Altura / 2f),
                new PointF(Largura / 2f, Altura / 2f),
                new PointF(-Largura / 2f, Altura / 2f)
            };

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var p in local)
            {
                float x = p.X * sx;
                float y = p.Y * sy;

                float wx = cx + (x * cos - y * sin);
                float wy = cy + (x * sin + y * cos);

                minX = Math.Min(minX, wx);
                minY = Math.Min(minY, wy);
                maxX = Math.Max(maxX, wx);
                maxY = Math.Max(maxY, wy);
            }

            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }
    }
}