using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Muzzles
{
    //职责：保存下挂相对主枪口的完整贴图归一化偏移，与主枪共享可视化校准量。
    public sealed class UnderbarrelMuzzleExtension : DefModExtension
    {
        public Vector2 offset;
    }
}
