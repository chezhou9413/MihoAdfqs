using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //从交付队列绘制范围和到达时间，不复制另一套计时状态。
    public sealed class MapComponent_ImperialSupportMarkers : MapComponent
    {
        private static readonly AccessTools.FieldRef<TickManager, float> TickRemainder =
            AccessTools.FieldRefAccess<TickManager, float>("realTimeToTickThrough");
        private readonly HashSet<IntVec3> beaconCells = new HashSet<IntVec3>();
        private float tickOffset;
        private int sampledTick = -1;

        //地图标记使用现有网络组件中的指令。
        public MapComponent_ImperialSupportMarkers(Map map) : base(map) { }

        //使用原版刻间余量插值；暂停时保留当前画面，不推进实际交付计时。
        private double VisualTick
        {
            get
            {
                TickManager manager = Find.TickManager;
                int tick = manager.TicksGame;
                if (!manager.Paused)
                    tickOffset = Mathf.Clamp(TickRemainder(manager) * 60f * manager.TickRateMultiplier, -1f, 0f);
                else if (sampledTick != tick)
                    tickOffset = 0f;
                sampledTick = tick;
                return tick + (double)tickOffset;
            }
        }

        //单发打击显示伤害范围，火力网显示落点散布区与实际炮弹落点。
        public override void MapComponentDraw()
        {
            if (Find.CurrentMap != map) return;
            double visualTick = VisualTick;
            beaconCells.Clear();
            foreach (OrbitalDelivery delivery in GameComponent_OrbitalNetwork.Current.Deliveries)
            {
                if (delivery.map != map) continue;
                //同一格的多个支援共用光柱，避免叠加后过亮。
                if (beaconCells.Add(delivery.cell))
                    ImperialSupportMarkerVisuals.Beacon(delivery.cell,
                        Mathf.Max(0f, (float)((visualTick - delivery.orderedTick) / 60d)));
                ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                Color color = option.IsStrike ? OrbitalStrike.TrailColor(delivery.kind) : new Color(.35f,.75f,.85f);
                if (delivery.kind == "Bombard")
                {
                    GenDraw.DrawFieldEdges(OrbitalSupport.StrikeArea(map, delivery.cell).Cells.ToList(), color);
                    GenDraw.DrawRadiusRing(delivery.cell,.75f,color);
                }
                else
                {
                    GenDraw.DrawRadiusRing(delivery.cell, option.IsStrike ? option.areaRadius : 5f, color);
                    foreach (RimWorld.Skyfaller flight in delivery.flights)
                        if (flight?.Spawned == true) GenDraw.DrawRadiusRing(flight.Position, .8f, color);
                }
            }
        }

        //图标下方显示圆角计时条，圆环与秒数同步更新。
        public override void MapComponentOnGUI()
        {
            if (Find.CurrentMap != map) return;
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            try
            {
                Text.Font = GameFont.Small; Text.Anchor = TextAnchor.MiddleCenter; Text.WordWrap = false;
                GUI.color = Color.white;
                float tickLag = (float)(Find.TickManager.TicksGame - VisualTick);
                float timerHeight = Mathf.Max(34f, Text.LineHeight + 10f);
                float rowHeight = 76f + 4f + timerHeight + 8f;
                var rows = new Dictionary<IntVec3, int>();
                foreach (OrbitalDelivery delivery in GameComponent_OrbitalNetwork.Current.Deliveries)
                {
                    if (delivery.map != map) continue;
                    int row = rows.TryGetValue(delivery.cell, out int index) ? index : 0;
                    rows[delivery.cell] = row + 1;
                    ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                    Vector2 position = GenMapUI.LabelDrawPosFor(delivery.cell);
                    Rect rect = new Rect(position.x - 38f, position.y + 12f + row * rowHeight, 76f, 76f);
                    //透明图标保持原色，浅色轮廓在土地和浅色地面上都能辨识。
                    GUI.color = Color.white;
                    Widgets.DrawTextureFitted(rect.ContractedBy(4),option.markerIcon,1);
                    int remaining = delivery.BarrageActive ? delivery.BarrageTicksLeft : delivery.ArrivalTicks;
                    int total = delivery.BarrageActive ? option.durationTicks
                        : delivery.dueTick-delivery.orderedTick+(option.IsStrike ? 0 : 120);
                    float visualRemaining = remaining > 0 ? remaining + tickLag : 0f;
                    string seconds = (visualRemaining / 60f).ToString("0.0") + "秒";
                    float timerWidth = Mathf.Max(96f, Text.CalcSize(seconds).x + 44f);
                    Rect timer = new Rect(position.x - timerWidth * .5f, rect.yMax + 4f, timerWidth, timerHeight);
                    ImperialSupportMarkerVisuals.Countdown(timer, seconds,
                        Mathf.Clamp01(visualRemaining / Mathf.Max(1, total)), delivery.BarrageActive, option.IsStrike);
                    Rect tooltip = new Rect(timer.x, rect.y, timer.width, timer.yMax - rect.y);
                    TooltipHandler.TipRegion(tooltip,option.label+(delivery.npcCall ? "\n呼叫方："+delivery.faction.Name : "")
                        +"\n"+(delivery.BarrageActive ? "炮击剩余：" : "预计到达：")
                        +Countdown(remaining)+(option.IsStrike ? "\n友军也会受到影响。" : ""));
                }
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
        }

        //倒计时按正常速度下的秒显示，随游戏速度推进并在暂停时停止。
        internal static string Countdown(int ticks)
        {
            float seconds = ticks / 60f;
            return seconds < 60 ? seconds.ToString("0.0") + "秒" :
                ((int)seconds / 60) + "分" + ((int)seconds % 60).ToString("00") + "秒";
        }
    }
}
