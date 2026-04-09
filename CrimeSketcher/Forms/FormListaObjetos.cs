using CrimeSketcher.Core;
using CrimeSketcher.Objects;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrimeSketcher.Forms
{
    public class FormListaObjetos : Form
    {
        private ListView listViewObjetos;
        private SketchDocument _documento;
        public event Action<BaseSketchObject> ObjetoSelecionado;
        public event Action<BaseSketchObject> ObjetoExcluido;

        public FormListaObjetos(SketchDocument documento)
        {
            _documento = documento;
            InitializeComponent();
            AtualizarLista();
        }

        private void InitializeComponent()
        {
            Text = "Objetos na Cena";
            Size = new Size(560, 400);
            listViewObjetos = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewObjetos.Columns.Add("Nome", 150);
            listViewObjetos.Columns.Add("Tipo", 100);
            listViewObjetos.Columns.Add("Posição", 180);
            listViewObjetos.Columns.Add("Ação", 80);
            listViewObjetos.SelectedIndexChanged += ListViewObjetos_SelectedIndexChanged;
            listViewObjetos.MouseClick += ListViewObjetos_MouseClick;
            Controls.Add(listViewObjetos);
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

                item.SubItems.Add("🗑 Excluir");
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

        private void ListViewObjetos_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var hit = listViewObjetos.HitTest(e.Location);
            if (hit?.Item == null || hit.SubItem == null)
                return;

            int subItemIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
            if (subItemIndex != 3)
                return;

            if (hit.Item.Tag is not BaseSketchObject obj)
                return;

            var confirm = MessageBox.Show(
                $"Deseja excluir o objeto '{obj.Nome}'?",
                "Excluir objeto",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _documento.RemoverObjeto(obj);
            ObjetoExcluido?.Invoke(obj);
            AtualizarLista();
        }
    }
}
