// Forms/FormConfiguracaoEscala.cs
using System;
using System.Windows.Forms;
using CrimeSketcher.Core;

namespace CrimeSketcher.Forms
{
    public class FormConfiguracaoEscala : Form
    {
        private NumericUpDown nudNumerador;
        private NumericUpDown nudDenominador;
        private ComboBox cmbUnidade;
        private NumericUpDown nudGridSpacing;
        private CheckBox chkSnap;
        private CheckBox chkGrid;
        private Button btnOk;
        private Button btnCancelar;
        private Label lblPreview;

        public ScaleManager Escala { get; private set; }
        public GridManager Grid { get; private set; }

        public FormConfiguracaoEscala(ScaleManager escala, GridManager grid)
        {
            Escala = escala;
            Grid = grid;
            InitializeComponent();
            CarregarValores();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuração de Escala e Grade";
            this.Size = new System.Drawing.Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var grpEscala = new GroupBox
            {
                Text = "Escala",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(365, 120)
            };

            grpEscala.Controls.Add(new Label
            {
                Text = "Escala 1 :",
                Location = new System.Drawing.Point(15, 30),
                AutoSize = true
            });

            nudDenominador = new NumericUpDown
            {
                Location = new System.Drawing.Point(85, 28),
                Size = new System.Drawing.Size(80, 23),
                Minimum = 1,
                Maximum = 10000,
                Value = 100,
                DecimalPlaces = 0
            };
            nudDenominador.ValueChanged += (s, e) => AtualizarPreview();
            grpEscala.Controls.Add(nudDenominador);

            grpEscala.Controls.Add(new Label
            {
                Text = "Unidade real:",
                Location = new System.Drawing.Point(15, 65),
                AutoSize = true
            });

            cmbUnidade = new ComboBox
            {
                Location = new System.Drawing.Point(120, 63),
                Size = new System.Drawing.Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbUnidade.Items.AddRange(new[] { "m", "cm", "mm" });
            cmbUnidade.SelectedIndex = 0;
            grpEscala.Controls.Add(cmbUnidade);

            lblPreview = new Label
            {
                Location = new System.Drawing.Point(15, 95),
                Size = new System.Drawing.Size(340, 20),
                ForeColor = System.Drawing.Color.DarkBlue,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Italic)
            };
            grpEscala.Controls.Add(lblPreview);

            this.Controls.Add(grpEscala);

            // Grade
            var grpGrid = new GroupBox
            {
                Text = "Grade",
                Location = new System.Drawing.Point(10, 140),
                Size = new System.Drawing.Size(365, 110)
            };

            chkGrid = new CheckBox
            {
                Text = "Mostrar grade",
                Location = new System.Drawing.Point(15, 25),
                AutoSize = true,
                Checked = true
            };
            grpGrid.Controls.Add(chkGrid);

            chkSnap = new CheckBox
            {
                Text = "Snap to Grid",
                Location = new System.Drawing.Point(15, 50),
                AutoSize = true,
                Checked = true
            };
            grpGrid.Controls.Add(chkSnap);

            grpGrid.Controls.Add(new Label
            {
                Text = "Espaçamento (px):",
                Location = new System.Drawing.Point(15, 80),
                AutoSize = true
            });

            nudGridSpacing = new NumericUpDown
            {
                Location = new System.Drawing.Point(140, 78),
                Size = new System.Drawing.Size(60, 23),
                Minimum = 5,
                Maximum = 100,
                Value = 20
            };
            grpGrid.Controls.Add(nudGridSpacing);

            this.Controls.Add(grpGrid);

            btnOk = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(200, 265),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new System.Drawing.Point(290, 265),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancelar);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancelar;
        }

        private void CarregarValores()
        {
            nudDenominador.Value = (decimal)Escala.EscalaDenominador;
            cmbUnidade.SelectedItem = Escala.UnidadeReal;
            nudGridSpacing.Value = (decimal)Grid.EspacamentoPixels;
            chkGrid.Checked = Grid.Visivel;
            chkSnap.Checked = Grid.SnapAtivo;
            AtualizarPreview();
        }

        private void AtualizarPreview()
        {
            float denom = (float)nudDenominador.Value;
            lblPreview.Text =
                $"1 cm no desenho = {denom / 100f:F2} m no local real";
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Escala.EscalaDenominador = (float)nudDenominador.Value;
            Escala.UnidadeReal = cmbUnidade.SelectedItem.ToString();
            Grid.EspacamentoPixels = (float)nudGridSpacing.Value;
            Grid.Visivel = chkGrid.Checked;
            Grid.SnapAtivo = chkSnap.Checked;
        }
    }
}