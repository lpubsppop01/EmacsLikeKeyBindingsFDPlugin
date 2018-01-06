using PluginCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace lpubsppop01.EmacsLikeKeyBindingsFDPlugin
{
    static class IMainFormExtension
    {
        static object InvokeMethod<T1, T2>(IMainForm mainForm, string methodName, T1 arg1, T2 arg2)
        {
            var method = mainForm.GetType().GetMethod(methodName, new Type[] { typeof(T1), typeof(T2) });
            if (method == null) return null;
            return method.Invoke(mainForm, new object[] { arg1, arg2 });
        }

        public static void FindNext(this IMainForm mainForm, Object sender, EventArgs e)
        {
            InvokeMethod(mainForm, "FindNext", sender, e);
        }

        public static void FindPrevious(this IMainForm mainForm, Object sender, EventArgs e)
        {
            InvokeMethod(mainForm, "FindPrevious", sender, e);
        }

        public static void ShowQuickFind(this IMainForm mainForm, Object sender, EventArgs e)
        {
            InvokeMethod(mainForm, "QuickFind", sender, e);
        }

        public static void SetFindText(this IMainForm mainForm, Object sender, String text)
        {
            InvokeMethod(mainForm, "SetFindText", sender, text);
        }

        public static System.Windows.Forms.ToolStrip GetQuickFind(this IMainForm mainForm)
        {
            var field = mainForm.GetType().GetField("quickFind", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return null;
            return field.GetValue(mainForm) as System.Windows.Forms.ToolStrip;
        }

        public static bool QuickFindIsVisible(this IMainForm mainForm)
        {
            var quickFind = GetQuickFind(mainForm);
            if (quickFind == null) return false;
            return quickFind.Visible;
        }
    }
}
