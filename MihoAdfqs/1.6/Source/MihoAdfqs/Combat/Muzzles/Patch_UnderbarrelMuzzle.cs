using HarmonyLib;
using MihoAdfqs.Combat.Weapons;
using UnityEngine;
using Verse;
using WeaponMuzzleFramework.Runtime;

namespace MihoAdfqs.Combat.Muzzles
{
    //职责：让选中的下挂动词使用独立枪口，并沿用库的旋转、后坐力及墙体限制。
    [HarmonyPatch(typeof(MuzzlePose), nameof(MuzzlePose.Offset))]
    public static class Patch_UnderbarrelMuzzle
    {
        //职责：在库计算世界偏移前平移贴图坐标，同时覆盖弹丸、火光和瞄准标记。
        public static void Prefix(Thing weapon, ref Vector2 position)
        {
            UnderbarrelMuzzleExtension extension = weapon.def.GetModExtension<UnderbarrelMuzzleExtension>();
            if (extension == null) return;
            foreach (Verb verb in weapon.TryGetComp<CompEquippable>().AllVerbs)
            {
                if (verb is Verb_Magazine magazine && magazine.Selected
                    && ((VerbProperties_Magazine)verb.verbProps).secondary)
                {
                    position += extension.offset;
                    return;
                }
            }
        }
    }
}
