using Verse;

namespace MihoAdfqs.Combat.Armor
{
    //职责：配置护甲抵消值或塔盾的防护参数。
    public class CompProperties_ArmorReserve : CompProperties
    {
        public float capacity;
        public bool towerShield;
        //职责：指定抵消值组件类型。
        public CompProperties_ArmorReserve() { compClass = typeof(CompArmorReserve); }
    }
}
