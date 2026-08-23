using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MihoAdfqs.Utility
{
    public static class BrainwashUtility
    {
        /// <summary>
        /// 对目标执行洗脑操作
        /// </summary>
        /// <param name="pawn">目标 Pawn</param>
        public static void BrainwashPawn(Pawn pawn)
        {
            // 基础检查：必须是活人且属于人类
            if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike) return;

            // 1. 处理派系和身份 (针对囚犯)
            // 囚犯：强制招募，无视死忠
            if (pawn.IsPrisoner)
            {
                // 将派系设为玩家（强制招募的核心）
                pawn.SetFaction(Faction.OfPlayer);

                // 确保移除囚犯状态，将其变为普通殖民者
                // 注意：如果想保留奴隶身份，不要执行这一行，但你的需求是"囚犯变成殖民者"
                if (pawn.guest != null)
                {
                    pawn.guest.SetGuestStatus(null);
                }
            }
            // 奴隶：已经是玩家派系，且 IsSlave 为 true，这里不做操作，他们会保持奴隶身份。
            if (pawn.needs?.mood?.thoughts?.memories != null)
            {
                pawn.needs.mood.thoughts.memories.Memories.Clear();
            }

            // 3. 断绝所有社会关系 (Relationships)
            if (pawn.relations != null)
            {
                List<DirectPawnRelation> relations = pawn.relations.DirectRelations.ToList();
                foreach (var rel in relations)
                {
                    // 移除关系 (双向移除)
                    pawn.relations.RemoveDirectRelation(rel.def, rel.otherPawn);
                }
            }

            // 4. 文化同步 (Ideology DLC)
            if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos?.PrimaryIdeo != null)
            {
                pawn.ideo?.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }

            // 5. 添加 "我什么都忘了" 心情 Buff
            ThoughtDef buffDef = MihoDefRef.Thought_BrainwashHappy;
            if (buffDef != null)
            {
                pawn.needs?.mood?.thoughts?.memories.TryGainMemory(buffDef);
            }

            // 可选：刷新一下Pawn的缓存，确保数据更新
            pawn.Notify_DisabledWorkTypesChanged();
        }
    }
}
