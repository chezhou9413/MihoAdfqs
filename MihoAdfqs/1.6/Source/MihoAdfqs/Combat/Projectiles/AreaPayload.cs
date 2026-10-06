using Verse;

namespace MihoAdfqs.Combat.Projectiles
{
    //职责：配置方形弹头范围、中心伤害以及命中后施加的持续状态。
    public class AreaPayload : DefModExtension
    {
        public int width;
        public int height;
        public bool orientToFlight;
        public int centerWidth;
        public float centerDamage;
        public float wallDamage;
        public HediffDef hediff;
    }
}
