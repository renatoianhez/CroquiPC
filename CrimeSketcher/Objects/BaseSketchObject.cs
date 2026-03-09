// Objects/BaseSketchObject.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public abstract class BaseSketchObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public PointF Posicao { get; set; }
        public float Rotacao { get; set; } = 0f;
        public float EscalaX { get; set; } = 1f;
        public float EscalaY { get; set; } = 1f;
        public bool Visivel { get; set; } = true;
        public bool Bloqueado { get; set; } = false;
        public bool Selecionado { get; set; } = false;
        public int Camada { get; set; } = 0;
        public float Opacidade { get; set; } = 1f;

        // Cores serializáveis
        public int CorPreenchimentoArgb { get; set; } = Color.Transparent.ToArgb();
        public int CorContornoArgb { get; set; } = Color.Black.ToArgb();
        public float EspessuraContorno { get; set; } = 2f;

        [JsonIgnore]
        public Color CorPreenchimento
        {
            get => Color.FromArgb(CorPreenchimentoArgb);
            set => CorPreenchimentoArgb = value.ToArgb();
        }

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
        }

        public virtual int GetHandleAtPoint(PointF ponto, float tolerancia = 5f)
        {
            var bounds = GetBounds();
            var handles = new PointF[]
            {
                new PointF(bounds.Left, bounds.Top),
                new PointF(bounds.Right, bounds.Top),
                new PointF(bounds.Left, bounds.Bottom),
                new PointF(bounds.Right, bounds.Bottom)
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