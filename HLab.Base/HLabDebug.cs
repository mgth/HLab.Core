using System.Diagnostics;

namespace HLab.Base
{
    public static class HLabDebug
    {
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugBreak()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
    }
}
