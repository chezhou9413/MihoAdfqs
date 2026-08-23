using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MihoAdfqs.ThingClass
{
   public class myCareerThing : ThingWithComps
    {
        public int lastUsedTick = -999999;

        // 冷却时间常量：7天 = 420,000 Ticks
        private const int CoolDownTicks = 420000;

        public override void ExposeData()
        {
            base.ExposeData();
            // 保存上次使用的时间
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -999999);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                yield return opt;
            }

            // --- 冷却逻辑判断 ---
            int ticksPassed = Find.TickManager.TicksGame - lastUsedTick;
            bool isInCooldown = ticksPassed < CoolDownTicks;

            string label = "对目标对象进行洗脑";

            if (isInCooldown)
            {
                // 计算剩余天数，四舍五入保留一位小数
                float daysLeft = (float)(CoolDownTicks - ticksPassed) / 60000f;
                label += $" (冷却中: 剩余 {daysLeft:F1} 天)";
            }

            // 如果在冷却中，创建一个灰色的不可点击选项
            if (isInCooldown)
            {
                yield return new FloatMenuOption(label, null); // 第二个参数传 null 意味着不可点击
            }
            else
            {
                yield return new FloatMenuOption(label, () =>
                {
                    // 选择目标的逻辑（你原来的代码）
                    StartTargeting(selPawn);
                });
            }
        }

        private void StartTargeting(Pawn selPawn)
        {
            TargetingParameters targetParams = new TargetingParameters();
            targetParams.canTargetPawns = true;
            targetParams.validator = (TargetInfo t) =>
            {
                if (t.Thing is Pawn p && !p.Dead)
                {
                    bool belongsToPlayer = (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer);
                    bool isCorrectType = p.IsColonist || p.IsSlave || p.IsPrisonerOfColony;
                    return belongsToPlayer && isCorrectType;
                }
                return false;
            };

            Find.Targeter.BeginTargeting(targetParams, (LocalTargetInfo target) =>
            {
                Job job = JobMaker.MakeJob(MihoDefRef.Miho_ToMyCareerFoPawn, this, target);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job);
            });
        }
    }
}
