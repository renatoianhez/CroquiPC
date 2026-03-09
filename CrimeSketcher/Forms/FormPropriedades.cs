// Forms/FormPropriedades.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using CrimeSketcher.Objects;

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
            this.Text = "Propriedades";
            this.Size = new Size(300, 500);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized
            };

            this.Controls.Add(propertyGrid);
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