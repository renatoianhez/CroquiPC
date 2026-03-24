// Forms/FormPropriedades.cs
using CrimeSketcher.Objects;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormPropriedades : Form
    {
        private PropertyGrid propertyGrid;
        private BaseSketchObject _objeto;

        public FormPropriedades()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            propertyGrid = new PropertyGrid();
            SuspendLayout();
            // 
            // propertyGrid
            // 
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.Name = "propertyGrid";
            propertyGrid.TabIndex = 0;
            propertyGrid.ViewBackColor = SystemColors.Window;
            propertyGrid.ViewForeColor = SystemColors.WindowText;
            propertyGrid.HelpBackColor = SystemColors.Control;
            propertyGrid.HelpForeColor = SystemColors.ControlText;
            propertyGrid.LineColor = SystemColors.ActiveBorder;
            // 
            // FormPropriedades
            // 
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            ClientSize = new Size((int)(Screen.PrimaryScreen.Bounds.Width / 6f * 0.6f), 461);
            Controls.Add(propertyGrid);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "FormPropriedades";
            Text = "Propriedades";
            ResumeLayout(false);
        }

        public void MostrarPropriedades(BaseSketchObject obj)
        {
            _objeto = obj;
            propertyGrid.SelectedObject = obj;
        }

        public void Limpar()
        {
            propertyGrid.SelectedObject = null;
        }
    }
}