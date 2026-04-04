// Objects/BaseSketchObject.cs
using CrimeSketcher.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public abstract class BaseSketchObject
    {
        [Browsable(false)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Category("Identificação")]
        [DisplayName("Nome")]
        [Description("Nome ou identificação do objeto")]
        public string Nome { get; set; } = "";

        [Category("Identificação")]
        [DisplayName("Tipo")]
        [Description("Tipo do objeto")]
        [ReadOnly(true)]
        public string Tipo { get; set; } = "";

        [Category("Transformação")]
        [DisplayName("Posição (m)")]
        [Description("Posição do objeto em metros")]
        [TypeConverter(typeof(PosicaoMetrosConverter))]
        public PointF Posicao { get; set; }

        [Category("Transformação")]
        [DisplayName("Rotação (graus)")]
        [Description("Ângulo de rotação em graus")]
        public float Rotacao { get; set; } = 0f;

        [Category("Transformação")]
        [DisplayName("Escala X")]
        [Description("Escala horizontal do objeto")]
        public float EscalaX { get; set; } = 1f;

        [Category("Transformação")]
        [DisplayName("Escala Y")]
        [Description("Escala vertical do objeto")]
        public float EscalaY { get; set; } = 1f;

        [Category("Visibilidade")]
        [DisplayName("Visível")]
        [Description("Define se o objeto está visível")]
        public bool Visivel { get; set; } = true;

        [Category("Visibilidade")]
        [DisplayName("Bloqueado")]
        [Description("Impede modificações no objeto")]
        public bool Bloqueado { get; set; } = false;

        [Browsable(false)]
        public bool Selecionado { get; set; } = false;

        [Category("Organização")]
        [DisplayName("Camada")]
        [Description("Camada de desenho (determina ordem de sobreposição)")]
        public int Camada { get; set; } = 0;

        [Category("Visibilidade")]
        [DisplayName("Opacidade")]
        [Description("Opacidade do objeto (0.0 a 1.0)")]
        public float Opacidade { get; set; } = 1f;

        // Cores serializáveis
        [Browsable(false)]
        public int CorPreenchimentoArgb { get; set; } = Color.Transparent.ToArgb();

        [Browsable(false)]
        public int CorContornoArgb { get; set; } = Color.Black.ToArgb();

        [Category("Aparência")]
        [DisplayName("Espessura do Contorno (m)")]
        [Description("Espessura da linha de contorno em metros")]
        [TypeConverter(typeof(MetrosTypeConverter))]
        public float EspessuraContorno { get; set; } = 2f;

        [Category("Aparência")]
        [DisplayName("Cor de Preenchimento")]
        [Description("Cor de preenchimento do objeto")]
        [JsonIgnore]
        public Color CorPreenchimento
        {
            get => Color.FromArgb(CorPreenchimentoArgb);
            set => CorPreenchimentoArgb = value.ToArgb();
        }

        [Category("Aparência")]
        [DisplayName("Cor do Contorno")]
        [Description("Cor da linha de contorno")]
        [JsonIgnore]
        public Color CorContorno
        {
            get => Color.FromArgb(CorContornoArgb);
            set => CorContornoArgb = value.ToArgb();
        }

        public abstract void Desenhar(Graphics g);
        public abstract bool ContemPonto(PointF ponto, float tolerancia);
        public abstract RectangleF GetBounds();

        public virtual bool IntersectaRetangulo(RectangleF rect)
        {
            return rect.IntersectsWith(GetBounds());
        }

        public virtual void Mover(float dx, float dy)
        {
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
        }

        public virtual void DesenharSelecao(Graphics g)
        {
            if (!Selecionado) return;

            var bounds = GetBounds();
            using (var pen = new Pen(Color.DodgerBlue, 1.5f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(pen, bounds.X - 3, bounds.Y - 3,
                    bounds.Width + 6, bounds.Height + 6);
            }

            float handleSize = 6f;
            var handles = new PointF[]
            {
                new PointF(bounds.Left, bounds.Top),
                new PointF(bounds.Right, bounds.Top),
                new PointF(bounds.Left, bounds.Bottom),
                new PointF(bounds.Right, bounds.Bottom),
                new PointF(bounds.Left + bounds.Width/2, bounds.Top),
                new PointF(bounds.Left + bounds.Width/2, bounds.Bottom),
                new PointF(bounds.Left, bounds.Top + bounds.Height/2),
                new PointF(bounds.Right, bounds.Top + bounds.Height/2)
            };

            foreach (var h in handles)
            {
                g.FillRectangle(Brushes.White,
                    h.X - handleSize / 2, h.Y - handleSize / 2,
                    handleSize, handleSize);
                g.DrawRectangle(Pens.DodgerBlue,
                    h.X - handleSize / 2, h.Y - handleSize / 2,
                    handleSize, handleSize);
            }

            var handleRotacao = new PointF(bounds.Left + bounds.Width / 2, bounds.Top - 18f);
            g.DrawLine(Pens.DodgerBlue,
                bounds.Left + bounds.Width / 2, bounds.Top,
                handleRotacao.X, handleRotacao.Y + handleSize / 2);
            g.FillEllipse(Brushes.White,
                handleRotacao.X - handleSize / 2, handleRotacao.Y - handleSize / 2,
                handleSize, handleSize);
            g.DrawEllipse(Pens.DodgerBlue,
                handleRotacao.X - handleSize / 2, handleRotacao.Y - handleSize / 2,
                handleSize, handleSize);
        }

        public virtual int GetHandleAtPoint(PointF ponto, float tolerancia = 5f)
        {
            var bounds = GetBounds();
            var handles = new PointF[]
            {
                new PointF(bounds.Left, bounds.Top),
                new PointF(bounds.Right, bounds.Top),
                new PointF(bounds.Left, bounds.Bottom),
                new PointF(bounds.Right, bounds.Bottom),
                new PointF(bounds.Left + bounds.Width/2, bounds.Top),
                new PointF(bounds.Left + bounds.Width/2, bounds.Bottom),
                new PointF(bounds.Left, bounds.Top + bounds.Height/2),
                new PointF(bounds.Right, bounds.Top + bounds.Height/2),
                new PointF(bounds.Left + bounds.Width/2, bounds.Top - 18f)
            };

            for (int i = 0; i < handles.Length; i++)
            {
                float dist = (float)Math.Sqrt(
                    Math.Pow(ponto.X - handles[i].X, 2) +
                    Math.Pow(ponto.Y - handles[i].Y, 2));
                if (dist <= tolerancia) return i;
            }
            return -1;
        }

        public virtual void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
            EscalaX *= fatorX;
            EscalaY *= fatorY;
        }

        public virtual void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }

        protected static PointF EscalarPonto(PointF ponto, PointF centro, float fatorX, float fatorY)
        {
            return new PointF(
                centro.X + (ponto.X - centro.X) * fatorX,
                centro.Y + (ponto.Y - centro.Y) * fatorY);
        }

        protected static PointF RotacionarPonto(PointF ponto, PointF centro, float anguloGraus)
        {
            double rad = anguloGraus * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            float dx = ponto.X - centro.X;
            float dy = ponto.Y - centro.Y;

            return new PointF(
                centro.X + (float)(dx * cos - dy * sin),
                centro.Y + (float)(dx * sin + dy * cos));
        }

        protected Matrix GetTransformMatrix()
        {
            var matrix = new Matrix();
            matrix.Translate(Posicao.X, Posicao.Y);
            matrix.Rotate(Rotacao);
            matrix.Scale(EscalaX, EscalaY);
            return matrix;
        }
    }
}