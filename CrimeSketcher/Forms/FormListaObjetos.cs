using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormListaObjetos : Form
    {
        private ListView listViewObjetos;
        private SketchDocument _documento;
        public event Action<BaseSketchObject> ObjetoSelecionado;

        public FormListaObjetos(SketchDocument documento)
        {
            _documento = documento;
            InitializeComponent();
            AtualizarLista();
        }

        private void InitializeComponent()
        {
            this.Text = "Objetos na Cena";
            this.Size = new Size(500, 400);
            listViewObjetos = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            listViewObjetos.Columns.Add("Nome", 150);
            listViewObjetos.Columns.Add("Tipo", 100);
            listViewObjetos.Columns.Add("Posição", 120);
            listViewObjetos.SelectedIndexChanged += ListViewObjetos_SelectedIndexChanged;
            this.Controls.Add(listViewObjetos);
        }

        public void AtualizarLista()
        {
            listViewObjetos.Items.Clear();
            var esc = ScaleManager.Atual;
            foreach (var obj in _documento.Objetos)
            {
                var item = new ListViewItem(obj.Nome);
                item.SubItems.Add(obj.Tipo);
                if (esc != null)
                {
                    float xm = esc.PixelsParaReal(obj.Posicao.X);
                    float ym = esc.PixelsParaReal(obj.Posicao.Y);
                    item.SubItems.Add($"({xm:F2} {esc.UnidadeReal}, {ym:F2} {esc.UnidadeReal})");
                }
                else
                {
                    item.SubItems.Add($"({obj.Posicao.X:F0}, {obj.Posicao.Y:F0})");
                }
                item.Tag = obj;
                listViewObjetos.Items.Add(item);
            }
        }

        private void ListViewObjetos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewObjetos.SelectedItems.Count > 0)
            {
                var obj = listViewObjetos.SelectedItems[0].Tag as BaseSketchObject;
                ObjetoSelecionado?.Invoke(obj);
            }
        }
    }
}
