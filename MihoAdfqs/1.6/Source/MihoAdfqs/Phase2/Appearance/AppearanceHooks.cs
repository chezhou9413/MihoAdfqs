using System;
using Verse;

namespace MihoAdfqs.Phase2.Appearance
{
    //职责：为独立表情兼容程序集提供显示状态接口，主程序集不引用FA类型。
    public static class AppearanceHooks
    {
        public static Func<Pawn, bool> UseAnimatedFace = pawn => false;
    }
}
