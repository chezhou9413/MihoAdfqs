using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using MihoAdfqs.Phase2.UI;
using Verse.Sound;

namespace MihoAdfqs.Combat.Weapons
{
    //职责：按武器实例管理弹匣、射击模式、下挂选择和换弹计时。
    public class Verb_Magazine : Verb_Shoot
    {
        private int rounds = -1;
        private int reloadUntil;
        private int reloadDuration;
        private int mode;
        private int ammunitionIndex;
        private bool selected = true;
        private bool initialized;
        private float intervalRemainder;
        private int nextShotTicks = 1;
        private readonly List<MagazineShotVisual> shotVisuals = new List<MagazineShotVisual>();
        public IReadOnlyList<MagazineShotVisual> ShotVisuals => shotVisuals;
        public int NextShotTicks => nextShotTicks;
        private VerbProperties_Magazine Settings => (VerbProperties_Magazine)verbProps;
        public bool Selected { get { Initialize(); return selected; } }

        //职责：半自动每次一发，多模式点射三发，全自动连续发射余弹；单发武器每次一发。
        protected override int ShotsPerBurst => Math.Min(Rounds, Settings.fireModes[mode] == "全自动"
            ? Settings.magazineSize : Settings.fireModes[mode] == "点射" && Settings.fireModes.Count > 1 ? 3 : 1);
        public override ThingDef Projectile => Settings.ammunition.Count == 0 ? base.Projectile : Settings.ammunition[ammunitionIndex];
        private int Rounds { get { Initialize(); CompleteReload(); return rounds; } }

        //向子弹图标和装弹界面提供实时余弹与容量。
        public int AmmoCount => Rounds;
        public int AmmoCapacity => Settings.magazineSize;
        public MagazineWeaponType WeaponType => Settings.weaponType;

        //职责：刷新换弹状态并提供剩余时长和已完成比例。
        public int ReloadTicksLeft { get { CompleteReload(); return Math.Max(0, reloadUntil - Find.TickManager.TicksGame); } }
        public float ReloadProgress => reloadDuration > 0 ? Mathf.Clamp01(1f - (float)ReloadTicksLeft / reloadDuration) : 0f;

        //绘制只读取换弹时间，结算和完成音效由动词Tick负责。
        internal bool TryGetReloadDisplay(int tick, out int remaining, out float progress)
        {
            remaining = Math.Max(0, reloadUntil - tick);
            progress = remaining > 0 && reloadDuration > 0 ? Mathf.Clamp01(1f - (float)remaining / reloadDuration) : 0f;
            return remaining > 0;
        }

        //只有玩家实际控制且仍持有这把武器的单位可以接受手动操作。
        public bool PlayerCanControl => CasterIsPawn && CasterPawn.IsPlayerControlled
            && !CasterPawn.Downed && !CasterPawn.Dead && EquipmentSource != null
            && CasterPawn.equipment?.Primary == EquipmentSource;

        //手动换弹同时检查控制权、射击状态和弹匣余量，自动换弹直接走内部流程。
        public bool CanReload => PlayerCanControl && !Bursting && !WarmingUp
            && ReloadTicksLeft == 0 && Rounds < Settings.magazineSize;

        //点击弹药条后重新检查控制权和状态，避免旧界面继续操作已失去控制的单位。
        public void RequestReload()
        {
            if (CanReload) BeginReload();
        }

        //职责：为首次生成的武器装满弹匣，并设置主副武器的默认选择。
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            selected = !Settings.secondary;
            rounds = Settings.magazineSize;
        }

        //在换弹结束时补满弹匣，并立即播放完成声。
        internal void CompleteReload()
        {
            if (reloadUntil <= 0 || Find.TickManager.TicksGame < reloadUntil) return;
            rounds = Settings.magazineSize;
            reloadUntil = 0;
            PlayReloadSound(Settings.reloadCompleteSound);
        }

        //禁止未选中的枪管、空弹匣及换弹期间实际发射。
        public override bool Available()
        {
            Initialize();
            if (!selected) return false;
            CompleteReload();
            if (rounds == 0 && reloadUntil == 0) BeginReload();
            return reloadUntil == 0 && rounds > 0 && base.Available();
        }

        //换弹期间保留攻击指令，但不重复进入瞄准和冷却姿态。
        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg,
            bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false,
            bool nonInterruptingSelfCast = false)
        {
            return Available() && base.TryStartCastOn(castTarg, destTarg, surpriseAttack,
                canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }

        //新任务中断整匣连射，并只清除当前枪管持有的射击姿态与光束。
        public void StopFiring()
        {
            bool active = Bursting || WarmingUp;
            Stance_Busy stance = CasterPawn?.stances.curStance as Stance_Busy;
            if (!active && stance?.verb != this) return;
            Reset();
            if (stance?.verb == this)
            {
                if (stance is Stance_Warmup warmup) warmup.Interrupt();
                else CasterPawn.stances.CancelBusyStanceHard();
            }
            if (caster.Spawned)
                caster.Map.GetComponent<MihoAdfqs.Combat.Effects.MapComponent_CombatVfx>().Stop(this);
        }

        //只有成功发射后扣弹并记录消耗动效，霰弹多弹丸共用一次扣弹。
        protected override bool TryCastShot()
        {
            bool fired = false;
            for (int i = 0; i < Settings.pellets; i++)
            {
                if (!Settings.guaranteedAccuracy && Phase2.Helmer.Gene_Helmer.Get(CasterPawn)?.storm != false) { fired |= base.TryCastShot(); continue; }
                if (!TryFindShootLineFromTo(caster.Position, currentTarget, out ShootLine line)) continue;
                LaunchAccurateShot(line);
                fired = true;
            }
            if (!fired) return false;
            UpdateCadence();
            var visual = Projectile.GetModExtension<MihoAdfqs.Combat.Effects.CombatVisuals>();
            if (visual != null)
                caster.Map.GetComponent<MihoAdfqs.Combat.Effects.MapComponent_CombatVfx>().Shot(this, currentTarget, visual);
            rounds--;
            float now = Time.realtimeSinceStartup;
            shotVisuals.RemoveAll(shot => now - shot.startedAt >= MagazineShotVisual.Duration);
            shotVisuals.Add(new MagazineShotVisual(rounds, now));
            if (rounds == 0) BeginReload();
            return true;
        }

        //必中特殊射击同样应用独特武器的伤害类型与附加伤害，并记录后坐力计时。
        private void LaunchAccurateShot(ShootLine line)
        {
            EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
            EquipmentSource.GetComp<CompApparelVerbOwner_Charged>()?.UsedOnce();
            lastShotTick = Find.TickManager.TicksGame;
            var projectile = (Projectile)GenSpawn.Spawn(Projectile, line.Source, caster.Map);
            CompUniqueWeapon unique = EquipmentSource.TryGetComp<CompUniqueWeapon>();
            if (unique != null)
                foreach (WeaponTraitDef trait in unique.TraitsListForReading)
                {
                    if (trait.damageDefOverride != null) projectile.damageDefOverride = trait.damageDefOverride;
                    if (trait.extraDamages.NullOrEmpty()) continue;
                    if (projectile.extraDamages == null) projectile.extraDamages = new List<ExtraDamage>();
                    projectile.extraDamages.AddRange(trait.extraDamages);
                }
            projectile.Launch(caster, caster.DrawPos, currentTarget, currentTarget,
                ProjectileHitFlags.IntendedTarget, preventFriendlyFire, EquipmentSource);
        }

        //职责：交替使用相邻整数游戏刻，保留射速小数余量，使长期平均射速符合设定。
        private void UpdateCadence()
        {
            float ticks = Settings.preciseBurstTicks > 0 ? Settings.preciseBurstTicks : Settings.ticksBetweenBurstShots;
            CompUniqueWeapon unique = EquipmentSource?.TryGetComp<CompUniqueWeapon>();
            if (unique != null)
                foreach (WeaponTraitDef trait in unique.TraitsListForReading) ticks /= trait.burstShotSpeedMultiplier;
            ticks *= Phase2.Helmer.Patch_HelmerShootingTime.Factor(this) * Phase2.Backgrounds.BackgroundUtility.ShotTime(CasterPawn)
                * Phase2.Medicine.InjectorUtility.ShotTime(CasterPawn) * WeaponTiming.ShotFactor(CasterPawn);
            float accumulated = Mathf.Max(1f, ticks) + intervalRemainder;
            nextShotTicks = Mathf.FloorToInt(accumulated);
            intervalRemainder = accumulated - nextShotTicks;
        }

        //职责：按装备和战斗状态修正换弹耗时，不消耗文档未要求的外置弹药物品。
        private void BeginReload()
        {
            if (reloadUntil != 0) return;
            float factor = WeaponTiming.ReloadFactor(CasterPawn);
            reloadDuration = Math.Max(1, Mathf.RoundToInt(Settings.reloadSeconds * factor * 60f));
            reloadUntil = Find.TickManager.TicksGame + reloadDuration;
            PlayReloadSound(Settings.reloadSound);
        }

        //只在射手实际位于地图上时播放装填阶段音效。
        private void PlayReloadSound(SoundDef sound)
        {
            if (sound != null && caster?.Spawned == true)
                sound.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
        }

        //所有单位显示弹药状态，手动射击模式和弹种按钮只提供给玩家控制的单位。
        public IEnumerable<Gizmo> GetCommands()
        {
            Initialize();
            yield return new UI.Gizmo_MagazineProgress(this);
            if (!PlayerCanControl) yield break;
            if (Settings.fireModes.Count > 1)
            {
                var command = new Command_ImperialAction
                {
                    defaultLabel = Settings.fireModes[mode], defaultDesc = "半自动一发，点射三发，全自动连续发射余弹；只有点射模式的单发武器每次一发。",
                    icon = EquipmentSource.def.uiIcon,
                    action = () =>
                    {
                        if (!PlayerCanControl || Bursting || WarmingUp) return;
                        mode = (mode + 1) % Settings.fireModes.Count;
                    }
                };
                if (Bursting || WarmingUp) command.Disable("射击结束后切换");
                yield return command;
            }
            if (Settings.ammunition.Count > 1)
            {
                var command = new Command_ImperialAction
                {
                    defaultLabel = Projectile.LabelCap, defaultDesc = Projectile.description + "\n切换弹种会执行一次换弹。",
                    icon = EquipmentSource.def.uiIcon,
                    action = () =>
                    {
                        if (!PlayerCanControl || Bursting || WarmingUp || ReloadTicksLeft > 0) return;
                        ammunitionIndex = (ammunitionIndex + 1) % Settings.ammunition.Count;
                        BeginReload();
                    }
                };
                if (Bursting || WarmingUp || reloadUntil > 0) command.Disable("当前无法切换弹种");
                yield return command;
            }
        }

        //检查玩家控制权后切换主枪和下挂，保留各自弹匣与换弹进度。
        public static void Select(Verb_Magazine target)
        {
            if (!target.PlayerCanControl) return;
            if (target.EquipmentCompSource.AllVerbs.Any(v => v is Verb_Magazine && (v.Bursting || v.WarmingUp))) return;
            if (Find.Targeter.targetingSource?.GetVerb?.EquipmentSource == target.EquipmentSource)
                Find.Targeter.StopTargeting();
            foreach (Verb_Magazine verb in target.EquipmentCompSource.AllVerbs.OfType<Verb_Magazine>())
            {
                verb.Initialize();
                verb.selected = verb == target;
            }
        }

        //职责：保存各弹匣独立的余弹、换弹终点和当前模式。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rounds, "rounds", -1);
            Scribe_Values.Look(ref reloadUntil, "reloadUntil");
            Scribe_Values.Look(ref reloadDuration, "reloadDuration");
            Scribe_Values.Look(ref mode, "fireMode");
            Scribe_Values.Look(ref ammunitionIndex, "ammunitionIndex");
            Scribe_Values.Look(ref selected, "selected", true);
            Scribe_Values.Look(ref initialized, "initialized");
            Scribe_Values.Look(ref intervalRemainder, "intervalRemainder");
            Scribe_Values.Look(ref nextShotTicks, "nextShotTicks", 1);
        }
    }
}
