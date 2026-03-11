// Objects/StampObject.cs
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
        [DisplayName("Largura")]
        [Description("Largura do símbolo em pixels")]
        public float Largura { get; set; } = 40f;

        [Category("Dimensões")]
        [DisplayName("Altura")]
        [Description("Altura do símbolo em pixels")]
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
        public bool MostrarDescricao { get; set; } = true;

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
            var bounds = GetBounds();
            bounds.Inflate(tolerancia, tolerancia);
            return bounds.Contains(ponto);
        }

        public override RectangleF GetBounds()
        {
            return new RectangleF(Posicao.X, Posicao.Y,
                Largura * EscalaX, Altura * EscalaY);
        }
    }
}