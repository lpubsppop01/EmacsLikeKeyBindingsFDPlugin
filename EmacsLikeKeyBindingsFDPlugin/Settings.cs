using PluginCore;
using PluginCore.Helpers;
using PluginCore.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

// ref. https://stackoverflow.com/questions/18840924/callermembername-in-net-4-0-not-working
namespace System.Runtime.CompilerServices
{
    sealed class CallerMemberNameAttribute : Attribute { }
}

namespace lpubsppop01.EmacsLikeKeyBindingsFDPlugin
{
    [Serializable]
    class Settings : INotifyPropertyChanged
    {
        #region Properties

        EmacsKeys[] m_DisabledKeys;
        /// <summary>
        /// Gets or sets the disabled Emacs keys bindings.
        /// </summary>
        [DisplayName("Disabled keys")]
        [DefaultValue(null)]
        public EmacsKeys[] DisabledKeys
        {
            get { return m_DisabledKeys; }
            set { m_DisabledKeys = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets the enabled Emacs keys bindings.
        /// </summary>
        [DisplayName("Enabled keys")]
        [DefaultValue(null)]
        public EmacsKeys[] EnabledKeys => DisabledKeys.Others().ToArray();

        /// <summary>
        /// Gets the overrided shortcut ids;
        /// </summary>
        [DisplayName("Overrided shortcuts")]
        [Description("These are not works.")]
        [DefaultValue(null)]
        public string[] OverridedShortcutIds => EnabledKeys.GetOverridedShortcutIds();

        bool m_SetsHighToKeyHandlingPriority;
        /// <summary>
        /// Gets or sets whether to set the key event handling priority to high.
        /// <para>Required for Ctrl-Space.</para>
        /// </summary>
        [DisplayName("Set high to key event handling priority")]
        [Description("Required for Ctrl-Space.")]
        [DefaultValue(true)]
        public bool SetsHighToKeyHandlingPriority
        {
            get { return m_SetsHighToKeyHandlingPriority; }
            set { m_SetsHighToKeyHandlingPriority = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets the key event handling priority.
        /// </summary>
        [DisplayName("Key event handling priority")]
        public HandlingPriority KeyHandlingPriority =>
            SetsHighToKeyHandlingPriority ? HandlingPriority.High : HandlingPriority.Normal;

        bool m_HandlesCtrlKeyDownMessage;
        /// <summary>
        /// Gets or sets whether to handle WM_KEYDOWN of Ctrl or not.
        /// <para>Requied to disable the completion list hiding.</para>
        /// </summary>
        [DisplayName("Handle WM_KEYDOWN of Ctrl")]
        [Description("Requied to disable the completion list hiding.")]
        [DefaultValue(true)]
        public bool HandlesCtrlKeyDownMessage
        {
            get { return m_HandlesCtrlKeyDownMessage; }
            set { m_HandlesCtrlKeyDownMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Reset and Copy

        public void Reset()
        {
            DisabledKeys = null;
            SetsHighToKeyHandlingPriority = true;
            HandlesCtrlKeyDownMessage = true;
        }

        public void CopyFrom(Settings src)
        {
            DisabledKeys = src.DisabledKeys != null ? src.DisabledKeys.Clone() as EmacsKeys[] : null;
            SetsHighToKeyHandlingPriority = src.SetsHighToKeyHandlingPriority;
            HandlesCtrlKeyDownMessage = src.HandlesCtrlKeyDownMessage;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Static Members

        static Settings m_Current;
        public static Settings Current
        {
            get
            {
                if (m_Current == null)
                {
                    m_Current = new Settings();
                    LoadCurrent();
                }
                return m_Current;
            }
        }

        static string DataDirectoryPath => Path.Combine(PathHelper.DataDir, "EmacsLikeKeyBindings");
        static string SettingsFilePath => Path.Combine(DataDirectoryPath, "Settings.fdb");

        static Settings()
        {
            LoadCurrent();
        }

        public static void SaveCurrent()
        {
            if (!Directory.Exists(DataDirectoryPath)) Directory.CreateDirectory(DataDirectoryPath);
            ObjectSerializer.Serialize(SettingsFilePath, Current);
        }

        public static void LoadCurrent()
        {
            if (File.Exists(SettingsFilePath))
            {
                var deserialized = ObjectSerializer.Deserialize(SettingsFilePath, new Settings()) as Settings;
                Current.CopyFrom(deserialized);
            }
            else
            {
                Current.Reset();
            }
        }

        #endregion
    }
}
