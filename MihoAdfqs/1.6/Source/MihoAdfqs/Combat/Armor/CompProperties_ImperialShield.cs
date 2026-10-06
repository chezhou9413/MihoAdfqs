using RimWorld;
using UnityEngine;

namespace MihoAdfqs.Combat.Armor
{
    //为两款帝国护盾配置颜色、罩体尺寸和内层轮廓。
    public class CompProperties_ImperialShield : CompProperties_Shield
    {
        public Color shieldColor = new Color(1f, .63f, .16f);
        public float visualScale = 1.85f;
        public bool doubleLayer;

        //沿用原版护盾的容量、损耗与重启参数。
        public CompProperties_ImperialShield()
        {
            compClass = typeof(CompImperialShield);
        }
    }
}
