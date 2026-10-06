using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：为背景保存属性、射击计时、心情、精神免疫和指挥效果参数。
    public class BackgroundEffects : DefModExtension
    {
        public List<StatModifier> offsets = new List<StatModifier>();
        public List<StatModifier> factors = new List<StatModifier>();
        public float shotTime = 1f;
        public float aimTime = 1f;
        public int mood;
        public bool mentalImmune;
        public bool catSlaughter;
        public bool homebody;
        public bool commander;
        public bool oldGunshot;
        public bool heartDisease;
        public int colleagueDeathPenalty;
    }
}
