using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //按弹头配置曳光、命中闪光和持续激光的实际表现。
    public sealed class CombatVisuals : DefModExtension
    {
        public Color color = new Color(1f, .58f, .18f);
        public float tracerWidth = .09f;
        public float tracerLength = 1.6f;
        public float impactScale = .45f;
        public bool energy;
        public bool beam;
        public bool physicalProjectile;
        public bool rocketExhaust;
        public bool smoke;
        public Color smokeColor = new Color(.22f, .20f, .18f, .8f);
        public SoundDef impactSound;
        public SoundDef loopSound;
    }
}
