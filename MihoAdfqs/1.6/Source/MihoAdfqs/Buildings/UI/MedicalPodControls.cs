using System;
using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Buildings.Medical;
using MihoAdfqs.Phase2.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Buildings.UI
{
    //把入舱、药物搬运和医疗手术集中在同一面板。
    internal static class MedicalPodControls
    {
        //按患者状态显示费用、进度与明确的不可用原因。
        public static void Draw(BuildingControlListing listing, Building_MedicalPod pod, Dialog_ImperialBuilding window)
        {
            listing.Label("舱内：" + (pod.Patient == null ? "空舱" : pod.Patient.LabelShort + (pod.Patient.Dead ? "（尸体）" : "（患者）")));
            listing.Progress(pod.medicine / 200f, $"药物储量：{pod.medicine:F0}/200点");
            if (pod.RemainingTicks > 0) listing.Progress(pod.OperationProgress,
                $"{pod.Operation} · 剩余{pod.RemainingTicks / 2500f:F1}小时" + (pod.Powered ? "" : " · 已暂停"));
            listing.Label("先装填药物，再选择入舱人员。能行动的殖民者自行入舱，倒地患者或尸体由操作者搬运。", true);
            window.DrawOperator(listing);
            string transportReason = pod.HasAnyContents ? "舱内已有患者" : pod.medicine <= 0 ? "先装填至少1点药物" : window.Operator == null ? "请选择操作者" : null;
            if (listing.Button("选择倒地患者或尸体 · 安排搬运入舱", transportReason)) ChoosePatient(pod, window.Operator);
            if (listing.Button("让选中的殖民者入舱治疗", pod.AdmissionBlockReason(window.Operator)))
                pod.OrderAdmission(window.Operator);
            listing.Label("装填药物：草药每份1点、医药每份2点、闪耀医药每份8点。系统只安排剩余容量能容纳的数量，需要操作者实际搬运。", true);
            foreach (ThingDef medicine in new[] { ThingDefOf.MedicineHerbal, ThingDefOf.MedicineIndustrial, ThingDefOf.MedicineUltratech })
            {
                ThingDef selected = medicine;
                int value = Building_MedicalPod.MedicineValue(medicine);
                string reason = window.Operator == null ? "请选择操作者" : 200 - pod.medicine < value ? $"剩余容量不足{value}点" : null;
                if (listing.Button($"安排装填{medicine.LabelCap} · 每份{value}点", reason)) ChooseMedicine(pod, window.Operator, selected);
            }
            listing.Label("普通治疗：自动包扎伤口，每秒为每个受伤部件恢复1点伤势，不会自动再生缺失部件。通电后，只要储量大于0或付费手术正在进行就会治疗，普通治疗不另外扣除药物点数。", true);
            if (pod.Patient != null && !pod.Patient.Dead)
            {
                var missing = pod.Patient.health.hediffSet.GetMissingPartsCommonAncestors().ToList();
                listing.Label(missing.Count == 0 ? "肢体重建：没有需要重建的缺失部件" : "肢体重建：每次选择一个部件，消耗16点，耗时6小时");
                foreach (Hediff_MissingPart part in missing)
                {
                    BodyPartRecord selected = part.Part;
                    if (listing.Button("重建" + selected.Label + " · 16点 / 6小时", pod.OperationBlockReason("修复肢体", 16, selected)))
                        pod.StartOperation("修复肢体", 16, 15000, selected);
                }
            }
            if (listing.Button("复活舱内尸体 · 50点 / 72小时", pod.OperationBlockReason("复活", 50))) pod.StartOperation("复活", 50, 180000);
            DrawConversion(listing, pod, "转换为帝国美狐");
            DrawConversion(listing, pod, "转换为帝国米莉拉");
            if (listing.Button("弹出患者 / 尸体", pod.HasAnyContents ? null : "医疗舱为空"))
            {
                if (pod.RemainingTicks > 0) Find.WindowStack.Add(new Dialog_ImperialConfirmation("中断手术并弹出患者",
                    "当前手术会取消，已经支付的药物点数不返还。是否弹出？", pod.EjectContents, () => { }, "确认弹出", "继续手术"));
                else pod.EjectContents();
            }
            listing.Label("手术开始时一次性支付药物点数。断电暂停倒计时，恢复供电后继续；治疗与手术均不自动安排装药。", true);
        }

        //只在点击时检索候选患者，不在每次界面重绘时遍历全地图。
        private static void ChoosePatient(Building_MedicalPod pod, Pawn operatorPawn)
        {
            IEnumerable<Thing> patients = pod.Map.mapPawns.AllPawnsSpawned.Where(p => p.Downed && p.RaceProps.Humanlike).Cast<Thing>()
                .Concat(pod.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Where(t => t is Corpse c && c.InnerPawn.RaceProps.Humanlike));
            var options = new List<FloatMenuOption>();
            foreach (Thing candidate in patients)
            {
                Thing selected = candidate;
                string reason = pod.TransportBlockReason(operatorPawn, candidate);
                options.Add(new FloatMenuOption(candidate.LabelCap + (reason == null ? " · 搬运入舱" : " · " + reason),
                    reason == null ? (Action)(() => pod.OrderTransport(operatorPawn, selected)) : null));
            }
            if (options.Count == 0) options.Add(new FloatMenuOption("本地图没有倒地的人形患者或人形尸体", null));
            Find.WindowStack.Add(new FloatMenu(options));
        }

        //显示地图上的药物堆栈及本次实际能搬入的数量。
        private static void ChooseMedicine(Building_MedicalPod pod, Pawn operatorPawn, ThingDef medicine)
        {
            var options = new List<FloatMenuOption>();
            foreach (Thing stack in pod.Map.listerThings.ThingsOfDef(medicine).OrderBy(t => t.Position.DistanceToSquared(pod.Position)))
            {
                Thing selected = stack;
                string reason = pod.TransportBlockReason(operatorPawn, stack);
                int count = Math.Min(stack.stackCount, (int)((200 - pod.medicine) / Building_MedicalPod.MedicineValue(medicine)));
                options.Add(new FloatMenuOption(stack.LabelCap + $" · 本次{count}份 · 位置({stack.Position.x},{stack.Position.z})" + (reason == null ? "" : " · " + reason),
                    reason == null ? (Action)(() => pod.OrderTransport(operatorPawn, selected)) : null));
            }
            if (options.Count == 0) options.Add(new FloatMenuOption("本地图没有可装填的" + medicine.LabelCap, null));
            Find.WindowStack.Add(new FloatMenu(options));
        }

        //种族转换在明确告知影响后确认，并锁定确认时的患者身份。
        private static void DrawConversion(BuildingControlListing listing, Building_MedicalPod pod, string operation)
        {
            if (!listing.Button(operation + " · 25点 / 通电后立即完成", pod.OperationBlockReason(operation, 25))) return;
            Pawn patient = pod.Patient;
            Find.WindowStack.Add(new Dialog_ImperialConfirmation(operation,
                "将" + patient.LabelShort + "转换种族，保留身份、年龄、技能、关系和记忆，并重建身体与种族组件；不适穿的衣物会转入库存。开始时消耗25点药物。是否继续？",
                () =>
                {
                    if (pod.Patient != patient) Messages.Message("舱内患者已改变，请重新选择医疗操作", pod, MessageTypeDefOf.RejectInput, false);
                    else pod.StartOperation(operation, 25, 1);
                }, () => { }, "确认转换", "返回医疗面板"));
        }
    }
}
