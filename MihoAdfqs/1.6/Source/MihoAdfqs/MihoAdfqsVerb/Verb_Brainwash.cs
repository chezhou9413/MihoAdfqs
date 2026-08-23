using MihoAdfqs.MihoAdfDefRef;
using MihoAdfqs.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MihoAdfqs.MihoAdfqsVerb
{
    public class Verb_Brainwash : Verb_CastAbility
    {
        public override bool Targetable => true;
        public override bool MultiSelect => true;

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return base.CanHitTargetFrom(root, targ);
        }

        /// <summary>
        /// 验证目标：只能是己方殖民者、奴隶或囚犯
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            // 1. 基础检查
            if (!base.ValidateTarget(target, showMessages))
            {
                return false;
            }

            // 2. 必须是有生命的实体
            if (!target.HasThing || !(target.Thing is Pawn p))
            {
                if (showMessages)
                {
                    Messages.Message("必须选择一个人。", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (!p.RaceProps.Humanlike)
            {
                if (showMessages)
                {
                    Messages.Message("目标必须是人类。", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            bool isMyPawn = p.Faction == Faction.OfPlayer || p.IsPrisonerOfColony;

            if (!isMyPawn)
            {
                if (showMessages)
                {
                    Messages.Message("只能对自己殖民地的成员、奴隶或囚犯使用。", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 释放逻辑：执行洗脑
        /// </summary>
        protected override bool TryCastShot()
        {
            Pawn targetPawn = this.currentTarget.Pawn;

            if (targetPawn != null)
            {
                Pawn caster = this.CasterPawn;
                Thing targetThing = this.currentTarget.Thing;

                if (caster != null && targetThing != null)
                {
                    JobDef jobDef = MihoDefRef.Miho_Brainwash;
                    Job job = JobMaker.MakeJob(jobDef, targetThing);
                    job.ability = this.Ability;
                    job.count = 1;
                    caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
                return true;
            }

            // 4. 处理冷却时间
            if (this.Ability != null)
            {
                int cd = this.Ability.def.cooldownTicksRange.RandomInRange;
                if (cd > 0)
                    this.Ability.StartCooldown(cd);
            }

            return true;
        }
    }
}
