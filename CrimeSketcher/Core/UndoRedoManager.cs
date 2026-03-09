// Core/UndoRedoManager.cs
using System;
using System.Collections.Generic;

namespace CrimeSketcher.Core
{
    public interface IUndoableAction
    {
        string Descricao { get; }
        void Executar();
        void Desfazer();
    }

    public class AddObjectAction : IUndoableAction
    {
        private SketchDocument _doc;
        private Objects.BaseSketchObject _obj;

        public string Descricao => $"Adicionar {_obj.Tipo}";

        public AddObjectAction(SketchDocument doc, Objects.BaseSketchObject obj)
        {
            _doc = doc;
            _obj = obj;
        }

        public void Executar() => _doc.AdicionarObjeto(_obj, false);
        public void Desfazer() => _doc.RemoverObjeto(_obj, false);
    }

    public class RemoveObjectAction : IUndoableAction
    {
        private SketchDocument _doc;
        private Objects.BaseSketchObject _obj;

        public string Descricao => $"Remover {_obj.Tipo}";

        public RemoveObjectAction(SketchDocument doc, Objects.BaseSketchObject obj)
        {
            _doc = doc;
            _obj = obj;
        }

        public void Executar() => _doc.RemoverObjeto(_obj, false);
        public void Desfazer() => _doc.AdicionarObjeto(_obj, false);
    }

    public class MoveObjectAction : IUndoableAction
    {
        private Objects.BaseSketchObject _obj;
        private PointF _posAnterior;
        private PointF _posNova;

        public string Descricao => $"Mover {_obj.Tipo}";

        public MoveObjectAction(Objects.BaseSketchObject obj,
            System.Drawing.PointF posAnterior, System.Drawing.PointF posNova)
        {
            _obj = obj;
            _posAnterior = new PointF(posAnterior.X, posAnterior.Y);
            _posNova = new PointF(posNova.X, posNova.Y);
        }

        public void Executar() => _obj.Posicao =
            new System.Drawing.PointF(_posNova.X, _posNova.Y);
        public void Desfazer() => _obj.Posicao =
            new System.Drawing.PointF(_posAnterior.X, _posAnterior.Y);

        private struct PointF
        {
            public float X, Y;
            public PointF(float x, float y) { X = x; Y = y; }
        }
    }

    public class UndoRedoManager
    {
        private Stack<IUndoableAction> _undoStack = new Stack<IUndoableAction>();
        private Stack<IUndoableAction> _redoStack = new Stack<IUndoableAction>();

        public event EventHandler EstadoAlterado;

        public bool PodeDesfazer => _undoStack.Count > 0;
        public bool PodeRefazer => _redoStack.Count > 0;

        public string ProximoUndo =>
            PodeDesfazer ? _undoStack.Peek().Descricao : "";
        public string ProximoRedo =>
            PodeRefazer ? _redoStack.Peek().Descricao : "";

        public void ExecutarAcao(IUndoableAction acao)
        {
            acao.Executar();
            _undoStack.Push(acao);
            _redoStack.Clear();
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void RegistrarAcao(IUndoableAction acao)
        {
            _undoStack.Push(acao);
            _redoStack.Clear();
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void Desfazer()
        {
            if (!PodeDesfazer) return;
            var acao = _undoStack.Pop();
            acao.Desfazer();
            _redoStack.Push(acao);
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void Refazer()
        {
            if (!PodeRefazer) return;
            var acao = _redoStack.Pop();
            acao.Executar();
            _undoStack.Push(acao);
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }

        public void Limpar()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }
    }
}