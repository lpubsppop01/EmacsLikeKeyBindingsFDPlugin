using PluginCore;
using PluginCore.Controls;
using PluginCore.Managers;
using ScintillaNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace lpubsppop01.EmacsLikeKeyBindingsFDPlugin
{
    public class PluginMain : IPlugin, IMessageFilter
    {
        #region Properties

        ScintillaControl SciControl
        {
            get { return PluginBase.MainForm.CurrentDocument.SciControl; }
        }

        bool SciControlIsFocus
        {
            get
            {
                return PluginBase.MainForm.CurrentDocument != null &&
                    PluginBase.MainForm.CurrentDocument.SciControl.IsFocus;
            }
        }

        bool QuickFindIsFocus
        {
            get
            {
                var quickFind = PluginBase.MainForm.GetQuickFind();
                if (quickFind == null) return false;
                return quickFind.ContainsFocus;
            }
        }

        #endregion

        #region IPlugin Members

        int IPlugin.Api => 1;

        string IPlugin.Name => "EmacsLikeKeyBindings";

        string IPlugin.Guid => "91befa26-3ed5-4b0e-9884-e75495082a95";

        string IPlugin.Help => "";

        string IPlugin.Author => "lpubsppop01";

        string IPlugin.Description => "Provides Emacs like key bindings.";

        object IPlugin.Settings => Settings.Current;

        void IPlugin.Initialize()
        {
            rootDict = currDict = CreateRootDictionary();
            foreach (var key in EnumerateEnabledKeys(rootDict))
            {
                PluginBase.MainForm.IgnoredKeys.Add(key);
            }
            PluginBase.MainForm.IgnoredKeys.Add(Keys.ControlKey);
            EventManager.AddEventHandler(this, EventType.Keys, Settings.Current.KeyHandlingPriority);
            Application.AddMessageFilter(this);
            Settings.Current.PropertyChanged += Current_PropertyChanged;
        }

        void IPlugin.Dispose()
        {
            Settings.Current.PropertyChanged -= Current_PropertyChanged;
            Settings.SaveCurrent();
            Application.RemoveMessageFilter(this);
            EventManager.RemoveEventHandler(this);
        }

        void IEventHandler.HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type == EventType.Keys)
            {
                HandleKeys(sender, e as KeyEvent);
            }
        }

        void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisabledKeys")
            {
                rootDict = currDict = CreateRootDictionary();
            }
            else if (e.PropertyName == "SetsHighToKeyHandlingPriority")
            {
                EventManager.RemoveEventHandler(this);
                EventManager.AddEventHandler(this, EventType.Keys, Settings.Current.KeyHandlingPriority);
            }
        }

        #endregion

        #region IMessageFilter Members

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            // Disable hiding of completion list
            if (Settings.Current.HandlesCtrlKeyDownMessage &&
                m.Msg == Win32.WM_KEYDOWN &&
                (int)m.WParam == 17) // Ctrl
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Key Event Handler

        bool isTempDisabled = false;

        void HandleKeys(object sender, KeyEvent e)
        {
            if (!SciControlIsFocus && !QuickFindIsFocus) return;
            if (isTempDisabled) return;

            KeymapValue value;
            if (currDict.TryGetValue(e.Value, out value))
            {
                e.Handled = true;
                if (value.Action != null)
                {
                    value.Action(sender);
                    currDict = rootDict;
                }
                else if (value.NextDictionary != null)
                {
                    currDict = value.NextDictionary;
                }
            }
            else
            {
                currDict = rootDict;
            }
        }

        #endregion

        #region Keymap Dictionary

        class KeymapValue
        {
            #region Constructors

            public KeymapValue(Action<object> action)
            {
                Action = action;
            }

            public KeymapValue(Dictionary<Keys, KeymapValue> nextDict)
            {
                NextDictionary = nextDict;
            }

            #endregion

            #region Properties

            public Action<object> Action { get; set; }
            public Dictionary<Keys, KeymapValue> NextDictionary { get; set; }

            #endregion
        }

        Dictionary<Keys, KeymapValue> rootDict;
        Dictionary<Keys, KeymapValue> currDict;

        Dictionary<Keys, KeymapValue> CreateRootDictionary()
        {
            var dict = new Dictionary<Keys, KeymapValue>();
            var disabledKeys = new HashSet<EmacsKeys>(
                Settings.Current.DisabledKeys != null ? Settings.Current.DisabledKeys : new EmacsKeys[0]);
            foreach (var binding in EnumerateAllKeyBindings())
            {
                if (disabledKeys.Contains(binding.Keys)) continue;
                var keys = binding.Keys.ToFormsKeys();
                if (keys.Length == 1)
                {
                    dict[keys[0]] = binding.KeymapValue;
                }
                else if (keys.Length == 2)
                {
                    KeymapValue firstValue;
                    if (!dict.TryGetValue(keys[0], out firstValue))
                    {
                        dict[keys[0]] = firstValue = new KeymapValue(new Dictionary<Keys, KeymapValue>());
                    }
                    firstValue.NextDictionary[keys[1]] = binding.KeymapValue;
                }
            }
            return dict;
        }

        class KeyBinding
        {
            #region Constructor

            public KeyBinding(EmacsKeys keys, KeymapValue keymapValue)
            {
                Keys = keys;
                KeymapValue = keymapValue;
            }

            #endregion

            #region Properties

            public EmacsKeys Keys;
            public KeymapValue KeymapValue;

            #endregion
        }

        IEnumerable<KeyBinding> EnumerateAllKeyBindings()
        {
            yield return new KeyBinding(EmacsKeys.Ctrl_f, new KeymapValue(s => ForwardChar()));
            yield return new KeyBinding(EmacsKeys.Ctrl_b, new KeymapValue(s => BackwardChar()));
            yield return new KeyBinding(EmacsKeys.Ctrl_a, new KeymapValue(s => MoveBeginningOfLine()));
            yield return new KeyBinding(EmacsKeys.Ctrl_e, new KeymapValue(s => MoveEndOfLine()));
            yield return new KeyBinding(EmacsKeys.Ctrl_n, new KeymapValue(s => NextLine(s)));
            yield return new KeyBinding(EmacsKeys.Ctrl_p, new KeymapValue(s => PreviousLine()));
            yield return new KeyBinding(EmacsKeys.Ctrl_d, new KeymapValue(s => DeleteChar()));
            yield return new KeyBinding(EmacsKeys.Ctrl_h, new KeymapValue(s => DeleteBackwardChar()));
            yield return new KeyBinding(EmacsKeys.Ctrl_w, new KeymapValue(s => KillRegion()));
            yield return new KeyBinding(EmacsKeys.Ctrl_k, new KeymapValue(s => KillLine()));
            yield return new KeyBinding(EmacsKeys.Ctrl_y, new KeymapValue(s => Yank()));
            yield return new KeyBinding(EmacsKeys.Ctrl_Slash, new KeymapValue(s => Undo()));
            yield return new KeyBinding(EmacsKeys.Ctrl_Space, new KeymapValue(s => SetMark()));
            yield return new KeyBinding(EmacsKeys.Ctrl_At, new KeymapValue(s => SetMark()));
            yield return new KeyBinding(EmacsKeys.Ctrl_g, new KeymapValue(s => KeyboardQuit(s)));
            yield return new KeyBinding(EmacsKeys.Alt_Slash, new KeymapValue(s => DabbrevExpand(s)));
            yield return new KeyBinding(EmacsKeys.Ctrl_x_Ctrl_f, new KeymapValue(s => FindFile(s)));
            yield return new KeyBinding(EmacsKeys.Ctrl_s, new KeymapValue(s => ISearchForward(s)));
            yield return new KeyBinding(EmacsKeys.Ctrl_r, new KeymapValue(s => ISearchBackward(s)));
        }

        IEnumerable<Keys> EnumerateEnabledKeys(Dictionary<Keys, KeymapValue> dict)
        {
            foreach (var pair in dict)
            {
                yield return pair.Key;
                if (pair.Value.NextDictionary == null) continue;
                foreach (var key in EnumerateEnabledKeys(pair.Value.NextDictionary)) yield return key;
            }
        }

        #endregion

        #region Action Implementations

        bool marked = false;
        string killedTextBuf;

        void SetKilledTextToClipboard(string currKilledText, bool combinesWithBuf)
        {
            string killedText = currKilledText;
            if (combinesWithBuf && !string.IsNullOrEmpty(killedTextBuf))
            {
                killedText = killedTextBuf + killedText;
            }
            Clipboard.SetText(killedText);
            killedTextBuf = killedText;
        }

        void ClearKilledTextBuffer()
        {
            killedTextBuf = null;
        }

        void RedispatchKeyEvent(object sender, Keys value)
        {
            isTempDisabled = true;
            try
            {
                var newEvent = new KeyEvent(EventType.Keys, value);
                EventManager.DispatchEvent(sender, newEvent);
            }
            finally { isTempDisabled = false; }
        }

        void ForwardChar()
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.CharRightExtend();
            else SciControl.CharRight();
        }

        void BackwardChar()
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.CharLeftExtend();
            else SciControl.CharLeft();
        }

        void MoveBeginningOfLine()
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.HomeExtend();
            else SciControl.Home();
        }

        void MoveEndOfLine()
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.LineEndExtend();
            else SciControl.LineEnd();
        }

        void NextLine(object sender)
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.LineDownExtend();
            else if (CompletionList.Active) CompletionList.HandleKeys(SciControl, Keys.Down);
            else SciControl.LineDown();
        }

        void PreviousLine()
        {
            ClearKilledTextBuffer();
            if (marked) SciControl.LineUpExtend();
            else if (CompletionList.Active) CompletionList.HandleKeys(SciControl, Keys.Up);
            else SciControl.LineUp();
        }

        void DeleteChar()
        {
            marked = false;
            ClearKilledTextBuffer();
            SciControl.DeleteForward();
        }

        void DeleteBackwardChar()
        {
            marked = false;
            ClearKilledTextBuffer();
            SciControl.DeleteBack();
        }

        void KillRegion()
        {
            marked = false;
            ClearKilledTextBuffer();
            SciControl.Cut();
        }

        void KillLine()
        {
            marked = false;
            int startPos = SciControl.CurrentPos;
            int endPos = SciControl.LineEndPosition(SciControl.CurrentLine);
            if (startPos == endPos)
            {
                endPos = SciControl.PositionFromLine(SciControl.CurrentLine + 1);
                if (startPos == endPos) return;
            }
            string currLineText = SciControl.GetCurLine(SciControl.CurLineSize);
            int linePos = SciControl.PositionFromLine(SciControl.CurrentLine);
            string cutText = currLineText.Substring(startPos - linePos, endPos - startPos);
            SetKilledTextToClipboard(cutText, combinesWithBuf: true);
            SciControl.DeleteRange(startPos, endPos - startPos);
            SciControl.ChooseCaretX();
        }

        void Yank()
        {
            ClearKilledTextBuffer();
            marked = false;
            SciControl.Paste();
        }

        void Undo()
        {
            ClearKilledTextBuffer();
            marked = false;
            SciControl.Undo();
        }

        void SetMark()
        {
            ClearKilledTextBuffer();
            marked = true;
        }

        void KeyboardQuit(object sender)
        {
            var quickFind = PluginBase.MainForm.GetQuickFind();
            if (quickFind != null && quickFind.ContainsFocus)
            {
                PluginBase.MainForm.SetFindText(null, "");
                quickFind.Hide();
                SciControl.Focus();
                return;
            }

            ClearKilledTextBuffer();
            marked = false;
            if (SciControl.SelectionStart != SciControl.SelectionEnd)
            {
                var backupPos = SciControl.CurrentPos;
                SciControl.ClearSelections(); // Caret move to head
                SciControl.GotoPos(backupPos);
            }
            RedispatchKeyEvent(sender, Keys.Escape);
        }

        void DabbrevExpand(object sender)
        {
            ClearKilledTextBuffer();
            marked = false;
            RedispatchKeyEvent(sender, Keys.Space | Keys.Control);
        }

        void FindFile(object sender)
        {
            ClearKilledTextBuffer();
            marked = false;
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PluginBase.MainForm.OpenEditableDocument(dialog.FileName);
            }
        }

        void ISearchForward(object sender)
        {
            ClearKilledTextBuffer();
            marked = false;
            if (QuickFindIsFocus)
            {
                PluginBase.MainForm.FindNext(sender, EventArgs.Empty);
            }
            else
            {
                PluginBase.MainForm.ShowQuickFind(sender, EventArgs.Empty);
            }
        }

        void ISearchBackward(object sender)
        {
            ClearKilledTextBuffer();
            marked = false;
            if (QuickFindIsFocus)
            {
                PluginBase.MainForm.FindPrevious(sender, EventArgs.Empty);
            }
            else
            {
                PluginBase.MainForm.ShowQuickFind(sender, EventArgs.Empty);
            }
        }

        #endregion
    }
}
