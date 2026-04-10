using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CrimeSketcher.Core
{
    public sealed class TemplateTrevoInfo
    {
        public string NomeExibicao { get; init; }
        public string CaminhoArquivo { get; init; }
        public string Descricao { get; init; }

        public override string ToString() => NomeExibicao;
    }

    public static class TrevoTemplateFactory
    {
        public static List<TemplateTrevoInfo> ListarTemplates()
        {
            string pastaTemplates = ObterPastaTemplates();
            if (string.IsNullOrWhiteSpace(pastaTemplates) || !Directory.Exists(pastaTemplates))
                return [];

            return Directory
                .EnumerateFiles(pastaTemplates, "*.csk", SearchOption.TopDirectoryOnly)
                .Select(CriarInfoTemplate)
                .OrderBy(t => ObterOrdemTemplate(t.NomeExibicao))
                .ThenBy(t => t.NomeExibicao)
                .ToList();
        }

        public static List<BaseSketchObject> Criar(TemplateTrevoInfo template, PointF centro, float escala)
        {
            if (template == null)
                return [];

            if (string.IsNullOrWhiteSpace(template.CaminhoArquivo) || !File.Exists(template.CaminhoArquivo))
                return [];

            var documento = SketchDocument.Carregar(template.CaminhoArquivo);
            if (documento?.Objetos == null || documento.Objetos.Count == 0)
                return [];

            var objetos = documento.Objetos;
            RegenerarIds(objetos);

            float escalaNormalizada = escala <= 0f ? 1f : escala;
            PointF centroOrigem = CalcularCentroObjetos(objetos);

            if (Math.Abs(escalaNormalizada - 1f) > 0.0001f)
            {
                foreach (var obj in objetos)
                {
                    obj.EscalarAoRedor(centroOrigem, escalaNormalizada, escalaNormalizada);
                }
            }

            float dx = centro.X - centroOrigem.X;
            float dy = centro.Y - centroOrigem.Y;
            if (Math.Abs(dx) > 0.0001f || Math.Abs(dy) > 0.0001f)
            {
                foreach (var obj in objetos)
                {
                    obj.Mover(dx, dy);
                }
            }

            return objetos;
        }

        private static TemplateTrevoInfo CriarInfoTemplate(string caminhoArquivo)
        {
            string nomeArquivo = Path.GetFileNameWithoutExtension(caminhoArquivo);
            string nomeExibicao = NormalizarNomeTemplate(nomeArquivo);

            return new TemplateTrevoInfo
            {
                NomeExibicao = nomeExibicao,
                CaminhoArquivo = caminhoArquivo,
                Descricao = ObterDescricaoTemplate(nomeArquivo)
            };
        }

        private static string ObterPastaTemplates()
        {
            var diretorioAtual = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (diretorioAtual != null)
            {
                string pastaTemplates = Path.Combine(diretorioAtual.FullName, "Templates");
                if (Directory.Exists(pastaTemplates))
                    return pastaTemplates;

                diretorioAtual = diretorioAtual.Parent;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        }

        private static PointF CalcularCentroObjetos(List<BaseSketchObject> objetos)
        {
            RectangleF bounds = objetos[0].GetBounds();
            for (int i = 1; i < objetos.Count; i++)
            {
                bounds = RectangleF.Union(bounds, objetos[i].GetBounds());
            }

            return new PointF(
                bounds.Left + bounds.Width / 2f,
                bounds.Top + bounds.Height / 2f);
        }

        private static void RegenerarIds(IEnumerable<BaseSketchObject> objetos)
        {
            foreach (var obj in objetos)
            {
                obj.Id = Guid.NewGuid().ToString();

                if (obj is GroupObject grupo)
                    RegenerarIds(grupo.ObjetosMembro);
            }
        }

        private static int ObterOrdemTemplate(string nome)
        {
            string chave = nome.ToLowerInvariant();
            if (chave.Contains("nível") || chave.Contains("nivel"))
                return 0;
            if (chave.Contains("desnível") || chave.Contains("desnivel"))
                return 1;
            return 9;
        }

        private static string NormalizarNomeTemplate(string nomeArquivo)
        {
            string normalizado = nomeArquivo.Replace('_', ' ').Replace('-', ' ').Trim();
            if (string.IsNullOrWhiteSpace(normalizado))
                return "Template";

            var partes = normalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();
            for (int i = 0; i < partes.Length; i++)
            {
                if (i > 0)
                    builder.Append(' ');

                string parte = partes[i].ToLowerInvariant();
                if (parte == "desnivel")
                    builder.Append("desnível");
                else if (parte == "nivel")
                    builder.Append("nível");
                else
                    builder.Append(char.ToUpper(parte[0]) + parte[1..]);
            }

            return builder.ToString();
        }

        private static string ObterDescricaoTemplate(string nomeArquivo)
        {
            string chave = nomeArquivo.ToLowerInvariant();
            if (chave.Contains("desnivel"))
                return "Template carregado de arquivo .csk salvo pela aplicação para representar trevo em desnível.";
            if (chave.Contains("nivel"))
                return "Template carregado de arquivo .csk salvo pela aplicação para representar trevo em nível.";
            return "Template carregado de arquivo .csk salvo pela própria aplicação.";
        }
    }
}
