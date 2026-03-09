// Tools/ITool.cs
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Tools
{
    public interface ITool
    {
        string Nome { get; }
        Cursor Cursor { get; }
        void OnMouseDown(MouseEventArgs e, PointF worldPos);
        void OnMouseMove(MouseEventArgs e, PointF worldPos);
        void OnMouseUp(MouseEventArgs e, PointF worldPos);
        void OnKeyDown(KeyEventArgs e);
        void Desenhar(Graphics g); // Preview/feedback visual
        void Cancelar();
    }
}