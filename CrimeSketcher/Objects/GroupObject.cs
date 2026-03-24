// Objects/GroupObject.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace CrimeSketcher.Objects
{
    [Serializable]
    public class GroupObject : BaseSketchObject
    {
        [Browsable(false)]
        public List<BaseSketchObject> ObjetosMembro { get; set; } = new List<BaseSketchObject>();

        [Browsable(false)]
        public string TipoGrupo
        {
            get => "Grupo";
        }

        public GroupObject()
        {
            Nome = "Grupo";
            Tipo = "Grupo";
        }

        public GroupObject(List<BaseSketchObject> objetos)
        {
            Nome = "Grupo";
            Tipo = "Grupo";
            ObjetosMembro = objetos;
            AtualizarBounds();
        }

        public void AtualizarBounds()
        {
            if (ObjetosMembro.Count == 0) return;

            var bounds = ObjetosMembro[0].GetBounds();
            foreach (var obj in ObjetosMembro.Skip(1))
            {
                var objBounds = obj.GetBounds();
                bounds = RectangleF.Union(bounds, objBounds);
            }

            Posicao = new PointF(bounds.Left, bounds.Top);
        }

        public RectangleF ObterBoundsTodos()
        {
            if (ObjetosMembro.Count == 0)
                return new RectangleF(Posicao.X, Posicao.Y, 0, 0);

            var bounds = ObjetosMembro[0].GetBounds();
            foreach (var obj in ObjetosMembro.Skip(1))
            {
                var objBounds = obj.GetBounds();
                bounds = RectangleF.Union(bounds, objBounds);
            }

            return bounds;
        }

        public override void Desenhar(Graphics g)
        {
            // Desenhar todos os membros do grupo
            foreach (var obj in ObjetosMembro)
            {
                if (obj.Visivel)
                {
                    obj.Desenhar(g);
                }
            }
        }

        public override void DesenharSelecao(Graphics g)
        {
            if (!Selecionado) return;

            // Desenhar seleção ao redor de todos os membros
            var bounds = ObterBoundsTodos();
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

        public override bool ContemPonto(PointF ponto, float tolerancia = 5f)
        {
            // Um clique no grupo contém um ponto se algum membro contém
            return ObjetosMembro.Any(obj => obj.ContemPonto(ponto, tolerancia));
        }

        public override bool IntersectaRetangulo(RectangleF retangulo)
        {
            return ObjetosMembro.Any(obj => obj.IntersectaRetangulo(retangulo));
        }

        public override void Mover(float dx, float dy)
        {
            foreach (var obj in ObjetosMembro)
            {
                obj.Mover(dx, dy);
            }
            Posicao = new PointF(Posicao.X + dx, Posicao.Y + dy);
        }

        public override void EscalarAoRedor(PointF centro, float fatorX, float fatorY)
        {
            foreach (var obj in ObjetosMembro)
            {
                obj.EscalarAoRedor(centro, fatorX, fatorY);
            }
            Posicao = EscalarPonto(Posicao, centro, fatorX, fatorY);
        }

        public override void RotacionarAoRedor(PointF centro, float deltaGraus)
        {
            foreach (var obj in ObjetosMembro)
            {
                obj.RotacionarAoRedor(centro, deltaGraus);
            }
            Posicao = RotacionarPonto(Posicao, centro, deltaGraus);
            Rotacao += deltaGraus;
        }

        public override RectangleF GetBounds()
        {
            return ObterBoundsTodos();
        }
    }
}
