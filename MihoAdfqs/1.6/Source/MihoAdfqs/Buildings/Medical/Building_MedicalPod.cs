using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Medicine;
using RimWorld;
using UnityEngine;
using Verse;
using MihoAdfqs.Phase2.UI;
using Verse.AI;
using MihoAdfqs.Buildings.UI;

namespace MihoAdfqs.Buildings.Medical
{
    //容纳一个患者或尸体，管理药物容量和通电期间的治疗流程。
    public class Building_MedicalPod : Building_CryptosleepCasket, IThingHolderWithDrawnPawn
    {
        private Graphic interior;
        public float medicine;
        private string operation = "治疗";
        private int remainingTicks;
        private int operationTicks;
        private BodyPartRecord restoringPart;
        public Pawn Patient => ContainedThing is Corpse corpse ? corpse.InnerPawn : ContainedThing as Pawn;
        public bool Powered => GetComp<CompPowerTrader>().PowerOn && FlickUtility.WantsToBeOn(this);
        public string Operation => operation;
        public int RemainingTicks => remainingTicks;
        public float OperationProgress => operationTicks > 0 ? 1f - (float)remainingTicks / operationTicks : 0f;
        public float HeldPawnDrawPos_Y => DrawPos.y + Altitudes.AltInc;
        public float HeldPawnBodyAngle => Rotation.AsAngle;
        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        //在透明顶盖下面绘制内衬，贴图随建筑静态网格缓存。
        public override void Print(SectionLayer layer)
        {
            if (interior == null)
                interior = GraphicDatabase.Get<Graphic_Single>(
                    "Building/MihoPhase2/MedicalPod/MihoPhase2_SupernovaMedicalPodInterior_south",
                    ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
            Vector3 center = this.TrueCenter() + def.graphicData.DrawOffsetForRot(Rotation);
            center.y = AltitudeLayer.Building.AltitudeFor() - 0.01f;
            Printer_Plane.PrintPlane(layer, center, interior.drawSize, interior.MatSingle);
            base.Print(layer);
        }

        //沿用原版生长舱的分阶段绘制，让舱内患者保持躺卧并使用自身种族和服装图层。
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            Pawn patient = Patient;
            if (patient != null)
            {
                Vector3 offset = Rotation.FacingCell.ToVector3() * 0.45f + def.graphicData.DrawOffsetForRot(Rotation);
                patient.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + offset, Rot4.South, neverAimWeapon: true);
            }
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }

        //顶盖位于患者头发和服装上方，透明玻璃保留舱内可见的身体。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Graphic.Draw(drawLoc + Altitudes.AltIncVect * 4f, Rotation, this);
        }

        //职责：只接受单名人形患者或其尸体，并要求舱内存在药物。
        public override bool Accepts(Thing thing) => !HasAnyContents && medicine > 0 &&
            (thing is Pawn p && p.RaceProps.Humanlike || thing is Corpse c && c.InnerPawn.RaceProps.Humanlike);

        //职责：接收患者后从普通治疗开始，避免沿用上一名患者的手术。
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!base.TryAcceptThing(thing, allowSpecialEffects)) return false;
            operation = "治疗";
            remainingTicks = 0;
            return true;
        }

        //职责：根据药物种类换算填充单位，最多保存二百点。
        public void AddMedicine(Thing stack)
        {
            int value = MedicineValue(stack.def);
            int count = Mathf.Min(stack.stackCount, Mathf.FloorToInt((200f - medicine) / value));
            if (count <= 0) return;
            medicine += count * value;
            stack.SplitOff(count).Destroy();
        }

        //职责：仅在通电时累计操作时间，治疗按每秒每部件一点执行。
        protected override void Tick()
        {
            base.Tick();
            if (!Powered || Patient == null || medicine <= 0 && remainingTicks == 0) return;
            if (!Patient.Dead && this.IsHashIntervalTick(60)) MedicalUtility.Treat(Patient, 1);
            if (remainingTicks <= 0 || --remainingTicks > 0) return;
            Pawn patient = Patient;
            if (operation == "修复肢体") patient.health.RestorePart(restoringPart);
            else if (operation == "复活")
            {
                EjectContents();
                if (!ResurrectionUtility.TryResurrect(patient)) Log.Error("医疗舱无法复活目标：" + patient);
            }
            else if (operation == "转换为帝国美狐" || operation == "转换为帝国米莉拉") RaceConversion.Convert(patient, operation.EndsWith("米莉拉"));
            operation = "治疗";
            restoringPart = null;
        }

        //弹出按钮明确说明会中断当前手术。
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action command && command.action?.Method.Name == nameof(EjectContents))
                {
                    command.defaultLabel = "弹出患者";
                    command.defaultDesc = "将患者或尸体放在舱外；正在进行的手术会取消，已支付的药物点数不返还。";
                }
                yield return gizmo;
            }
        }

        //显示手术不能开始的具体原因，面板和执行阶段共用条件。
        public string OperationBlockReason(string label, int cost, BodyPartRecord part = null)
        {
            if (!Spawned || Faction != Faction.OfPlayer) return "医疗舱不可操作";
            if (Patient == null) return "请先放入患者或尸体";
            if (remainingTicks > 0) return "正在执行" + operation + "，请等待完成或弹出患者";
            if (!Powered) return "需要通电并开启医疗舱";
            if (medicine < cost) return $"需要{cost}点药物，当前只有{medicine:F0}点";
            if (Patient.Dead && label != "复活") return "尸体只能执行复活";
            if (!Patient.Dead && label == "复活") return "患者仍然存活";
            if (label == "修复肢体" && !Patient.health.hediffSet.PartIsMissing(part)) return "该身体部件已恢复";
            if (label == "转换为帝国米莉拉" && !Phase2.Compatibility.OptionalMods.Milira) return "未加载米莉拉种族模组";
            return null;
        }

        //操作开始时扣费，重新检查确认窗口打开期间可能变化的条件。
        public void StartOperation(string label, int cost, int ticks, BodyPartRecord part = null)
        {
            string reason = OperationBlockReason(label, cost, part);
            if (reason != null) { Messages.Message(reason, this, MessageTypeDefOf.RejectInput, false); return; }
            medicine -= cost;
            operation = label;
            operationTicks = remainingTicks = ticks;
            restoringPart = part;
        }

        //药物容量按实际种类换算，搬运数量不超过剩余容量。
        public static int MedicineValue(ThingDef medicineDef) => medicineDef == ThingDefOf.MedicineUltratech ? 8
            : medicineDef == ThingDefOf.MedicineIndustrial ? 2 : 1;

        //下达搬运任务前核对目标与医疗舱的路径和预约。
        public string TransportBlockReason(Pawn pawn, Thing target)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map != Map || pawn.Downed || pawn.Dead) return "请选择同地图、能够行动的殖民者";
            if (!target.Spawned || target.Map != Map) return "目标已经离开地图";
            if (target.def.IsMedicine && 200 - medicine < MedicineValue(target.def)) return "剩余药物容量不足以装入一份该药物";
            if (!target.def.IsMedicine && !Accepts(target)) return HasAnyContents ? "舱内已有患者" : "请先装填药物；仅接受人形患者及其尸体";
            if (target.IsForbidden(pawn)) return "目标已被禁止使用";
            if (!pawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Deadly)) return "医疗舱无法到达或已被其他人预约";
            if (!pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, Danger.Deadly)) return "目标无法到达或已被其他人预约";
            return null;
        }

        //实际搬运药物或患者，而不是在界面中直接转移物品。
        public void OrderTransport(Pawn pawn, Thing target)
        {
            string reason = TransportBlockReason(pawn, target);
            if (reason != null) { Messages.Message(reason, this, MessageTypeDefOf.RejectInput, false); return; }
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("MihoPhase2_ServiceMedicalPod"), target, this);
            job.count = target.def.IsMedicine ? Mathf.Min(target.stackCount,
                Mathf.FloorToInt((200 - medicine) / MedicineValue(target.def))) : 1;
            if (!pawn.jobs.TryTakeOrderedJob(job)) Messages.Message("搬运任务未能开始，请检查操作者当前状态", this, MessageTypeDefOf.RejectInput, false);
        }

        //自行入舱必须有药物，并由可控制的殖民者预约舱体。
        public string AdmissionBlockReason(Pawn pawn)
        {
            if (!Spawned || Faction != Faction.OfPlayer) return "医疗舱不可操作";
            if (pawn == null || !pawn.IsColonistPlayerControlled || pawn.Map != Map || pawn.Downed || pawn.Dead)
                return "请选择本地图能够行动的殖民者";
            if (pawn.IsQuestLodger()) return "任务访客不能自行入舱";
            if (HasAnyContents) return "舱内已有患者";
            if (medicine <= 0f) return "请先在医疗舱管理中装填药物";
            if (!Accepts(pawn)) return "仅接受人形患者";
            if (!pawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                return "医疗舱无法到达或已被其他人预约";
            return null;
        }

        //右键与管理面板使用相同的检查和原版入舱任务。
        public void OrderAdmission(Pawn pawn)
        {
            string reason = AdmissionBlockReason(pawn);
            if (reason != null) { Messages.Message(reason, this, MessageTypeDefOf.RejectInput, false); return; }
            if (!pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterCryptosleepCasket, this)))
                Messages.Message("入舱任务未能开始，请检查殖民者当前状态", this, MessageTypeDefOf.RejectInput, false);
        }

        //直接安排本人治疗，药物搬运和手术仍在医疗舱管理中选择。
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            if (Faction != Faction.OfPlayer) yield break;
            string reason = AdmissionBlockReason(pawn);
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                reason == null ? "入舱治疗" : "无法入舱治疗：" + reason,
                reason == null ? (System.Action)(() => OrderAdmission(pawn)) : null), pawn, this);
            yield return new FloatMenuOption("管理医疗舱（操作者：" + pawn.LabelShort + "）",
                () => Find.WindowStack.Add(new Dialog_ImperialBuilding(this, pawn)));
        }

        //职责：安全弹出患者并取消手术，医疗流程不会附加冷冻休眠病。
        public override void EjectContents()
        {
            remainingTicks = 0;
            restoringPart = null;
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
        }

        //职责：显示药物容量、当前操作及剩余游戏小时。
        public override string GetInspectString() => base.GetInspectString() + $"\n药物容量：{medicine:F0}/200点\n"
            + (remainingTicks > 0 ? $"{operation}：剩余{remainingTicks / 2500f:F1}小时" : Patient == null ? "空舱：在医疗舱管理中安排入舱" : "普通治疗：不消耗药物点数")
            + (!Powered ? "\n未通电或关闭：治疗与手术暂停" : "");

        //职责：保存药物、手术进度和待修复部件。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref medicine, "medicine");
            Scribe_Values.Look(ref operation, "operation", "治疗");
            Scribe_Values.Look(ref remainingTicks, "remainingTicks");
            Scribe_Values.Look(ref operationTicks, "operationTicks");
            Scribe_BodyParts.Look(ref restoringPart, "restoringPart");
        }
    }
}
