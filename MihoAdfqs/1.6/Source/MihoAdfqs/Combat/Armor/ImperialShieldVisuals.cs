using MihoAdfqs.Combat.Effects;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Armor
{
    //每件护盾保留最近四次命中，短暂动画不进入存档。
    internal sealed class ImperialShieldVisuals
    {
        private readonly Vector4[] hits = new Vector4[4];
        private readonly int[] hitTicks = { -9999, -9999, -9999, -9999 };
        private int nextHit;
        private int brokenTick = -9999;
        private int formedTick = -9999;
        private int openedTick = -1;

        //从来弹反方向定位罩面，未知来源仍沿DamageInfo携带的入射角处理。
        public void Hit(Pawn pawn, DamageInfo damage, float blocked, bool broken, CompProperties_ImperialShield settings)
        {
            int tick = Find.TickManager.TicksGame;
            Vector3 direction = Vector3Utility.HorizontalVectorFromAngle(damage.Angle + 180);
            hits[nextHit] = new Vector4(direction.x * .82f, direction.z * .82f, 0,
                Mathf.Clamp(.5f + Mathf.Sqrt(blocked) * .045f, .5f, 1.5f));
            hitTicks[nextHit] = tick;
            nextHit = (nextHit + 1) % hits.Length;
            if (!broken) return;
            brokenTick = tick;
            if (pawn.Spawned)
                pawn.Map.GetComponent<MapComponent_CombatVfx>().ShieldBreak(pawn.Drawer.DrawPos,
                    settings.shieldColor, Size(pawn, settings));
        }

        //恢复可用时触发从核心向外扩展的闭合波。
        public void Reform()
        {
            formedTick = Find.TickManager.TicksGame;
        }

        //首次展开与停机重建共用拼片进度，充满后的闭合波不会再次拆开球壳。
        public void Draw(Pawn pawn, CompProperties_ImperialShield settings, float energyFraction,
            bool charging, int resetTicks, bool normallyVisible)
        {
            if (!pawn.Spawned || pawn.Dead || pawn.Map != Find.CurrentMap || pawn.Position.Fogged(pawn.Map)) return;
            int tick = Find.TickManager.TicksGame;
            bool recent = tick - brokenTick < 45 || tick - formedTick < 70
                || tick - hitTicks[(nextHit + hits.Length - 1) % hits.Length] < 66
                || openedTick >= 0 && tick - openedTick < 66;
            if (!normallyVisible && !recent && !charging && resetTicks <= 0) return;
            if (openedTick < 0) openedTick = resetTicks > 0 ? tick - 66 : tick;
            for (int i = 0; i < hits.Length; i++) hits[i].z = (tick - hitTicks[i]) / 60f;
            bool resetting = resetTicks > 0;
            float deployment = resetting ? Mathf.Clamp01(1 - resetTicks / 90f)
                : Mathf.Clamp01((tick - openedTick) / 66f);
            Vector4 state = new Vector4(energyFraction, charging || resetting ? 1 : 0, resetting ? 1 : 0, deployment);
            Vector4 events = new Vector4((tick-brokenTick)/60f, (tick-formedTick)/60f,
                pawn.thingIDNumber % 79, settings.doubleLayer ? 1 : 0);
            ShieldVfxMaterials.Shell(pawn.Drawer.DrawPos, Size(pawn, settings), settings.shieldColor, state, events, hits);
        }

        //按体型缓慢放大，罩住人物但避免大型人物撑出整片屏幕。
        private static float Size(Pawn pawn, CompProperties_ImperialShield settings)
        {
            return settings.visualScale * Mathf.Clamp(Mathf.Pow(pawn.BodySize, .25f), .65f, 1.6f);
        }
    }
}
