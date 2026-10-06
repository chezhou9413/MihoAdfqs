[CmdletBinding()]
param(
    [string]$SourceRoot = (Join-Path $PSScriptRoot '..\Assets\Phase2'),
    [string]$ModRoot = (Join-Path $PSScriptRoot '..'),
    [switch]$VerifyOnly,
    [switch]$ListOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Mappings = [System.Collections.Generic.List[object]]::new()

#登记一张源贴图与目标贴图的明确对应关系。
function Add-TextureMapping {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRelative,
        [Parameter(Mandatory = $true)][string]$TargetRelative
    )

    $Mappings.Add([PSCustomObject]@{
        Source = $SourceRelative
        Target = $TargetRelative
    })
}

#按侧、正、背顺序登记一组三方向贴图，并按需登记物品图。
function Add-ThreeViewMapping {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDirectory,
        [Parameter(Mandatory = $true)][string]$SourceStem,
        [Parameter(Mandatory = $true)][string]$TargetDirectory,
        [Parameter(Mandatory = $true)][string]$TargetStem,
        [switch]$IncludeItem
    )

    Add-TextureMapping "$SourceDirectory\$SourceStem 侧.png" "$TargetDirectory\${TargetStem}_east.png"
    Add-TextureMapping "$SourceDirectory\$SourceStem 正.png" "$TargetDirectory\${TargetStem}_south.png"
    Add-TextureMapping "$SourceDirectory\$SourceStem 背.png" "$TargetDirectory\${TargetStem}_north.png"
    if ($IncludeItem) {
        Add-TextureMapping "$SourceDirectory\$SourceStem 物品.png" "$TargetDirectory\$TargetStem.png"
    }
}

#登记赫尔默表情的右、左、正三方向贴图。
function Add-HelmerFaceMapping {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDirectory,
        [Parameter(Mandatory = $true)][string]$SourceState,
        [Parameter(Mandatory = $true)][string]$TargetDirectory,
        [Parameter(Mandatory = $true)][string]$TargetPrefix
    )

    Add-TextureMapping "$SourceDirectory\$SourceState 右侧.png" "$TargetDirectory\${TargetPrefix}_east.png"
    Add-TextureMapping "$SourceDirectory\$SourceState 左侧.png" "$TargetDirectory\${TargetPrefix}_west.png"
    Add-TextureMapping "$SourceDirectory\$SourceState 正.png" "$TargetDirectory\${TargetPrefix}_south.png"
}

#登记提亚娜表情的侧、正两方向贴图。
function Add-TianaFaceMapping {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDirectory,
        [Parameter(Mandatory = $true)][string]$SourceState,
        [Parameter(Mandatory = $true)][string]$TargetDirectory,
        [Parameter(Mandatory = $true)][string]$TargetPrefix
    )

    Add-TextureMapping "$SourceDirectory\$SourceState 侧.png" "$TargetDirectory\${TargetPrefix}_east.png"
    Add-TextureMapping "$SourceDirectory\$SourceState 正.png" "$TargetDirectory\${TargetPrefix}_south.png"
}

#武器，共14张。
Add-TextureMapping '武器\aa12.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_AA12.png'
Add-TextureMapping '武器\dsr 1.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_DSR1.png'
Add-TextureMapping '武器\dsr 50.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_DSR50.png'
Add-TextureMapping '武器\g36.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_G36.png'
Add-TextureMapping '武器\hk416 榴弹.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_HK416GrenadeLauncher.png'
Add-TextureMapping '武器\hk416 霰弹.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_HK416Shotgun.png'
Add-TextureMapping '武器\mp7.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_MP7.png'
Add-TextureMapping '武器\usp1.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_USPSuppressed.png'
Add-TextureMapping '武器\usp2.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_USP.png'
Add-TextureMapping '武器\手雷.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_SupernovaFusionGrenade.png'
Add-TextureMapping '武器\毒气.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_SupernovaHeavyLauncher.png'
Add-TextureMapping '武器\激光步枪.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_SupernovaLaserRifle.png'
Add-TextureMapping '武器\电磁射手.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_SupernovaEMMarksmanRifle.png'
Add-TextureMapping '武器\电磁精准.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_SupernovaEMPrecisionRifle.png'

#食物、材料、药物与通讯道具，共23张。
Add-TextureMapping '道具\啤酒 会腐烂.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_ImperialDraftBeer.png'
Add-TextureMapping '道具\啤酒.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_ImperialCannedBeer.png'
Add-TextureMapping '道具\小蛋糕.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_RefinedFruitPastry\MihoPhase2_RefinedFruitPastry.png'
Add-TextureMapping '道具\小蛋糕 堆叠.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_RefinedFruitPastry\MihoPhase2_RefinedFruitPastry_Stack.png'
Add-TextureMapping '道具\曲奇.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_MysticCookie\MihoPhase2_MysticCookie.png'
Add-TextureMapping '道具\曲奇 堆叠.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_MysticCookie\MihoPhase2_MysticCookie_Stack.png'
Add-TextureMapping '道具\罐头.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_ImperialMilitaryRation\MihoPhase2_ImperialMilitaryRation.png'
Add-TextureMapping '道具\罐头 堆叠2.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_ImperialMilitaryRation\MihoPhase2_ImperialMilitaryRation_Stack2.png'
Add-TextureMapping '道具\罐头 堆叠.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_ImperialMilitaryRation\MihoPhase2_ImperialMilitaryRation_Stack3.png'
Add-TextureMapping '道具\面包.png' 'Textures\Thing\MihoPhase2\Food\MihoPhase2_CommemorativeBread.png'
Add-TextureMapping '道具\新星.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_NovaMaterial\MihoPhase2_NovaMaterial.png'
Add-TextureMapping '道具\新星 堆叠.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_NovaMaterial\MihoPhase2_NovaMaterial_Stack.png'
Add-TextureMapping '道具\聚乙烯.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_Polyethylene\MihoPhase2_Polyethylene.png'
Add-TextureMapping '道具\聚乙烯 堆叠.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_Polyethylene\MihoPhase2_Polyethylene_Stack.png'
Add-TextureMapping '道具\钢板.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_IndustrialSteelPlate\MihoPhase2_IndustrialSteelPlate.png'
Add-TextureMapping '道具\钢板 堆叠.png' 'Textures\Thing\MihoPhase2\Material\MihoPhase2_IndustrialSteelPlate\MihoPhase2_IndustrialSteelPlate_Stack.png'
Add-TextureMapping '道具\注射器 修复.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_SupernovaTreatmentInjector.png'
Add-TextureMapping '道具\注射器 激发.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_SupernovaActivationInjector.png'
Add-TextureMapping '道具\注射器 止痛.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_SupernovaAnalgesicInjector.png'
Add-TextureMapping '道具\注射器 兴奋.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_SupernovaStimulantInjector.png'
Add-TextureMapping '道具\注射器 紧急.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_SupernovaEmergencyInjector.png'
Add-TextureMapping '道具\注射器 人道处理.png' 'Textures\Thing\MihoPhase2\Medicine\MihoPhase2_HumaneConversionInjector.png'
Add-TextureMapping '道具\轨道呼叫.png' 'Textures\Thing\MihoPhase2\Utility\MihoPhase2_OrbitalCommunicator.png'

#建筑及其分层图，共17张。
Add-TextureMapping '建筑\堡垒 底座.png' 'Textures\Building\MihoPhase2\Fortress\MihoPhase2_ImperialFortressBase.png'
Add-TextureMapping '建筑\堡垒 88炮.png' 'Textures\Building\MihoPhase2\Fortress\MihoPhase2_ImperialFortress_Cannon88mm.png'
Add-TextureMapping '建筑\堡垒 速射炮.png' 'Textures\Building\MihoPhase2\Fortress\MihoPhase2_ImperialFortress_Autocannon30mm.png'
Add-TextureMapping '建筑\太阳灯.png' 'Textures\Building\MihoPhase2\MihoPhase2_ImperialAgriculturalFloodlight.png'
Add-TextureMapping '建筑\打印机.png' 'Textures\Building\MihoPhase2\MihoPhase2_Supernova3DPrinter.png'
Add-TextureMapping '建筑\聚乙烯制造.png' 'Textures\Building\MihoPhase2\MihoPhase2_NovaAggregator.png'
Add-TextureMapping '建筑\酒桶.png' 'Textures\Building\MihoPhase2\MihoPhase2_ImperialBeerBarrel.png'
Add-TextureMapping '建筑\药物制作 正.png' 'Textures\Building\MihoPhase2\DrugFabricator\MihoPhase2_SupernovaDrugFabricator_south.png'
Add-TextureMapping '建筑\药物制作 侧.png' 'Textures\Building\MihoPhase2\DrugFabricator\MihoPhase2_SupernovaDrugFabricator_east.png'
Add-TextureMapping '建筑\种植 正 顶盖.png' 'Textures\Building\MihoPhase2\IndoorCultivator\MihoPhase2_IndoorCultivatorCover_south.png'
Add-TextureMapping '建筑\种植 正 内衬.png' 'Textures\Building\MihoPhase2\IndoorCultivator\MihoPhase2_IndoorCultivatorInterior_south.png'
Add-TextureMapping '建筑\种植 侧 顶盖.png' 'Textures\Building\MihoPhase2\IndoorCultivator\MihoPhase2_IndoorCultivatorCover_east.png'
Add-TextureMapping '建筑\种植 侧 内衬.png' 'Textures\Building\MihoPhase2\IndoorCultivator\MihoPhase2_IndoorCultivatorInterior_east.png'
Add-TextureMapping '建筑\转化机 正 顶盖.png' 'Textures\Building\MihoPhase2\MedicalPod\MihoPhase2_SupernovaMedicalPodCover_south.png'
Add-TextureMapping '建筑\转化机 正 内衬.png' 'Textures\Building\MihoPhase2\MedicalPod\MihoPhase2_SupernovaMedicalPodInterior_south.png'
Add-TextureMapping '建筑\转化机 侧 顶盖.png' 'Textures\Building\MihoPhase2\MedicalPod\MihoPhase2_SupernovaMedicalPodCover_east.png'
Add-TextureMapping '建筑\转化机 侧 内衬.png' 'Textures\Building\MihoPhase2\MedicalPod\MihoPhase2_SupernovaMedicalPodInterior_east.png'

#技能图标与派系图标，共8张。
Add-TextureMapping '技能\洗脑.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_Brainwash.png'
Add-TextureMapping '技能\洗脑 2.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_Brainwash_Alt.png'
Add-TextureMapping '技能\派系.png' 'Textures\UI\MihoPhase2\Factions\MihoPhase2_ThirdEmpire.png'
Add-TextureMapping '技能\空投.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_RequestBodyguards.png'
Add-TextureMapping '技能\米技能.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_TianaInspiration.png'
Add-TextureMapping '技能\翻滚.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_HelmerZogRoll.png'
Add-TextureMapping '技能\骷髅 未激活.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_HelmerZogBloodStorm_Inactive.png'
Add-TextureMapping '技能\骷髅 激活.png' 'Textures\UI\MihoPhase2\Abilities\MihoPhase2_HelmerZogBloodStorm_Active.png'

#普通衣物与装备，共76张。
Add-ThreeViewMapping '衣服' '保镖重甲' 'Textures\apparel\MihoPhase2\DemiHeavyArmorAlpha' 'MihoPhase2_DemiHeavyArmorAlpha'
Add-ThreeViewMapping '衣服' '重甲 普通糊糊' 'Textures\apparel\MihoPhase2\DemiHeavyArmor' 'MihoPhase2_DemiHeavyArmor'
Add-TextureMapping '衣服\重甲 物品.png' 'Textures\apparel\MihoPhase2\DemiHeavyArmor\MihoPhase2_DemiHeavyArmor.png'
Add-ThreeViewMapping '衣服' '四眼夜视仪' 'Textures\apparel\MihoPhase2\PSOStandardNVGHelmet' 'MihoPhase2_PSOStandardNVGHelmet' -IncludeItem
Add-ThreeViewMapping '衣服' '夜视仪 独眼' 'Textures\apparel\MihoPhase2\PSOSpecialNVGHelmet' 'MihoPhase2_PSOSpecialNVGHelmet' -IncludeItem
Add-ThreeViewMapping '衣服' '太空头' 'Textures\apparel\MihoPhase2\SupernovaDropHelmet' 'MihoPhase2_SupernovaDropHelmet' -IncludeItem
Add-ThreeViewMapping '衣服' '太空甲' 'Textures\apparel\MihoPhase2\SupernovaDropArmor' 'MihoPhase2_SupernovaDropArmor' -IncludeItem
Add-ThreeViewMapping '衣服' '盾牌' 'Textures\apparel\MihoPhase2\AssaultTowerShield' 'MihoPhase2_AssaultTowerShield' -IncludeItem
Add-TextureMapping '衣服\正面盾.png' 'Textures\apparel\MihoPhase2\HaliviShieldBelt\MihoPhase2_HaliviShieldBelt.png'
Add-TextureMapping '衣服\日耀盾.png' 'Textures\apparel\MihoPhase2\SupernovaShieldGenerator\MihoPhase2_SupernovaShieldGenerator.png'
Add-TextureMapping '衣服\防弹板.png' 'Textures\apparel\MihoPhase2\KaleviArmorPlate\MihoPhase2_KaleviArmorPlate.png'
Add-ThreeViewMapping '衣服' '国防军 男' 'Textures\apparel\MihoPhase2\DefenseForceUniform' 'MihoPhase2_DefenseForceUniform'
Add-ThreeViewMapping '衣服' '国防军 女' 'Textures\apparel\MihoPhase2\DefenseForceUniform' 'MihoPhase2_DefenseForceUniform_Female'
Add-TextureMapping '衣服\国防军 物品.png' 'Textures\apparel\MihoPhase2\DefenseForceUniform\MihoPhase2_DefenseForceUniform.png'
Add-ThreeViewMapping '衣服' '女仆 男' 'Textures\apparel\MihoPhase2\MaidDress' 'MihoPhase2_MaidDress'
Add-TextureMapping '衣服\女仆 女 侧.png' 'Textures\apparel\MihoPhase2\MaidDress\MihoPhase2_MaidDress_Female_east.png'
Add-TextureMapping '衣服\女仆 女 正.png' 'Textures\apparel\MihoPhase2\MaidDress\MihoPhase2_MaidDress_Female_south.png'
Add-TextureMapping '衣服\女仆 女  背.png' 'Textures\apparel\MihoPhase2\MaidDress\MihoPhase2_MaidDress_Female_north.png'
Add-TextureMapping '衣服\女仆 物品.png' 'Textures\apparel\MihoPhase2\MaidDress\MihoPhase2_MaidDress.png'
Add-ThreeViewMapping '衣服' '性感 男' 'Textures\apparel\MihoPhase2\FashionOutfit' 'MihoPhase2_FashionOutfit'
Add-ThreeViewMapping '衣服' '性感 普通' 'Textures\apparel\MihoPhase2\FashionOutfit' 'MihoPhase2_FashionOutfit_Female'
Add-TextureMapping '衣服\性感 物品.png' 'Textures\apparel\MihoPhase2\FashionOutfit\MihoPhase2_FashionOutfit.png'
Add-ThreeViewMapping '衣服' '阿迪达斯 男' 'Textures\apparel\MihoPhase2\HoodieOutfit' 'MihoPhase2_HoodieOutfit'
Add-ThreeViewMapping '衣服' '阿迪达斯 女' 'Textures\apparel\MihoPhase2\HoodieOutfit' 'MihoPhase2_HoodieOutfit_Female'
Add-TextureMapping '衣服\阿迪达斯 物品.png' 'Textures\apparel\MihoPhase2\HoodieOutfit\MihoPhase2_HoodieOutfit.png'
Add-ThreeViewMapping '衣服' '酒糊' 'Textures\apparel\MihoPhase2\WineFoxCosplay' 'MihoPhase2_WineFoxCosplay' -IncludeItem
Add-ThreeViewMapping '衣服' '米塔帽子' 'Textures\apparel\MihoPhase2\MitaCap' 'MihoPhase2_MitaCap'
Add-TextureMapping '衣服\面具 侧.png' 'Textures\apparel\MihoPhase2\FoxMask\MihoPhase2_FoxMask_east.png'
Add-TextureMapping '衣服\面具 正.png' 'Textures\apparel\MihoPhase2\FoxMask\MihoPhase2_FoxMask_south.png'
Add-ThreeViewMapping '衣服' '研究服 男' 'Textures\apparel\MihoPhase2\ResearchCoat' 'MihoPhase2_ResearchCoat'
Add-TextureMapping '衣服\研究服 男 侧 后.png' 'Textures\apparel\MihoPhase2\ResearchCoat\MihoPhase2_ResearchCoatBack_east.png'
Add-ThreeViewMapping '衣服' '研究服 普通' 'Textures\apparel\MihoPhase2\ResearchCoat' 'MihoPhase2_ResearchCoat_Female'
Add-TextureMapping '衣服\研究服 普通 侧 后.png' 'Textures\apparel\MihoPhase2\ResearchCoat\MihoPhase2_ResearchCoatBack_Female_east.png'
Add-TextureMapping '衣服\研究服 物品.png' 'Textures\apparel\MihoPhase2\ResearchCoat\MihoPhase2_ResearchCoat.png'

#赫尔默·佐格身体、耳尾、专属服装及动态表情，共59张。
Add-ThreeViewMapping '保镖' '体型 壮' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Body' 'MihoPhase2_HelmerZogBody'
Add-ThreeViewMapping '保镖' '头发' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Hair' 'MihoPhase2_HelmerZogHair'
Add-ThreeViewMapping '保镖' '黑尾巴' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Tail' 'MihoPhase2_HelmerZogTail'
Add-TextureMapping '保镖\耳朵 正.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Ear\MihoPhase2_HelmerZogEar_south.png'
Add-TextureMapping '保镖\耳朵 背.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Ear\MihoPhase2_HelmerZogEar_north.png'
Add-TextureMapping '保镖\耳朵 侧 右.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Ear\MihoPhase2_HelmerZogEar_east.png'
Add-TextureMapping '保镖\耳朵 侧 左.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\Ear\MihoPhase2_HelmerZogEar_west.png'
Add-TextureMapping '保镖\耳朵 侧 右 后.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\EarBack\MihoPhase2_HelmerZogEarBack_east.png'
Add-TextureMapping '保镖\耳朵 侧 左 后.png' 'Textures\AlienRace\adfqs\MihoPhase2\HelmerZog\EarBack\MihoPhase2_HelmerZogEarBack_west.png'
Add-ThreeViewMapping '保镖' '常服' 'Textures\apparel\MihoPhase2\HelmerZogCasual' 'MihoPhase2_HelmerZogCasual' -IncludeItem
Add-ThreeViewMapping '保镖' '西装' 'Textures\apparel\MihoPhase2\HelmerZogSuit' 'MihoPhase2_HelmerZogSuit' -IncludeItem

$HelmerMouth = 'Textures\FacialAnimation\MihoPhase2\HelmerZog\Mouth\Unisex'
Add-HelmerFaceMapping '保镖\动态表情\嘴' '开心' $HelmerMouth 'MihoPhase2_HelmerZogMouth_Happy'
Add-HelmerFaceMapping '保镖\动态表情\嘴' '张嘴' $HelmerMouth 'MihoPhase2_HelmerZogMouth_Open'
Add-HelmerFaceMapping '保镖\动态表情\嘴' '闭嘴' $HelmerMouth 'MihoPhase2_HelmerZogMouth_Closed'

$HelmerBrows = 'Textures\FacialAnimation\MihoPhase2\HelmerZog\Brows\Unisex'
Add-HelmerFaceMapping '保镖\动态表情\眉毛' '普通' $HelmerBrows 'MihoPhase2_HelmerZogBrow_Normal'
Add-HelmerFaceMapping '保镖\动态表情\眉毛' '开心' $HelmerBrows 'MihoPhase2_HelmerZogBrow_Happy'
Add-HelmerFaceMapping '保镖\动态表情\眉毛' '悲伤' $HelmerBrows 'MihoPhase2_HelmerZogBrow_Sad'
Add-HelmerFaceMapping '保镖\动态表情\眉毛' '愤怒' $HelmerBrows 'MihoPhase2_HelmerZogBrow_Angry'

$HelmerLids = 'Textures\FacialAnimation\MihoPhase2\HelmerZog\Lids\Unisex'
Add-HelmerFaceMapping '保镖\动态表情\眼皮' '睁眼' $HelmerLids 'MihoPhase2_HelmerZogLid_Open_cover'
Add-HelmerFaceMapping '保镖\动态表情\眼皮' '闭眼' $HelmerLids 'MihoPhase2_HelmerZogLid_Closed_cover'
Add-HelmerFaceMapping '保镖\动态表情\瞳孔' '眼白' $HelmerLids 'MihoPhase2_HelmerZogLid_Open_bottom'

$HelmerEyes = 'Textures\FacialAnimation\MihoPhase2\HelmerZog\Eyes\Unisex'
Add-HelmerFaceMapping '保镖\动态表情\瞳孔' '瞳孔' $HelmerEyes 'MihoPhase2_HelmerZogEye_Normal'
Add-HelmerFaceMapping '保镖\动态表情\瞳孔' '瞳孔高光' $HelmerEyes 'MihoPhase2_HelmerZogEye_Normal_highlight'

#提亚娜固定外观、专属服装及动态表情，共44张。
Add-TextureMapping '独特米\光环 正.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Halo\MihoPhase2_TianaHalo_south.png'
Add-TextureMapping '独特米\光环 侧.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Halo\MihoPhase2_TianaHalo_east.png'
Add-TextureMapping '独特米\固定脸 正 睁眼.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Face\MihoPhase2_TianaFace_Open_south.png'
Add-TextureMapping '独特米\固定脸 正 闭眼.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Face\MihoPhase2_TianaFace_Closed_south.png'
Add-TextureMapping '独特米\固定脸 侧 睁眼.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Face\MihoPhase2_TianaFace_Open_east.png'
Add-TextureMapping '独特米\固定脸 侧 闭眼.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Face\MihoPhase2_TianaFace_Closed_east.png'
Add-TextureMapping '独特米\头发 正.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Hair\MihoPhase2_TianaHair_south.png'
Add-TextureMapping '独特米\头发 侧.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Hair\MihoPhase2_TianaHair_east.png'
Add-TextureMapping '独特米\头发 背.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\Hair\MihoPhase2_TianaHair_north.png'
Add-TextureMapping '独特米\头发 正 后.png' 'Textures\SpecialPawn\MihoPhase2\Tiana\HairBack\MihoPhase2_TianaHairBack_south.png'
Add-ThreeViewMapping '独特米' '哥特' 'Textures\apparel\MihoPhase2\TianaChant' 'MihoPhase2_TianaChant' -IncludeItem
Add-ThreeViewMapping '独特米' '女仆' 'Textures\apparel\MihoPhase2\TianaMaidDress' 'MihoPhase2_TianaMaidDress' -IncludeItem

$TianaMouth = 'Textures\FacialAnimation\MihoPhase2\Tiana\Mouth\Unisex'
Add-TianaFaceMapping '独特米\动态表情\嘴' '普通' $TianaMouth 'MihoPhase2_TianaMouth_Normal'
Add-TianaFaceMapping '独特米\动态表情\嘴' '闭嘴' $TianaMouth 'MihoPhase2_TianaMouth_Closed'
Add-TianaFaceMapping '独特米\动态表情\嘴' '张嘴' $TianaMouth 'MihoPhase2_TianaMouth_Open'
Add-TianaFaceMapping '独特米\动态表情\嘴' '生气' $TianaMouth 'MihoPhase2_TianaMouth_Angry'

$TianaBrows = 'Textures\FacialAnimation\MihoPhase2\Tiana\Brows\Unisex'
Add-TianaFaceMapping '独特米\动态表情\眉毛' '普通' $TianaBrows 'MihoPhase2_TianaBrow_Normal'
Add-TianaFaceMapping '独特米\动态表情\眉毛' '开心' $TianaBrows 'MihoPhase2_TianaBrow_Happy'
Add-TianaFaceMapping '独特米\动态表情\眉毛' '悲伤' $TianaBrows 'MihoPhase2_TianaBrow_Sad'
Add-TianaFaceMapping '独特米\动态表情\眉毛' '生气' $TianaBrows 'MihoPhase2_TianaBrow_Angry'

$TianaLids = 'Textures\FacialAnimation\MihoPhase2\Tiana\Lids\Unisex'
Add-TianaFaceMapping '独特米\动态表情\眼皮' '睁眼' $TianaLids 'MihoPhase2_TianaLid_Open_cover'
Add-TianaFaceMapping '独特米\动态表情\眼皮' '闭眼' $TianaLids 'MihoPhase2_TianaLid_Closed_cover'
Add-TianaFaceMapping '独特米\动态表情\瞳孔' '眼白' $TianaLids 'MihoPhase2_TianaLid_Open_bottom'

$TianaEyes = 'Textures\FacialAnimation\MihoPhase2\Tiana\Eyes\Unisex'
Add-TianaFaceMapping '独特米\动态表情\瞳孔' '瞳孔' $TianaEyes 'MihoPhase2_TianaEye_Normal'
Add-TianaFaceMapping '独特米\动态表情\瞳孔' '瞳孔高光' $TianaEyes 'MihoPhase2_TianaEye_Normal_highlight'

Add-TextureMapping '武器\emp.png' 'Textures\Weapon\MihoPhase2\MihoPhase2_EMP.png'
Add-ThreeViewMapping '衣服' '钢盔2' 'Textures\apparel\MihoPhase2\DefenseForceHelmet' 'MihoPhase2_DefenseForceHelmet'

#核对映射自身完整且不会把两张源图写入同一目标。
if ($Mappings.Count -ne 245) {
    throw "二期贴图映射数量应为245，当前为$($Mappings.Count)。"
}

$DuplicateSources = $Mappings | Group-Object Source | Where-Object Count -gt 1
if ($DuplicateSources) {
    throw "存在重复源路径：$($DuplicateSources.Name -join ', ')"
}

$DuplicateTargets = $Mappings | Group-Object Target | Where-Object Count -gt 1
if ($DuplicateTargets) {
    throw "存在重复目标路径：$($DuplicateTargets.Name -join ', ')"
}

$NonAsciiTargets = $Mappings | Where-Object { $_.Target -match '[^\x00-\x7F]' }
if ($NonAsciiTargets) {
    throw "目标路径必须全部使用ASCII字符：$($NonAsciiTargets.Target -join ', ')"
}

if ($ListOnly) {
    $Mappings | Select-Object Source, Target
    return
}

$ResolvedSourceRoot = (Resolve-Path -LiteralPath $SourceRoot).Path
$ResolvedModRoot = (Resolve-Path -LiteralPath $ModRoot).Path
$SourceFiles = Get-ChildItem -LiteralPath $ResolvedSourceRoot -Recurse -File -Filter '*.png'
if ($SourceFiles.Count -ne 245) {
    throw "源目录PNG数量应为245，当前为$($SourceFiles.Count)。"
}

$SourceHashesBefore = @{}
foreach ($Mapping in $Mappings) {
    $SourcePath = Join-Path $ResolvedSourceRoot $Mapping.Source
    if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
        throw "源贴图不存在：$SourcePath"
    }

    $SourceHashesBefore[$Mapping.Source] = (Get-FileHash -LiteralPath $SourcePath -Algorithm SHA256).Hash
}

if (-not $VerifyOnly) {
    foreach ($Mapping in $Mappings) {
        $SourcePath = Join-Path $ResolvedSourceRoot $Mapping.Source
        $TargetPath = Join-Path $ResolvedModRoot $Mapping.Target
        $TargetDirectory = Split-Path -Parent $TargetPath
        [System.IO.Directory]::CreateDirectory($TargetDirectory) | Out-Null
        Copy-Item -LiteralPath $SourcePath -Destination $TargetPath -Force
    }
}

Add-Type -AssemblyName System.Drawing
$VerifiedCount = 0
foreach ($Mapping in $Mappings) {
    $SourcePath = Join-Path $ResolvedSourceRoot $Mapping.Source
    $TargetPath = Join-Path $ResolvedModRoot $Mapping.Target
    if (-not (Test-Path -LiteralPath $TargetPath -PathType Leaf)) {
        throw "目标贴图不存在：$TargetPath"
    }

    $SourceHashAfter = (Get-FileHash -LiteralPath $SourcePath -Algorithm SHA256).Hash
    $TargetHash = (Get-FileHash -LiteralPath $TargetPath -Algorithm SHA256).Hash
    if ($SourceHashesBefore[$Mapping.Source] -ne $SourceHashAfter) {
        throw "复制过程中源贴图发生变化：$SourcePath"
    }
    if ($SourceHashAfter -ne $TargetHash) {
        throw "源与目标SHA-256不一致：$TargetPath"
    }

    $SourceImage = [System.Drawing.Image]::FromFile($SourcePath)
    $TargetImage = [System.Drawing.Image]::FromFile($TargetPath)
    try {
        if ($SourceImage.Width -ne $TargetImage.Width -or $SourceImage.Height -ne $TargetImage.Height) {
            throw "源与目标尺寸不一致：$TargetPath"
        }
        if ($TargetImage.Width -ne 512 -or $TargetImage.Height -ne 512) {
            throw "目标贴图不是512x512：$TargetPath"
        }
    }
    finally {
        $SourceImage.Dispose()
        $TargetImage.Dispose()
    }

    $VerifiedCount++
}

Write-Output "二期贴图映射：$($Mappings.Count)"
Write-Output "源目录PNG：$($SourceFiles.Count)"
Write-Output "SHA-256与尺寸验证通过：$VerifiedCount"
