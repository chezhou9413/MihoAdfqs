using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MihoAdfqs.MihoAdfComp
{
    // 属性类保持不变
    public class ThingCompProperties_FueledSprayer : CompProperties
    {
        public int maxFuel = 60;
        public int fuelPerBurst = 1;
        public int ticksToRefuel = 120;
        public string fuelDef = "Chemfuel";
        public int fuelPerLoad = 1;
        public bool autoRefuel = true;

        public ThingCompProperties_FueledSprayer()
        {
            compClass = typeof(ThingComp_FueledSprayer);
        }
    }

    public class ThingComp_FueledSprayer : ThingComp
    {
        public ThingCompProperties_FueledSprayer Props => (ThingCompProperties_FueledSprayer)props;
        public int fuel;
        public bool needsReload;
        public int ticksUntilReloaded;

        public bool HasFuel => fuel >= Props.fuelPerBurst;
        public float FuelPercent => (float)fuel / Props.maxFuel;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            fuel = Props.maxFuel;
        }

        // 每次"开始一次喷射/一次burst"消耗一次燃料。
        // 这里我们把“消耗”挂在 Verb_ArcSprayIncinerator.TryCastShot（第一次子弹/第一tick）上，
        // 并用一个短暂的冷却窗口避免一次burst内重复扣。
        public int lastConsumeTick = -999999;

        public bool TryConsumeFuelOncePerBurst(Verb verb)
        {
            // 没地图/没tick就直接按一次处理
            int nowTick = Find.TickManager?.TicksGame ?? 0;

            // Verb在burst期间会连续调用 TryCastShot。我们用 verb.burstShotsLeft 判定“第一次”。
            // 第一次调用时 burstShotsLeft 通常 == ShotsPerBurst（即 BurstShotCount）。
            // 注意：Verb.burstShotsLeft / ShotsPerBurst 是 protected，外部 Comp 不能访问。
            // 这里改成纯 tick 窗口：一次burst内 TryCastShot 会在很短时间内多次调用，因此 10tick 窗口足够避免多扣。
            bool inConsumeWindow = nowTick - lastConsumeTick < 10;
            if (inConsumeWindow)
            {
                return true;
            }

            if (fuel >= Props.fuelPerBurst)
            {
                fuel -= Props.fuelPerBurst;
                lastConsumeTick = nowTick;
                if (fuel < Props.fuelPerBurst) needsReload = true;
                return true;
            }

            needsReload = true;
            return false;
        }

        public override void CompTick()
        {
            base.CompTick();
            // 自动装填改为由 Pawn.Tick 补丁驱动（因为在这里拿不到正确的持枪Pawn）
        }
        public bool CanAutoRefuel(Pawn p)
        {
            if (!Props.autoRefuel) return false;
            if (p == null) return false;
            if (p.inventory == null) return false;
            if (!needsReload && fuel >= Props.maxFuel) return false;
            return true;
        }

        public bool TryAutoRefuelFromInventory(Pawn p)
        {
            if (!CanAutoRefuel(p)) return false;
            ThingDef fuelThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(Props.fuelDef);
            if (fuelThingDef == null) return false;
            Thing fuelItem = p.inventory.innerContainer.FirstOrDefault(t => t.def == fuelThingDef);
            if (fuelItem == null) return false;
            if (fuelItem.stackCount < Props.fuelPerLoad) return false;
            fuelItem.SplitOff(Props.fuelPerLoad);
            fuel = Props.maxFuel;
            needsReload = false;
            SoundDef reload = SoundDef.Named("Standard_Reload");
            reload?.PlayOneShot(new TargetInfo(p.Position, p.Map));
            return true;
        }

        public override string CompInspectStringExtra()
        {
            return "Fuel: " + fuel + "/" + Props.maxFuel + (needsReload ? " (Needs Reloading)" : "");
        }

        public void RefuelFromItem(Thing item)
        {
            if (item == null || item.Destroyed) return;
            int amountNeeded = Props.maxFuel - fuel;
            int amountToTake = Mathf.Min(amountNeeded, item.stackCount);

            if (amountToTake > 0)
            {
                // 从物品堆里切分出来并销毁
                item.SplitOff(amountToTake).Destroy();

                // 加油
                fuel += amountToTake;
                needsReload = false;

                // 播放装填音效
                SoundDef reload = SoundDef.Named("Standard_Reload");
                reload?.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref fuel, "fuel", Props.maxFuel);
            Scribe_Values.Look(ref needsReload, "needsReload");
            Scribe_Values.Look(ref ticksUntilReloaded, "ticksUntilReloaded");
        }
    }

    [HarmonyPatch(typeof(Verb_ArcSprayIncinerator), "TryCastShot")]
    public static class Patch_Verb_ArcSprayIncinerator_TryCastShot
    {
        // 缓存反射字段，为了性能（虽然在燃料耗尽才调用一次，性能影响很小，但这是好习惯）
        private static FieldInfo burstShotsLeftField = AccessTools.Field(typeof(Verb), "burstShotsLeft");

        [HarmonyPrefix]
        public static bool Prefix(Verb_ArcSprayIncinerator __instance, ref bool __result)
        {
            // 基础检查
            if (!__instance.CasterIsPawn || __instance.EquipmentSource == null) return true;

            var comp = __instance.EquipmentSource.GetComp<MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer>();
            Pawn pawn = __instance.CasterPawn;

            if (comp != null)
            {
                // 1. 自动装填尝试
                if (!comp.HasFuel)
                {
                    comp.needsReload = true;
                    comp.TryAutoRefuelFromInventory(pawn);
                }

                // 2. 尝试扣除燃料
                if (!comp.TryConsumeFuelOncePerBurst(__instance))
                {
                    Messages.Message("燃料不足！", MessageTypeDefOf.RejectInput);

                    // === 修复点 1：使用反射修改受保护的 burstShotsLeft ===
                    // 将剩余连射数设为 0
                    burstShotsLeftField.SetValue(__instance, 0);

                    // === 修复点 2：重置姿态，消除冷却硬直 ===
                    if (pawn.stances != null)
                    {
                        pawn.stances.SetStance(new Stance_Mobile());
                    }

                    // === 修复点 3：移除 IsCombat 检查，直接结束任务 ===
                    // 既然在射击，当前任务肯定是可以被打断的动作
                    if (pawn.jobs?.curJob != null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }

                    __result = false; // 拦截原方法
                    return false;     // 不执行原方法
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class Patch_Pawn_Tick_Refuel
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            // 每60ticks检查一次是否需要装填
            if (__instance.IsHashIntervalTick(60) && __instance.equipment != null)
            {
                foreach (var eq in __instance.equipment.AllEquipmentListForReading)
                {
                    var comp = eq.GetComp<ThingComp_FueledSprayer>();
                    if (comp != null)
                    {
                        // 自动补充：只要缺燃料就尝试从背包补（不依赖 parent.ParentHolder）
                        if (comp.needsReload || comp.fuel < comp.Props.maxFuel)
                        {
                            comp.TryAutoRefuelFromInventory(__instance);
                        }
                    }
                }
            }
        }
    }
}