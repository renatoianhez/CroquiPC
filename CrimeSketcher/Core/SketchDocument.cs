// Core/SketchDocument.cs
using CrimeSketcher.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrimeSketcher.Core
{
    [Serializable]
    public class SketchDocument
    {
        public string Titulo { get; set; } = "Novo Croqui";
        public string NumeroProcedimento { get; set; } = "";
        public string Perito { get; set; } = "";
        public DateTime DataLevantamento { get; set; } = DateTime.Now;
        public string Endereco { get; set; } = "";
        public string Observacoes { get; set; } = "";

        public float LarguraPapelCm { get; set; } = 42f;
        public float AlturaPapelCm { get; set; } = 29.7f;
        public float EscalaDenominador { get; set; } = 100f;

        public List<BaseSketchObject> Objetos { get; set; } = new List<BaseSketchObject>();

        [JsonIgnore]
        private UndoRedoManager _undoRedo;

        [JsonIgnore]
        public UndoRedoManager UndoRedo
        {
            get
            {
                if (_undoRedo == null)
                    _undoRedo = new UndoRedoManager();
                return _undoRedo;
            }
        }

        public event EventHandler DocumentoAlterado;

        public void AdicionarObjeto(BaseSketchObject obj, bool comUndo = true)
        {
            Objetos.Add(obj);
            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new AddObjectAction(this, obj));
            }
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void RemoverObjeto(BaseSketchObject obj, bool comUndo = true)
        {
            Objetos.Remove(obj);
            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new RemoveObjectAction(this, obj));
            }
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public BaseSketchObject HitTest(PointF ponto, float tolerancia = 5f)
        {
            for (int i = Objetos.Count - 1; i >= 0; i--)
            {
                if (Objetos[i].ContemPonto(ponto, tolerancia))
                    return Objetos[i];
            }
            return null;
        }

        public List<BaseSketchObject> HitTestArea(RectangleF area)
        {
            return Objetos.Where(o => o.IntersectaRetangulo(area)).ToList();
        }

        public void Salvar(string caminho)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new SketchObjectConverter() }
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(caminho, json);
        }

        public static SketchDocument Carregar(string caminho)
        {
            var json = File.ReadAllText(caminho);
            var options = new JsonSerializerOptions
            {
                Converters = { new SketchObjectConverter() }
            };
            return JsonSerializer.Deserialize<SketchDocument>(json, options);
        }

        public void NotificarAlteracao()
        {
            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Agrupa múltiplos objetos em um GroupObject
        /// </summary>
        public GroupObject AgruparObjetos(List<BaseSketchObject> objetos, bool comUndo = true)
        {
            if (objetos.Count < 2)
                return null;

            // Remover grupos e ordenar pela ordem atual de desenho/camada
            var agrupar = objetos
                .Where(o => o is not GroupObject)
                .Distinct()
                .OrderBy(o => Objetos.IndexOf(o))
                .ToList();

            if (agrupar.Count < 2)
                return null;

            int indiceInsercao = agrupar.Min(o => Objetos.IndexOf(o));

            // Criar novo grupo
            var grupo = new GroupObject(agrupar);

            // Remover objetos individuais do documento
            foreach (var obj in agrupar)
            {
                Objetos.Remove(obj);
            }

            // Adicionar grupo na mesma posição do primeiro membro
            indiceInsercao = Math.Clamp(indiceInsercao, 0, Objetos.Count);
            Objetos.Insert(indiceInsercao, grupo);

            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new GroupAction(this, grupo, agrupar, indiceInsercao));
            }

            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
            return grupo;
        }

        /// <summary>
        /// Desagrupa um GroupObject
        /// </summary>
        public List<BaseSketchObject> DesagruparObjetos(GroupObject grupo, bool comUndo = true)
        {
            if (grupo == null || grupo.ObjetosMembro.Count == 0)
                return new List<BaseSketchObject>();

            var membros = new List<BaseSketchObject>(grupo.ObjetosMembro);
            int indiceGrupo = Objetos.IndexOf(grupo);
            if (indiceGrupo < 0)
                indiceGrupo = Objetos.Count;

            // Remover grupo
            Objetos.Remove(grupo);

            // Adicionar membros novamente preservando ordem e camada
            int indiceInsercao = Math.Clamp(indiceGrupo, 0, Objetos.Count);
            for (int i = 0; i < membros.Count; i++)
            {
                Objetos.Insert(indiceInsercao + i, membros[i]);
            }

            if (comUndo)
            {
                UndoRedo.RegistrarAcao(new UngroupAction(this, grupo, membros, indiceInsercao));
            }

            DocumentoAlterado?.Invoke(this, EventArgs.Empty);
            return membros;
        }
    }

    /// <summary>
    /// Converter para serialização polimórfica de objetos do croqui
    /// </summary>
    public class SketchObjectConverter : JsonConverter<BaseSketchObject>
    {
        public override BaseSketchObject Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                string tipo = root.GetProperty("Tipo").GetString();

                string json = root.GetRawText();

                return tipo switch
                {
                    "Parede" => JsonSerializer.Deserialize<WallObject>(json, options),
                    "Cômodo" => JsonSerializer.Deserialize<RoomObject>(json, options),
                    "Rua" or "Estrada" => JsonSerializer.Deserialize<StreetObject>(json, options),
                    "Cruzamento" => JsonSerializer.Deserialize<IntersectionObject>(json, options),
                    "Rotatória" => JsonSerializer.Deserialize<RoundaboutObject>(json, options),
                    "Cota" => JsonSerializer.Deserialize<DimensionLine>(json, options),
                    "Corpo" => JsonSerializer.Deserialize<StickFigure>(json, options),
                    "Símbolo" => JsonSerializer.Deserialize<StampObject>(json, options),
                    "Texto" => JsonSerializer.Deserialize<TextLabel>(json, options),
                    "Seta" => JsonSerializer.Deserialize<ArrowObject>(json, options),
                    "Marca" => JsonSerializer.Deserialize<MarkObject>(json, options),
                    "Área" => JsonSerializer.Deserialize<AreaObject>(json, options),
                    "Grupo" => JsonSerializer.Deserialize<GroupObject>(json, options),
                    _ => throw new JsonException($"Tipo desconhecido: {tipo}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer,
            BaseSketchObject value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    /// <summary>
    /// Ação de Undo/Redo para agrupar objetos
    /// </summary>
    public class GroupAction : IUndoableAction
    {
        private readonly SketchDocument _doc;
        private readonly GroupObject _grupo;
        private readonly List<BaseSketchObject> _membros;
        private readonly int _indiceInsercao;

        public string Descricao => $"Agrupar {_membros.Count} objetos";

        public GroupAction(SketchDocument doc, GroupObject grupo, List<BaseSketchObject> membros, int indiceInsercao)
        {
            _doc = doc;
            _grupo = grupo;
            _membros = new List<BaseSketchObject>(membros);
            _indiceInsercao = indiceInsercao;
        }

        public void Executar()
        {
            foreach (var obj in _membros)
            {
                _doc.Objetos.Remove(obj);
            }

            int indice = Math.Clamp(_indiceInsercao, 0, _doc.Objetos.Count);
            _doc.Objetos.Insert(indice, _grupo);
            _doc.NotificarAlteracao();
        }

        public void Desfazer()
        {
            _doc.Objetos.Remove(_grupo);

            int indice = Math.Clamp(_indiceInsercao, 0, _doc.Objetos.Count);
            for (int i = 0; i < _membros.Count; i++)
            {
                _doc.Objetos.Insert(indice + i, _membros[i]);
            }

            _doc.NotificarAlteracao();
        }
    }

    /// <summary>
    /// Ação de Undo/Redo para desagrupar objetos
    /// </summary>
    public class UngroupAction : IUndoableAction
    {
        private readonly SketchDocument _doc;
        private readonly GroupObject _grupo;
        private readonly List<BaseSketchObject> _membros;
        private readonly int _indiceInsercao;

        public string Descricao => $"Desagrupar {_membros.Count} objetos";

        public UngroupAction(SketchDocument doc, GroupObject grupo, List<BaseSketchObject> membros, int indiceInsercao)
        {
            _doc = doc;
            _grupo = grupo;
            _membros = new List<BaseSketchObject>(membros);
            _indiceInsercao = indiceInsercao;
        }

        public void Executar()
        {
            _doc.Objetos.Remove(_grupo);

            int indice = Math.Clamp(_indiceInsercao, 0, _doc.Objetos.Count);
            for (int i = 0; i < _membros.Count; i++)
            {
                _doc.Objetos.Insert(indice + i, _membros[i]);
            }

            _doc.NotificarAlteracao();
        }

        public void Desfazer()
        {
            foreach (var obj in _membros)
            {
                _doc.Objetos.Remove(obj);
            }

            int indice = Math.Clamp(_indiceInsercao, 0, _doc.Objetos.Count);
            _doc.Objetos.Insert(indice, _grupo);
            _doc.NotificarAlteracao();
        }
    }
}