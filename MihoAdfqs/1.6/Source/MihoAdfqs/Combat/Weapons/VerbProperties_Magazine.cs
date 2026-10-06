using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //定义弹匣容量、换弹耗时、弹丸数量和允许的射击模式。
    public class VerbProperties_Magazine : VerbProperties
    {
        public int magazineSize;
        public MagazineWeaponType weaponType = MagazineWeaponType.Bullet;
        public float reloadSeconds;
        public float preciseBurstTicks;
        public SoundDef reloadSound;
        public SoundDef reloadCompleteSound;
        public SimpleCurve accuracyByDistance;
        public int pellets = 1;
        public bool secondary;
        public bool guaranteedAccuracy;
        public List<string> fireModes = new List<string> { "点射" };
        public List<ThingDef> ammunition = new List<ThingDef>();

        //普通枪与下挂均可手动点选地格，人物和建筑目标沿用原版设置。
        public VerbProperties_Magazine()
        {
            targetParams.canTargetLocations = true;
        }
    }
}
