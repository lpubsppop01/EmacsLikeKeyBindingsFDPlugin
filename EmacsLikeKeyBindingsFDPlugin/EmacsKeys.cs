using PluginCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace lpubsppop01.EmacsLikeKeyBindingsFDPlugin
{
    enum EmacsKeys
    {
        Ctrl_f,
        Ctrl_b,
        Ctrl_a,
        Ctrl_e,
        Ctrl_n,
        Ctrl_p,
        Ctrl_d,
        Ctrl_h,
        Ctrl_w,
        Ctrl_k,
        Ctrl_y,
        Ctrl_Space,
        Ctrl_At,
        Ctrl_g,
        Ctrl_Slash,
        Alt_Slash,
        Ctrl_x_Ctrl_f,
    }

    static class EmacsKeysExtension
    {
        public static Keys[] ToFormsKeys(this EmacsKeys keys)
        {
            switch (keys)
            {
                case EmacsKeys.Ctrl_f: return new[] { Keys.F | Keys.Control };
                case EmacsKeys.Ctrl_b: return new[] { Keys.B | Keys.Control };
                case EmacsKeys.Ctrl_a: return new[] { Keys.A | Keys.Control };
                case EmacsKeys.Ctrl_e: return new[] { Keys.E | Keys.Control };
                case EmacsKeys.Ctrl_n: return new[] { Keys.N | Keys.Control };
                case EmacsKeys.Ctrl_p: return new[] { Keys.P | Keys.Control };
                case EmacsKeys.Ctrl_d: return new[] { Keys.D | Keys.Control };
                case EmacsKeys.Ctrl_h: return new[] { Keys.H | Keys.Control };
                case EmacsKeys.Ctrl_w: return new[] { Keys.W | Keys.Control };
                case EmacsKeys.Ctrl_k: return new[] { Keys.K | Keys.Control };
                case EmacsKeys.Ctrl_y: return new[] { Keys.Y | Keys.Control };
                case EmacsKeys.Ctrl_Space: return new[] { Keys.Space | Keys.Control };
                case EmacsKeys.Ctrl_At: return new[] { Keys.Oemtilde | Keys.Control };
                case EmacsKeys.Ctrl_g: return new[] { Keys.G | Keys.Control };
                case EmacsKeys.Ctrl_Slash: return new[] { Keys.Oem2 | Keys.Control };
                case EmacsKeys.Alt_Slash: return new[] { Keys.Oem2 | Keys.Alt };
                case EmacsKeys.Ctrl_x_Ctrl_f: return new[] { Keys.X | Keys.Control, Keys.F | Keys.Control };
            }
            return null;
        }

        public static ICollection<EmacsKeys> Others(this ICollection<EmacsKeys> src)
        {
            var all = Enum.GetValues(typeof(EmacsKeys));
            if (src == null) return all.OfType<EmacsKeys>().ToArray();
            return all.OfType<EmacsKeys>().Except(src).ToArray();
        }

        public static string[] GetOverridedShortcutIds(this ICollection<EmacsKeys> keys)
        {
            return keys.SelectMany(k => k.ToFormsKeys())
                .Select(k => PluginBase.MainForm.GetShortcutItemId(k))
                .Where(i => !string.IsNullOrEmpty(i)).ToArray();
        }
    }
}
