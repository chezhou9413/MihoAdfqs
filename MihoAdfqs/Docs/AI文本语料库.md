# MihoAdfqs AI 文本语料库

这份文档汇总模组中玩家可见、可本地化、叙事相关的文本，供 AI 阅读、检索、续写、校对或翻译使用。

## 语料说明

- 生成日期：2026-08-12
- 文本保持原文，不主动纠错或润色；只清理 XML 缩进产生的首尾空白。
- RimWorld 占位符、动态插值和规则表达式均保留，翻译或改写时不要删除。
- 收录 XML 名称、描述、背景、提示、报告文本、命名规则，以及 C# 运行时显示字符串。
- 不收录纯数值配置、贴图路径、程序集标识、代码注释和第三方框架内部名称。
- 当前收录：82 个 XML 记录、251 个 XML 文本字段、33 个代码文本片段，来自 72 个源文件。

## 快速索引

- 模组信息：1 条记录
- 派系与世界：5 条记录
- 角色、背景与血统：10 条记录
- 服装：13 条记录
- 武器与弹药：21 条记录
- 物品与配方：4 条记录
- 能力与状态：3 条记录
- 事件与工作：6 条记录
- 心情与记忆：17 条记录
- 命名词库：2 条记录
- 运行时界面文本：33 条记录

## 模组信息

### 美狐扩展派系-帝国狂想曲-美狐第三帝国

- 来源：`About/About.xml`
- XML 类型：`ModMetaData`
- 检索键：`chezhou.Kind.MihoAdfqs`
- `name`：美狐扩展派系-帝国狂想曲-美狐第三帝国
- `author`：作者:秋霜狐狐亲
- `supportedVersions/li`：1.6
- `description`：

  > （平行世界）美狐民主联盟，人民的生活水深火热，朝不保夕。那些无能的，贪婪的，残暴的，只会肆意掠夺权力的政客，打着一遍又一遍的改善现状的口号，人们不断失望。一场对外史无前例的大败，彻底击碎了联盟的脊梁，无数的子民沦为了玩物，奴隶。
  >   有人反抗着，咆哮着，想要改变这一切，被政客们控制的政府军无情镇压。
  >   恶劣的生存环境，飙升的死亡率，黑暗的未来，甚至是饥饿，一个在外界看来，无比先进的星际文明出现了饥饿。
  >   出逃，反抗，战争。
  >   联盟的人口消失了近一半，在这场长达百年的时间内，直到末期。
  >   政客们新推上的一位傀儡总统，结束了这个黑暗时代。
  >   这位傀儡总统，用着她早已暗中执行的计划，一个长达数十年的伟大计划，打破了牢笼。
  >   一场发自当地知名”餐馆“的暴动，掀开了时代的帷幕。
  >   从前无比顺从的军队倒戈；那些无能的，慌张的政客们，自一个空气中弥漫着血水气息的雨夜，仅剩下了自以为能掌控一切的脑袋，悬挂着总统府的房梁上。
  >   内战又持续了二十年，所有的”前政客“被清除干净。
  >   美狐的人口所剩不多，联盟？不，此时称为帝国的巨树伸直了它的根系，扎根于前联盟的遗骸上，重新生长。
  >   领导者在之后的五十年逐渐实现了她的承诺，她所构筑的世界，只是，尚未完成的，还有那不断生长的野心。
  >   美狐们信任这位”至高“领导者，相信她，会给美狐族，带来”新生“
  > 
  > 
  >   [美狐语]或许，你已经看清了许多，那些摆在眼前的事实。帝国的建立源于压迫，迷茫的美狐们也需要一个确切的领导者，所以，我站在台前，带着他们踏出黑暗。
  >   你可以说我极端，暴政，种族屠杀者，战争贩子。但，无可否认的事实是，我让一盘散沙的美狐们凝聚在了一起。
  >   我不关心其他种族过得怎么样，也不在他们的生命，我只在乎我的同族，其他人，只需要”安静的“变成帝国成长的肥料。
  > 
  >   在某位疯狂的独裁者的领导下，一大群狐狸来到了边缘世界——她们带来了战争，庞大的舰队，播撒战争，混乱，妄图拓张自己的地盘。届时，会有两个选择。这位独裁者会亲自前来你的殖民地，接受与否，接下来能预知的道路，会截然不同。
  > 
  >   内容：
  >   大致足够（存疑）的武器
  > 一个对所有美狐族表现的友好的派系（敌对所有非美狐派系，对玩家殖民地取决于你的选择）
  > 专为美狐族设计的服装（后续会添加，当前很少）
  > 若干小道具（部分会有些地狱笑话）
  > 一些有趣的机制（后续）
  > 帝国专属的科技树（暂时没加）
  > CE兼容（还没做）
  > 初版（内容不多，后续会往死里加）
  > 特殊人物（目前就一个，预期拓展到20+）
  > 升变机制（普通美狐跃迁至九尾，未实装）
  > 特殊灵能（未实装，击落鸽子！）
  > 
  > 
  > 常见问题
  > 刚发，暂未发现，后续会不断补充
  > 
  > 支持语言
  > 中文
  > 
  > 一定要加的
  > 动态表情【灵魂，许多专属人物都有自己的独特动态表情，当然，目前只有1个，甚至还没完成，大雾，完成了会去掉这句话（）】
  > 
  > 贡献者名单（名单顺序按照字母表）
  > 程序：追踪虫
  > 贴图：冰皮汤圆
  > 
  > 作者的话：
  > 咕咕嘎嘎，咕咕嘎嘎！
  > 封面也没做完！
- `modDependencies/li[1]/displayName`：Harmony
- `modDependencies/li[2]/displayName`：Humanoid Alien Races 2.0
- `modDependencies/li[3]/displayName`：ChezhouLib
- `modDependencies/li[4]/displayName`：miho

## 派系与世界

### 美狐第三帝国

- 来源：`1.6/Defs/FactionDefs/Faction_MihoThirdEmpire.xml`
- XML 类型：`FactionDef`
- 检索键：`MihoThirdEmpire`
- `label`：美狐第三帝国
- `fixedName`：美狐第三帝国
- `description`：一个组织严密、军事化程度极高的势力，会在星球上建立据点并与玩家保持友好关系。
- `leaderTitle`：元首

### 美狐第三帝国贸易队（基地）

- 来源：`1.6/Defs/TraderKindDefs/TraderKinds_MihoThirdEmpire.xml`
- XML 类型：`TraderKindDef`
- 检索键：`Base_MihoThirdEmpire_Weapons`
- `label`：美狐第三帝国贸易队（基地）

### 肥皂征募队

- 来源：`1.6/Defs/TraderKindDefs/TraderKinds_MihoThirdEmpire.xml`
- XML 类型：`TraderKindDef`
- 检索键：`Caravan_MihoThirdEmpire_SoapRecruit`
- `label`：肥皂征募队

### 美狐第三帝国贸易队

- 来源：`1.6/Defs/TraderKindDefs/TraderKinds_MihoThirdEmpire.xml`
- XML 类型：`TraderKindDef`
- 检索键：`Caravan_MihoThirdEmpire_Weapons`
- `label`：美狐第三帝国贸易队

### 美狐第三帝国据点

- 来源：`1.6/Defs/WorldObjectDefs/WorldObject_MihoThirdEmpire_Settlement.xml`
- XML 类型：`WorldObjectDef`
- 检索键：`MihoThirdEmpire_Settlement`
- `label`：美狐第三帝国据点

## 角色、背景与血统

### 美狐第三帝国元首

- 来源：`1.6/Defs/BackstoryDefs/Backstories_MihoAdfqs.xml`
- XML 类型：`BackstoryDef`
- 检索键：`MihoAdfqs_Fuhrer`
- `title`：美狐第三帝国元首
- `titleShort`：元首
- `baseDesc`：[PAWN_nameDef]是美狐第三帝国的最高领导人，拥有至高无上的权力和威望。[PAWN_pronoun]领导着整个帝国，制定战略方针，指挥千军万马。

### 美狐第三帝国党卫军

- 来源：`1.6/Defs/BackstoryDefs/Backstories_MihoAdfqs.xml`
- XML 类型：`BackstoryDef`
- 检索键：`MihoAdfqs_SS`
- `title`：美狐第三帝国党卫军
- `titleShort`：党卫军
- `baseDesc`：[PAWN_nameDef]是美狐第三帝国党卫军的一员，这是帝国最精锐的武装力量。[PAWN_pronoun]接受了严格的军事训练，忠诚于帝国和元首。

### 美狐第三帝国青年团

- 来源：`1.6/Defs/BackstoryDefs/Backstories_MihoAdfqs.xml`
- XML 类型：`BackstoryDef`
- 检索键：`MihoAdfqs_YouthMember_Fuhrer`
- `title`：美狐第三帝国青年团
- `titleShort`：青年团员
- `baseDesc`：[PAWN_nameDef]在美狐第三帝国青年团中成长，接受了严格的组织训练和思想教育。[PAWN_pronoun]在集体生活中学会了纪律和服从，为日后的发展打下了基础。

### 美狐第三帝国青年团

- 来源：`1.6/Defs/BackstoryDefs/Backstories_MihoAdfqs.xml`
- XML 类型：`BackstoryDef`
- 检索键：`MihoAdfqs_YouthMember_SS`
- `title`：美狐第三帝国青年团
- `titleShort`：青年团员
- `baseDesc`：[PAWN_nameDef]在美狐第三帝国青年团中成长，接受了严格的组织训练和思想教育。[PAWN_pronoun]在集体生活中学会了纪律和服从，为日后的发展打下了基础。

### 元首血统

- 来源：`1.6/Defs/GeneDefs/Gene_Miho.xml`
- XML 类型：`GeneDef`
- 检索键：`Gene_MihoThirdEmpireFuhrer`
- `label`：元首血统
- `description`：拥有元首血统的美狐

### 美狐第三帝国血统

- 来源：`1.6/Defs/GeneDefs/Gene_Miho.xml`
- XML 类型：`XenotypeDef`
- 检索键：`Xeno_MihoThirdEmpire`
- `label`：美狐第三帝国血统
- `description`：拥有美狐第三帝国血脉的美狐

### 阿道夫-秋霜

- 来源：`1.6/Defs/PawnKindDefs/MihuAdfqs.xml`
- XML 类型：`PawnKindDef`
- 检索键：`Miho_adfqs`
- `label`：阿道夫-秋霜
- `modExtensions/li[1]/kindName`：阿道夫-秋霜

### 美狐第三帝国特战部队

- 来源：`1.6/Defs/PawnKindDefs/MihuSpecialForces.xml`
- XML 类型：`PawnKindDef`
- 检索键：`Miho_SpecialForces`
- `label`：美狐第三帝国特战部队

### 美狐第三帝国党卫军

- 来源：`1.6/Defs/PawnKindDefs/MihuSS.xml`
- XML 类型：`PawnKindDef`
- 检索键：`Miho_SS`
- `label`：美狐第三帝国党卫军

### 美狐第三帝国商队护卫队

- 来源：`1.6/Defs/PawnKindDefs/MihuSS.xml`
- XML 类型：`PawnKindDef`
- 检索键：`Miho_SSTrader`
- `label`：美狐第三帝国商队护卫队

## 服装

### 蓝白内衣

- 来源：`1.6/Defs/Apparel/Apparel_BlueLingerie.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_BlueLingerie`
- `label`：蓝白内衣
- `description`：一套蓝白配色的内衣，穿戴者更具魅力，社交互动更有优势。

### 蓝色泳衣

- 来源：`1.6/Defs/Apparel/Apparel_BlueSwimsuit.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_BlueSwimsuit`
- `label`：蓝色泳衣
- `description`：一件清爽的蓝色泳衣，穿戴者更具魅力，社交更占优势。

### 圆顶帽子

- 来源：`1.6/Defs/Apparel/Apparel_BowlerHat.xml`
- XML 类型：`ThingDef`
- 检索键：`BowlerHat`
- `label`：圆顶帽子
- `description`：一顶经典的圆顶帽子。佩戴后略显更有魅力。

### 元首服

- 来源：`1.6/Defs/Apparel/Apparel_FuhrerCoat.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_FuhrerCoat`
- `label`：元首服
- `description`：威严而富有气场的服装，穿戴者更具魅力与领导风范。

### 元首帽

- 来源：`1.6/Defs/Apparel/Apparel_FuhrerHelmet.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_FuhrerHelmet`
- `label`：元首帽
- `description`：象征权威与气场的帽子。佩戴后显得更具魅力。

### 哥特连衣裙

- 来源：`1.6/Defs/Apparel/Apparel_Gothic.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_Gothic`
- `label`：哥特连衣裙
- `description`：一件风格独特的哥特连衣裙，穿戴者更显魅力。

### 粉色蝴蝶结

- 来源：`1.6/Defs/Apparel/Apparel_PinkBow.xml`
- XML 类型：`ThingDef`
- 检索键：`PinkBow`
- `label`：粉色蝴蝶结
- `description`：一个粉色的蝴蝶结。佩戴后略显更有魅力。

### 党卫军盔甲

- 来源：`1.6/Defs/Apparel/Apparel_SSArmor.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_SSArmor`
- `label`：党卫军盔甲
- `description`：党卫军专用的战术盔甲，提供优秀的防护能力。采用金属板材和战术织物制成，能够有效抵御利器和钝器攻击。可以与战术背心一起穿戴。

### 党卫军头盔

- 来源：`1.6/Defs/Apparel/Apparel_SSHelmet.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_SSHelmet`
- `label`：党卫军头盔
- `description`：党卫军专用的重型战术头盔，采用先进的金属合金制造，提供卓越的头部防护。能够有效抵御利器、钝器和热能攻击，是党卫军标准装备的重要组成部分。

### 人狼盔甲

- 来源：`1.6/Defs/Apparel/Apparel_WerewolfArmor.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_WerewolfArmor`
- `label`：人狼盔甲
- `description`：重型动力盔甲，提供极强的防护能力。

### 人狼头盔

- 来源：`1.6/Defs/Apparel/Apparel_WerewolfHelmet.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_WerewolfHelmet`
- `label`：人狼头盔
- `description`：与人狼盔甲配套的重型战术头盔，提供极强的头部防护。

### 白色内衣

- 来源：`1.6/Defs/Apparel/Apparel_WhiteLingerie.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_WhiteLingerie`
- `label`：白色内衣
- `description`：一套纯白色的内衣，穿戴者更具魅力，社交互动更有优势。

### 白色泳衣

- 来源：`1.6/Defs/Apparel/Apparel_WhiteSwimsuit.xml`
- XML 类型：`ThingDef`
- 检索键：`Apparel_WhiteSwimsuit`
- `label`：白色泳衣
- `description`：一件清爽的白色泳衣，穿戴者更具魅力，社交更占优势。

## 武器与弹药

### Ash12电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_Ash12.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_AshTwelve_Charge`
- `label`：Ash12电荷弹

### Ash12

- 来源：`1.6/Defs/Weapons/Weapon_Ash12.xml`
- XML 类型：`ThingDef`
- 检索键：`Weapon_AshTwelve`
- `label`：Ash12
- `description`：Ash12电荷步枪，三连发，兼具稳定性与中近距离火力。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### 火焰喷射器

- 来源：`1.6/Defs/Weapons/Weapon_BeamFlamethrower.xml`
- XML 类型：`ThingDef`
- 检索键：`Weapon_BeamFlamethrower`
- `label`：火焰喷射器
- `description`：一把参考原版焚化器（Incinerator）的火焰喷射器。

### 指挥用长柄军刀

- 来源：`1.6/Defs/Weapons/Weapon_CommanderSaber.xml`
- XML 类型：`ThingDef`
- 检索键：`Weapon_CommanderSaber`
- `label`：指挥用长柄军刀
- `description`：一把专为指挥官设计的长柄军刀，兼具威严与实用性。
- `tools/li[1]/label`：刀刃
- `tools/li[2]/label`：刀尖

### 电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_HK416.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_HK416_Charge`
- `label`：电荷弹

### HK416突击步枪

- 来源：`1.6/Defs/Weapons/Weapon_HK416.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_HKFourOneSix`
- `label`：HK416突击步枪
- `description`：一把经过改造的HK416步枪，配备了先进的电荷发射系统。采用十连发设计，能够快速连续射击电荷弹药，在中近距离战斗中表现出色。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### 电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_LugerP08.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_LugerP08_Charge`
- `label`：电荷弹

### 鲁格P08手枪

- 来源：`1.6/Defs/Weapons/Weapon_LugerP08.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_LugerP08_Charge`
- `label`：鲁格P08手枪
- `description`：一把鲁格P08手枪，经过改造可发射低功率电荷弹。轻便易用，适合近距离自卫。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### 机枪弹

- 来源：`1.6/Defs/Weapons/Weapon_MG42.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_MG42_Charge`
- `label`：机枪弹

### MG42通用机枪

- 来源：`1.6/Defs/Weapons/Weapon_MG42.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_MG42_Charge`
- `label`：MG42通用机枪
- `description`：一把重型通用机枪，采用电荷弹药供能并以极高射速倾泻火力。适合中距离压制与持续火力输出。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### 铁拳火箭弹

- 来源：`1.6/Defs/Weapons/Weapon_Panzerfaust.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_PanzerfaustRocket`
- `label`：铁拳火箭弹

### 破甲爆破

- 来源：`1.6/Defs/Weapons/Weapon_Panzerfaust.xml`
- XML 类型：`DamageDef`
- 检索键：`Damage_Panzerfaust`
- `label`：破甲爆破
- `deathMessage`：{0} died in an explosion.

### 铁拳火箭筒

- 来源：`1.6/Defs/Weapons/Weapon_Panzerfaust.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_Panzerfaust`
- `label`：铁拳火箭筒
- `description`：一把威力巨大的单发反坦克火箭筒。
- `tools/li/label`：tube

### Pzb38电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_Pzb38.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_Pzb38_Charge`
- `label`：Pzb38电荷弹

### Pzb38反坦克步枪

- 来源：`1.6/Defs/Weapons/Weapon_Pzb38.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_Pzb38_Charge`
- `label`：Pzb38反坦克步枪
- `description`：一把重型反坦克步枪，使用电荷技术将单发弹丸加速并充能，以极高动能贯穿目标装甲。单发装填，瞄准时间较长，适合超远距离精确打击。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### Q11改进型爆炸弹

- 来源：`1.6/Defs/Weapons/Weapon_Q11Improved.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_Q11Improved_Explosive`
- `label`：Q11改进型爆炸弹

### Q11改进版

- 来源：`1.6/Defs/Weapons/Weapon_Q11Improved.xml`
- XML 类型：`ThingDef`
- 检索键：`Weapon_Q11Improved`
- `label`：Q11改进版
- `description`：Q11的改进版本，能够进行6连发范围爆炸打击。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### 电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_stg44.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_Stg44_Charge`
- `label`：电荷弹

### stg44突击步枪

- 来源：`1.6/Defs/Weapons/Weapon_stg44.xml`
- XML 类型：`ThingDef`
- 检索键：`Gun_Stg44_Charge`
- `label`：stg44突击步枪
- `description`：一把经过改造的HK416步枪，配备了先进的电荷发射系统。采用十连发设计，能够快速连续射击电荷弹药，在中近距离战斗中表现出色。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

### type10Gun电荷弹

- 来源：`1.6/Defs/Weapons/Weapon_type10Gun.xml`
- XML 类型：`ThingDef`
- 检索键：`Bullet_type10Gun_Charge`
- `label`：type10Gun电荷弹

### 10式狙击步枪

- 来源：`1.6/Defs/Weapons/Weapon_type10Gun.xml`
- XML 类型：`ThingDef`
- 检索键：`Weapon_type10Gun`
- `label`：10式狙击步枪
- `description`：一把10式狙击步枪，使用电荷技术将单发弹丸加速并充能，以极高动能贯穿目标装甲。单发装填，瞄准时间较长，适合超远距离精确打击。
- `tools/li[1]/label`：枪托
- `tools/li[2]/label`：枪管

## 物品与配方

### 金色钢笔

- 来源：`1.6/Defs/ThingDefs/GoldenPen.xml`
- XML 类型：`ThingDef`
- 检索键：`Miho_GoldenPen`
- `label`：金色钢笔
- `description`：一支做工精致的金色钢笔。佩戴它会让人显得更专业、更有说服力，从而提升社交表现。

### 我的奋斗

- 来源：`1.6/Defs/ThingDefs/myCareer.xml`
- XML 类型：`ThingDef`
- 检索键：`Miho_myCareer`
- `label`：我的奋斗
- `description`：一本书

### 制作肥皂

- 来源：`1.6/Defs/ThingDefs/Soap.xml`
- XML 类型：`RecipeDef`
- 检索键：`Make_Miho_Soap`
- `label`：制作肥皂
- `description`：将人肉熬炼并加工成可用于合成的肥皂。

### 肥皂

- 来源：`1.6/Defs/ThingDefs/Soap.xml`
- XML 类型：`ThingDef`
- 检索键：`Miho_Soap`
- `label`：肥皂
- `description`：由脂肪与碱性物质加工而成的清洁用品。主要作为合成原料使用。

## 能力与状态

### 洗脑

- 来源：`1.6/Defs/AbilityDefs/Ability_Brainwash.xml`
- XML 类型：`AbilityDef`
- 检索键：`Ability_Brainwash`
- `label`：洗脑
- `description`：

  > 对殖民者、奴隶或囚犯进行洗脑。
  > 效果：
  > 1. 清除所有记忆和人际关系（亲属/恋人）。
  > 2. 强制将文化信仰转换为殖民地主流文化。
  > 3. 获得“洗脑极乐”心情增益（+40，持续30天）。
  > 4. 囚犯将立即被招募并加入殖民地。
  > 5. 需要施法者走向目标并进行持续吟唱。
- `verbProperties/label`：洗脑

### 召唤党卫军

- 来源：`1.6/Defs/AbilityDefs/Ability_RequestSSReinforcements.xml`
- XML 类型：`AbilityDef`
- 检索键：`Ability_RequestSSReinforcements`
- `label`：召唤党卫军
- `description`：召唤五个身着（党卫军盔甲+美狐防弹衣）的美狐种族乘坐空降舱空投到殖民地中心（不会破坏建筑），召唤的美狐属性（射击和近战均大于10），武器在Hk416，Mg42通用机枪，单发式铁拳火箭筒，Stg44步枪，Pzb38反坦克步枪，火焰喷射器中随机选择。
- `verbProperties/label`：召唤党卫军

### 生命维持系统

- 来源：`1.6/Defs/HediffDefs/Hediff_WerewolfArmor_Regen.xml`
- XML 类型：`HediffDef`
- 检索键：`WerewolfArmor_Regen`
- `label`：生命维持系统
- `description`：由生命维持系统提供的异常再生能力：伤口愈合极快，并显著提高免疫增长。

## 事件与工作

### soap recruit caravan arrival

- 来源：`1.6/Defs/IncidentDef/Incident_SoapRecruitCaravan_Yearly.xml`
- XML 类型：`IncidentDef`
- 检索键：`CaravanArrivalSoapRecruit`
- `label`：soap recruit caravan arrival

### 阿道夫-秋霜来访

- 来源：`1.6/Defs/IncidentDef/Incidents_MihoAdf.xml`
- XML 类型：`IncidentDef`
- 检索键：`Incidents_MihoAdf`
- `label`：阿道夫-秋霜来访

### Miho_Brainwash

- 来源：`1.6/Defs/JobDef/Miho_Brainwash.xml`
- XML 类型：`JobDef`
- 检索键：`Miho_Brainwash`
- `reportString`：正在洗脑 TargetA 。

### Miho_RefuelSprayerJob

- 来源：`1.6/Defs/JobDef/Miho_RefuelSprayerJob.xml`
- XML 类型：`JobDef`
- 检索键：`Miho_RefuelSprayerJob`
- `reportString`：reloading weapon.

### Miho_TalkToPawn

- 来源：`1.6/Defs/JobDef/Miho_TalkToPawn.xml`
- XML 类型：`JobDef`
- 检索键：`Miho_TalkToPawn`
- `reportString`：正在与 TargetA 交谈。

### Miho_ToMyCareerFoPawn

- 来源：`1.6/Defs/JobDef/Miho_ToMyCareerFoPawn.xml`
- XML 类型：`JobDef`
- 检索键：`Miho_ToMyCareerFoPawn`
- `reportString`：正在使用我的奋斗进行洗脑

## 心情与记忆

### 我什么都忘了

- 来源：`1.6/Defs/ThoughtDefs/Thought_BrainwashHappy.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thought_BrainwashHappy`
- `label`：我什么都忘了
- `description`：我什么都忘了... 感觉很平静。
- `stages/li/label`：我什么都忘了
- `stages/li/description`：我的过去消失了。我很开心。

### Thought_ExperiencedTribulation

- 来源：`1.6/Defs/ThoughtDefs/Thought_ExperiencedTribulation.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thought_ExperiencedTribulation`
- `stages/li/label`：经历劫难
- `stages/li/description`：我希望蓝天上的声音，是鸟儿飞过，树叶在莎莎作响。不是轰鸣的飞机，不是致命的炮弹，我不想，也不愿意看见这一切发生，我喜欢战争，但也讨厌它，因为不得不做。美狐第三帝国，诞生于美狐政权的暴政，我看见了太多惨案，它们本不该发生。

### Thought_FuhrerPromise

- 来源：`1.6/Defs/ThoughtDefs/Thought_FuhrerPromise.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thought_FuhrerPromise`
- `stages/li/label`：元首的承诺
- `stages/li/description`：我们的领袖与我们同在，为了未来的荣耀！

### Thought_NotLeaderOrMoralGuide

- 来源：`1.6/Defs/ThoughtDefs/Thought_NotLeaderOrMoralGuide.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thought_NotLeaderOrMoralGuide`
- `stages/li/label`：未担任领袖或思想家
- `stages/li/description`：想想吧，如果由我带领他们，这个殖民地将会变得多么强大。

### Thought_PregnantRealization

- 来源：`1.6/Defs/ThoughtDefs/Thought_PregnantRealization.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thought_PregnantRealization`
- `stages/li/label`：怀孕
- `stages/li/description`：抱歉，孩子，摆在你的面前，可能是残酷的现实。

### Thoughts_BabyBornIdeal

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_BabyBornIdeal.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_BabyBornIdeal`
- `stages/li/label`：孩子出生
- `stages/li/description`：如果有梦，就去追逐它吧，至于我的理想，不需要下一辈来承担它。

### Thoughts_ColonistDiedRespect

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_ColonistDiedRespect.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_ColonistDiedRespect`
- `stages/li/label`：殖民者死亡
- `stages/li/description`：你的死亡，所有人都应该铭记，安息吧，我的战士。

### Thoughts_GotMarriedIdeal

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_GotMarriedIdeal.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_GotMarriedIdeal`
- `stages/li/label`：结婚
- `stages/li/description`：我希望，我能和你一起，继续我的理想。待到鲜花开满大地，夕阳落下，银河闪耀之时。

### Thoughts_GotSomeLovin

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_GotSomeLovin.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_GotSomeLovin`
- `stages/li/label`：滚床单
- `stages/li/description`：唔，这不对，等下，哈....哈.....哈......绝不是，绝不是因为我的体质特殊！

### Miho_AteFineMeal_Critique

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Memory_Eating.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_AteFineMeal_Critique`
- `stages/li/label`：吃了精致食物
- `stages/li/description`：厨艺还算不错，有待提升，可惜，这个地方的红酒太少了。

### Miho_AteHumanMeat_Acceptance

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Memory_Eating.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_AteHumanMeat_Acceptance`
- `stages/li/label`：吃了人肉
- `stages/li/description`：或许，汉尼拔做的没错，脂肪也不该浪费。

### Miho_AteLavishMeal_Praise

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Memory_Eating.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_AteLavishMeal_Praise`
- `stages/li/label`：吃了奢侈食物
- `stages/li/description`：相当美味，或许，我应该把制作这道菜的家伙聘请为私人厨师了。

### Miho_AteNutrientPaste_Disgust

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Memory_Eating.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_AteNutrientPaste_Disgust`
- `stages/li/label`：吃了营养膏
- `stages/li/description`：呕！渣渣！这么难吃的东西是喂猪的吗！

### Thoughts_MihoEmpireAmbition

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_MihoEmpireAmbition.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_MihoEmpireAmbition`
- `stages/li/label`：殖民地有很多美狐
- `stages/li/description`：这里的大家都非常敬仰我，那么，我不该让他们失望！这颗星球，用不了多少时间，它会成为美狐第三帝国的领土的！无论是米莉拉，抑或是人类，都阻止不了我！

### Miho_AcceptedCourtship_DelayIdeal

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Romance.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_AcceptedCourtship_DelayIdeal`
- `stages/li/label`：接受求爱
- `stages/li/description`：唔，可惜，我的理想恐怕要暂时延期一段时间了。

### Miho_RejectedCourtship_Patriotism

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_Romance.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Miho_RejectedCourtship_Patriotism`
- `stages/li/label`：拒绝求爱
- `stages/li/description`：抱歉，我所挂念的，只有祖国，抱歉，对于你的爱念，也许，需要等到和平来临的一刻。

### Thoughts_WitnessedMarriageBlessing

- 来源：`1.6/Defs/ThoughtDefs/Thoughts_WitnessedMarriageBlessing.xml`
- XML 类型：`ThoughtDef`
- 检索键：`Thoughts_WitnessedMarriageBlessing`
- `stages/li/label`：见证结婚
- `stages/li/description`：呼，祝福这对新人吧，只是，在这颗星球，这样的好日子恐怕不会持续多久。

## 命名词库

### MihoThirdEmpire_SettlementNames_GermanCities

- 来源：`1.6/Defs/RulePackDefs/RulePack_GermanSettlementNames.xml`
- XML 类型：`RulePackDef`
- 检索键：`MihoThirdEmpire_SettlementNames_GermanCities`
- `rulePack/rulesStrings/li[1]`：root->[city]
- `rulePack/rulesStrings/li[2]`：city->柏林
- `rulePack/rulesStrings/li[3]`：city->汉堡
- `rulePack/rulesStrings/li[4]`：city->慕尼黑
- `rulePack/rulesStrings/li[5]`：city->科隆
- `rulePack/rulesStrings/li[6]`：city->法兰克福
- `rulePack/rulesStrings/li[7]`：city->斯图加特
- `rulePack/rulesStrings/li[8]`：city->杜塞尔多夫
- `rulePack/rulesStrings/li[9]`：city->多特蒙德
- `rulePack/rulesStrings/li[10]`：city->埃森
- `rulePack/rulesStrings/li[11]`：city->莱比锡
- `rulePack/rulesStrings/li[12]`：city->不来梅
- `rulePack/rulesStrings/li[13]`：city->德累斯顿
- `rulePack/rulesStrings/li[14]`：city->汉诺威
- `rulePack/rulesStrings/li[15]`：city->纽伦堡
- `rulePack/rulesStrings/li[16]`：city->杜伊斯堡
- `rulePack/rulesStrings/li[17]`：city->波鸿
- `rulePack/rulesStrings/li[18]`：city->伍珀塔尔
- `rulePack/rulesStrings/li[19]`：city->比勒费尔德
- `rulePack/rulesStrings/li[20]`：city->波恩
- `rulePack/rulesStrings/li[21]`：city->曼海姆
- `rulePack/rulesStrings/li[22]`：city->卡尔斯鲁厄
- `rulePack/rulesStrings/li[23]`：city->奥格斯堡
- `rulePack/rulesStrings/li[24]`：city->威斯巴登
- `rulePack/rulesStrings/li[25]`：city->盖尔森基兴
- `rulePack/rulesStrings/li[26]`：city->门兴格拉德巴赫
- `rulePack/rulesStrings/li[27]`：city->不伦瑞克
- `rulePack/rulesStrings/li[28]`：city->开姆尼茨
- `rulePack/rulesStrings/li[29]`：city->基尔
- `rulePack/rulesStrings/li[30]`：city->亚琛
- `rulePack/rulesStrings/li[31]`：city->哈勒
- `rulePack/rulesStrings/li[32]`：city->马格德堡
- `rulePack/rulesStrings/li[33]`：city->弗赖堡
- `rulePack/rulesStrings/li[34]`：city->克雷费尔德
- `rulePack/rulesStrings/li[35]`：city->吕贝克
- `rulePack/rulesStrings/li[36]`：city->奥伯豪森
- `rulePack/rulesStrings/li[37]`：city->爱尔福特
- `rulePack/rulesStrings/li[38]`：city->美因茨
- `rulePack/rulesStrings/li[39]`：city->罗斯托克
- `rulePack/rulesStrings/li[40]`：city->卡塞尔
- `rulePack/rulesStrings/li[41]`：city->萨尔布吕肯
- `rulePack/rulesStrings/li[42]`：city->波茨坦
- `rulePack/rulesStrings/li[43]`：city->明斯特

### NamerPerson_MihoThirdEmpire_GermanCN

- 来源：`1.6/Defs/RulePackDefs/RulePack_NamerPerson_GermanCN.xml`
- XML 类型：`RulePackDef`
- 检索键：`NamerPerson_MihoThirdEmpire_GermanCN`
- `rulePack/rulesStrings/li[1]`：r_name->[given]·[family]
- `rulePack/rulesStrings/li[2]`：given->阿道夫
- `rulePack/rulesStrings/li[3]`：given->奥托
- `rulePack/rulesStrings/li[4]`：given->海因里希
- `rulePack/rulesStrings/li[5]`：given->威廉
- `rulePack/rulesStrings/li[6]`：given->卡尔
- `rulePack/rulesStrings/li[7]`：given->约翰
- `rulePack/rulesStrings/li[8]`：given->弗里德里希
- `rulePack/rulesStrings/li[9]`：given->鲁道夫
- `rulePack/rulesStrings/li[10]`：given->埃里希
- `rulePack/rulesStrings/li[11]`：given->赫尔曼
- `rulePack/rulesStrings/li[12]`：given->马克斯
- `rulePack/rulesStrings/li[13]`：given->沃尔特
- `rulePack/rulesStrings/li[14]`：given->保罗
- `rulePack/rulesStrings/li[15]`：given->汉斯
- `rulePack/rulesStrings/li[16]`：given->约瑟夫
- `rulePack/rulesStrings/li[17]`：given->阿尔伯特
- `rulePack/rulesStrings/li[18]`：given->理查德
- `rulePack/rulesStrings/li[19]`：given->维尔纳
- `rulePack/rulesStrings/li[20]`：family->施密特
- `rulePack/rulesStrings/li[21]`：family->穆勒
- `rulePack/rulesStrings/li[22]`：family->施耐德
- `rulePack/rulesStrings/li[23]`：family->费舍尔
- `rulePack/rulesStrings/li[24]`：family->韦伯
- `rulePack/rulesStrings/li[25]`：family->迈耶
- `rulePack/rulesStrings/li[26]`：family->瓦格纳
- `rulePack/rulesStrings/li[27]`：family->贝克尔
- `rulePack/rulesStrings/li[28]`：family->霍夫曼
- `rulePack/rulesStrings/li[29]`：family->里希特
- `rulePack/rulesStrings/li[30]`：family->克莱因
- `rulePack/rulesStrings/li[31]`：family->沃尔夫
- `rulePack/rulesStrings/li[32]`：family->克劳斯
- `rulePack/rulesStrings/li[33]`：family->朗格
- `rulePack/rulesStrings/li[34]`：family->布劳恩
- `rulePack/rulesStrings/li[35]`：family->施瓦茨

## 运行时界面文本

以下内容来自 C# 字符串。连续行上的多个片段通常会在游戏中拼接成完整句子。

### `1.6/Source/MihoAdfqs/MihoAdfComp/ThingComp_FueledSprayer.cs:110`
- `text`：Fuel: 
- `codeContext`：`return "Fuel: " + fuel + "/" + Props.maxFuel + (needsReload ? " (Needs Reloading)" : "");`

### `1.6/Source/MihoAdfqs/MihoAdfComp/ThingComp_FueledSprayer.cs:110`
- `text`： (Needs Reloading)
- `codeContext`：`return "Fuel: " + fuel + "/" + Props.maxFuel + (needsReload ? " (Needs Reloading)" : "");`

### `1.6/Source/MihoAdfqs/MihoAdfComp/ThingComp_FueledSprayer.cs:169`
- `text`：燃料不足！
- `codeContext`：`Messages.Message("燃料不足！", MessageTypeDefOf.RejectInput);`

### `1.6/Source/MihoAdfqs/MihoAdfIncidents/IncidentWorker_MihoAdfApproach.cs:40`
- `text`：美狐来访
- `codeContext`：`string label = "美狐来访";`

### `1.6/Source/MihoAdfqs/MihoAdfIncidents/IncidentWorker_MihoAdfApproach.cs:41`
- `text`：

  > 一位身着军服的美狐正在缓缓靠近你的殖民地？
  > 
  > 
- `codeContext`：`string text = "一位身着军服的美狐正在缓缓靠近你的殖民地？\n\n" +`

### `1.6/Source/MihoAdfqs/MihoAdfIncidents/IncidentWorker_MihoAdfApproach.cs:42`
- `text`：

  > 或许她并非美狐？对方的面孔洋溢着青春的神色，但谈话水平相当老练。
  > 
  > 
- `codeContext`：`"或许她并非美狐？对方的面孔洋溢着青春的神色，但谈话水平相当老练。\n\n" +`

### `1.6/Source/MihoAdfqs/MihoAdfIncidents/IncidentWorker_MihoAdfApproach.cs:43`
- `text`：

  > 她正想跟你的殖民者们谈谈加入殖民地的事情。
  > 
- `codeContext`：`"她正想跟你的殖民者们谈谈加入殖民地的事情。\n" +`

### `1.6/Source/MihoAdfqs/MihoAdfIncidents/IncidentWorker_MihoAdfApproach.cs:44`
- `text`：（她将在殖民地停留 3 天，随后自行离开。你可以派人前去交谈。）
- `codeContext`：`"（她将在殖民地停留 3 天，随后自行离开。你可以派人前去交谈。）";`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_Brainwash.cs:65`
- `text`：{target.LabelShort} 已被洗脑！
- `codeContext`：`Messages.Message($"{target.LabelShort} 已被洗脑！", target, MessageTypeDefOf.PositiveEvent, true);`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:41`
- `text`：嗯，我大概在附近观察了几天，殖民地的思想很符合我的见解，正好，如今，我正需要一段特殊经历的历练。如果可以的话，愿意让我加入这里吗？放心，这只是一个小小的请求，不接受，也不会影响什么。
- `codeContext`：`DiaNode rootNode = new DiaNode("嗯，我大概在附近观察了几天，殖民地的思想很符合我的见解，正好，如今，我正需要一段特殊经历的历练。如果可以的话，愿意让我加入这里吗？放心，这只是一个小小的请求，不接受，也不会影响什么。");`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:43`
- `text`：允许加入
- `codeContext`：`DiaOption optJoin = new DiaOption("允许加入")`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:56`
- `text`：{p.LabelShort} 已加入你的殖民地！
- `codeContext`：`Messages.Message($"{p.LabelShort} 已加入你的殖民地！", p, MessageTypeDefOf.PositiveEvent);`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:63`
- `text`：驱逐
- `codeContext`：`DiaOption optBanish = new DiaOption("驱逐")`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:67`
- `text`：{p.LabelShort} 正在离开。
- `codeContext`：`Messages.Message($"{p.LabelShort} 正在离开。", p, MessageTypeDefOf.NeutralEvent);`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:78`
- `text`：攻击
- `codeContext`：`DiaOption optAttack = new DiaOption("攻击")`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:88`
- `text`：{p.LabelShort} 变得充满敌意！
- `codeContext`：`Messages.Message($"{p.LabelShort} 变得充满敌意！", p, MessageTypeDefOf.ThreatSmall);`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:97`
- `text`：与
- `codeContext`：`Find.WindowStack.Add(new Dialog_NodeTree(rootNode, true, true, "与"+pawn.LabelShort+ "交谈"));`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_TalkToMiho.cs:97`
- `text`：交谈
- `codeContext`：`Find.WindowStack.Add(new Dialog_NodeTree(rootNode, true, true, "与"+pawn.LabelShort+ "交谈"));`

### `1.6/Source/MihoAdfqs/MihoAdfJobDriver/JobDriver_ToMyCareerFoPawn.cs:105`
- `text`：{actor.LabelShort} 完成了对 {target.LabelShort} 的洗脑！
- `codeContext`：`Messages.Message($"{actor.LabelShort} 完成了对 {target.LabelShort} 的洗脑！", MessageTypeDefOf.PositiveEvent);`

### `1.6/Source/MihoAdfqs/MihoAdfqsVerb/Verb_Brainwash.cs:41`
- `text`：必须选择一个人。
- `codeContext`：`Messages.Message("必须选择一个人。", MessageTypeDefOf.RejectInput, false);`

### `1.6/Source/MihoAdfqs/MihoAdfqsVerb/Verb_Brainwash.cs:49`
- `text`：目标必须是人类。
- `codeContext`：`Messages.Message("目标必须是人类。", MessageTypeDefOf.RejectInput, false);`

### `1.6/Source/MihoAdfqs/MihoAdfqsVerb/Verb_Brainwash.cs:59`
- `text`：只能对自己殖民地的成员、奴隶或囚犯使用。
- `codeContext`：`Messages.Message("只能对自己殖民地的成员、奴隶或囚犯使用。", MessageTypeDefOf.RejectInput, false);`

### `1.6/Source/MihoAdfqs/MihoAdfqsVerb/Verb_RequestSSReinforcements.cs:28`
- `text`：没有可用的盟友派系来响应呼叫！
- `codeContext`：`Messages.Message("没有可用的盟友派系来响应呼叫！", MessageTypeDefOf.RejectInput, false);`

### `1.6/Source/MihoAdfqs/Patches/FloatMenuMakerMap.cs:63`
- `text`：交谈
- `codeContext`：`string label = "交谈"; // 菜单显示的文字`

### `1.6/Source/MihoAdfqs/Patches/FloatMenuMakerMap.cs:73`
- `text`： (无法到达)
- `codeContext`：`option.Label += " (无法到达)";`

### `1.6/Source/MihoAdfqs/Patches/Patch_InteractionWorker_Convert.cs:97`
- `text`：教化成功！
- `codeContext`：`MoteMaker.ThrowText(recipient.DrawPos, recipient.Map, "教化成功！", 8f);`

### `1.6/Source/MihoAdfqs/Patches/Patch_Pawn_GetGizmos_FuelBar.cs:59`
- `text`：Reload 
- `codeContext`：`string label = "Reload " + eq.LabelShort + " with " + t.Label;`

### `1.6/Source/MihoAdfqs/Patches/Patch_Pawn_GetGizmos_FuelBar.cs:59`
- `text`： with 
- `codeContext`：`string label = "Reload " + eq.LabelShort + " with " + t.Label;`

### `1.6/Source/MihoAdfqs/Patches/Patch_Pawn_GetGizmos_FuelBar.cs:147`
- `text`：燃料 {comp.fuel}/{comp.Props.maxFuel}
- `codeContext`：`Widgets.Label(barRect, $"燃料 {comp.fuel}/{comp.Props.maxFuel}");`

### `1.6/Source/MihoAdfqs/Patches/Patch_Pawn_GetGizmos_FuelBar.cs:153`
- `text`：

  > 燃料不足将无法开火。
  > 殖民者会在非征召状态下自动寻找燃料装填。
- `codeContext`：`TooltipHandler.TipRegion(rect, "燃料不足将无法开火。\n殖民者会在非征召状态下自动寻找燃料装填。");`

### `1.6/Source/MihoAdfqs/Patches/Patch_Pawn_Immortal.cs:79`
- `text`：不死之身！
- `codeContext`：`MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "不死之身！", 8f);`

### `1.6/Source/MihoAdfqs/ThingClass/myCareerThing.cs:38`
- `text`：对目标对象进行洗脑
- `codeContext`：`string label = "对目标对象进行洗脑";`

### `1.6/Source/MihoAdfqs/ThingClass/myCareerThing.cs:44`
- `text`： (冷却中: 剩余 {daysLeft:F1} 天)
- `codeContext`：`label += $" (冷却中: 剩余 {daysLeft:F1} 天)";`
