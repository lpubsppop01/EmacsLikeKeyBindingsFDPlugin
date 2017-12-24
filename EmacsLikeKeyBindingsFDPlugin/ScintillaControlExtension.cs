using ScintillaNet;

namespace lpubsppop01.EmacsLikeKeyBindingsFDPlugin
{
    static class ScintillaControlExtension
    {
        public static void DeleteRange(this ScintillaControl control, int start, int length)
        {
            const int SCI_DELETERANGE = 2645; // Defined in Scintilla.h of Scintilla
            control.SPerform(SCI_DELETERANGE, start, length);
        }

        public static void ClearSelections(this ScintillaControl control)
        {
            const int SCI_CLEARSELECTIONS = 2571; // Defined in Scintilla.h of Scintilla
            control.SPerform(SCI_CLEARSELECTIONS, 0, 0);
        }
    }
}
