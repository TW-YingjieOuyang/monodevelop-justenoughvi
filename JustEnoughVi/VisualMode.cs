﻿using System;
using Mono.TextEditor;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;

namespace JustEnoughVi
{
    public enum SelectMode
    {
        Normal,
        Line
    }

    public class VisualMode : ViMode
    {
        private int _startOffset;

        public SelectMode Select { get; set; }

        public VisualMode(TextEditorData editor) : base(editor)
        {
            // visual mode keys
            KeyMap.Add("d", SelectionCut);
            KeyMap.Add("x", SelectionCut);
            KeyMap.Add("<", IndentRemove);
            KeyMap.Add(">", IndentAdd);
            KeyMap.Add("y", Yank);
            KeyMap.Add("Y", Yank);

            // function key remaps
            KeyMap.Add("Delete", SelectionCut);
        }

        private bool SelectionCut(int count = 0, char[] args = null)
        {
            ClipboardActions.Cut(Editor);
            RequestedMode = Mode.Normal;
            return true;
        }

        private bool IndentRemove(int count = 0, char[] args = null)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                MiscActions.RemoveIndentSelection(Editor);
            }

            RequestedMode = Mode.Normal;
            return true;
        }

        private bool IndentAdd(int count = 0, char[] args = null)
        {
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                MiscActions.IndentSelection(Editor);
            }

            RequestedMode = Mode.Normal;
            return true;
        }

        private bool Yank(int count = 0, char[] args = null)
        {
            ClipboardActions.Copy(Editor);
            RequestedMode = Mode.Normal;
            return true;
        }

        #region implemented abstract members of ViMode

        private void UpdateSelection()
        {
            if (Select == SelectMode.Line)
            {
                var startLine = Editor.GetLineByOffset(_startOffset);
                var endLine = Editor.GetLineByOffset(Editor.Caret.Offset);
                SetSelectLines(startLine.LineNumber, endLine.LineNumber);
            }
            else
            {
                Editor.SetSelection(_startOffset + (Editor.Caret.Offset < _startOffset ? 1 : 0), Editor.Caret.Offset + (Editor.Caret.Offset >= _startOffset ? 1 : 0));
            }
        }

        protected override void Activate()
        {
            _startOffset = Editor.Caret.Offset;
            Editor.Caret.Mode = CaretMode.Block;
            UpdateSelection();
        }

        protected override void Deactivate()
        {
            Editor.Caret.Mode = CaretMode.Insert;
            Editor.ClearSelection();
        }

        public override bool KeyPress(KeyDescriptor descriptor)
        {
            var ret = base.KeyPress(descriptor);

            if (RequestedMode == Mode.None)
            {
                UpdateSelection();
            }

            return ret;
        }

        #endregion
    }
}

