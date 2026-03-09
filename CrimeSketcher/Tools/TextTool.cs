// Tools/TextTool.cs
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Core;
using CrimeSketcher.Objects;

namespace CrimeSketcher.Tools
{
    public class TextTool : ITool
    {
        public string Nome => "Texto";
        public Cursor Cursor => Cursors.IBeam;

        private SketchDocument _doc;
        private GridManager _grid;

        public TextTool(SketchDocument doc, GridManager grid)
        {
            _doc = doc;
            _grid = grid;
        }

        public void OnMouseDown(MouseEventArgs e, PointF worldPos)
        {
            if (e.Button == MouseButtons.Left)
            {
                string texto = ShowInputDialog("Digite o texto:", "Inserir Texto");

                if (!string.IsNullOrEmpty(texto))
                {
                    var label = new TextLabel
                    {
                        Posicao = _grid.Snap(worldPos),
                        Texto = texto
                    };
                    _doc.AdicionarObjeto(label);
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e, PointF worldPos) { }
        public void OnMouseUp(MouseEventArgs e, PointF worldPos) { }
        public void OnKeyDown(KeyEventArgs e) { }
        public void Desenhar(Graphics g) { }
        public void Cancelar() { }

        /// <summary>
        /// Diálogo simples de entrada de texto
        /// </summary>
        private string ShowInputDialog(string prompt, string title)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = prompt;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancelar";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 15, 280, 20);
            textBox.SetBounds(10, 40, 280, 25);
            buttonOk.SetBounds(120, 75, 80, 28);
            buttonCancel.SetBounds(210, 75, 80, 28);

            form.ClientSize = new Size(305, 115);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult result = form.ShowDialog();
            return result == DialogResult.OK ? textBox.Text : "";
        }
    }
}