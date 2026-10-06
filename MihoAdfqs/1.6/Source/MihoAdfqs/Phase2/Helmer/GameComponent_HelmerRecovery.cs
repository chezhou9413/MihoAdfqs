using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：在死亡和伤害调用栈退出后恢复45号人物，避免同次伤害继续伤及刚复活的身体。
    public class GameComponent_HelmerRecovery : GameComponent
    {
        private List<HelmerRecovery> pending = new List<HelmerRecovery>();
        public static GameComponent_HelmerRecovery Current => Verse.Current.Game.GetComponent<GameComponent_HelmerRecovery>();

        //职责：提供原版全局组件构造入口。
        public GameComponent_HelmerRecovery(Game game) { }

        //职责：登记唯一一次死亡恢复过程，供死亡期间的对象保留规则查询。
        public void Register(HelmerRecovery recovery) => pending.Add(recovery);

        //职责：查询人物是否处于死亡与恢复之间。
        public bool Contains(Pawn pawn) => pending.Any(item => item.pawn == pawn);

        //职责：处理已经完成死亡结算的记录，并在非死亡调用或恢复成功后释放记录。
        public override void GameComponentTick()
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                HelmerRecovery item = pending[i];
                if (!item.ready || Find.TickManager.TicksGame < item.dueTick) continue;
                if (item.pawn.Dead) Recover(item);
                pending.RemoveAt(i);
            }
        }

        //职责：恢复同一个人物并归还地图、远行队或世界身份及原先持有的武器。
        private static void Recover(HelmerRecovery item)
        {
            Pawn pawn = item.pawn;
            if (!ResurrectionUtility.TryResurrect(pawn, new ResurrectionParams { noLord = true, removeDiedThoughts = true }))
                throw new System.InvalidOperationException("赫尔默复活失败：" + pawn);
            if (!pawn.Spawned && item.map != null && Find.Maps.Contains(item.map))
            {
                if (pawn.IsWorldPawn()) Find.WorldPawns.RemovePawn(pawn);
                pawn.holdingOwner?.Remove(pawn);
                GenSpawn.Spawn(pawn, item.cell, item.map);
            }
            if (pawn.Spawned)
            {
                if (pawn.drafter != null) pawn.drafter.Drafted = item.drafted;
                if (item.lord != null && pawn.Map.lordManager.lords.Contains(item.lord)) item.lord.AddPawn(pawn);
            }
            else if (item.caravan?.Spawned == true) item.caravan.AddPawn(pawn, true);
            else if (item.tile.Valid && pawn.Faction == Faction.OfPlayer) CaravanMaker.MakeCaravan(new[] { pawn }, pawn.Faction, item.tile, true);
            else if (!pawn.IsWorldPawn()) Find.WorldPawns.PassToWorld(pawn);
            if (item.weapon != null && !item.weapon.Destroyed && pawn.equipment.Primary == null)
            {
                if (item.weapon.Spawned) item.weapon.DeSpawn();
                item.weapon.holdingOwner?.Remove(item.weapon);
                pawn.equipment.AddEquipment(item.weapon);
            }
        }

        //职责：持久化尚未完成的恢复记录。
        public override void ExposeData() => Scribe_Collections.Look(ref pending, "helmerRecoveries", LookMode.Deep);
    }
}
