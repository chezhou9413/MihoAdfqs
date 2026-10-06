# MihoAdfqs 本地开发说明

## 目录职责

- `E:\RimModDev\MihoAdfqs` 是外层开发目录，保存构建和部署入口。
- `E:\RimModDev\MihoAdfqs\MihoAdfqs` 是 RimWorld 实际加载的内层 Mod 目录。
- `MihoAdfqs\1.6\Source\MihoAdfqs\MihoAdfqs.csproj` 负责 C# 源码编译。
- `1.6\Assemblies` 保存编译输出的 `MihoAdfqs.dll`、调试符号及随包携带的 `WeaponMuzzleFramework.dll`。
- `FacialAnimation` 保存条件加载的表情兼容目录及独立 `MihoAdfqs.FA.dll`。Facial Animation 为可选前置，主程序集没有FA硬引用。
- `Compatibility/Milira` 仅在启用 Milira Race 时加载米莉拉、提亚娜及其种族补丁。
- `Compatibility/MiliraFacialAnimation` 仅在同时启用 FA、Milira Race 和米莉拉 FA 兼容时加载提亚娜动态脸。
- 米莉拉、Ancot Library、FA 及其扩展无需为本模组单独安装；若选择启用米莉拉或其 FA 兼容，仍需满足这些第三方模组自身声明的依赖。
- `deploy_modSelf.bat` 负责把内层 Mod 目录同步到 RimWorld Mods 目录。

## 编译

在外层开发目录运行：

```bat
build.bat
```

需要 Release 构建时运行：

```bat
build.bat Release
```

## 部署

Debug 和 Release 构建均会通过项目引用编译枪口库，并将库 DLL 复制到内层模组的 `1.6/Assemblies`。直接打包内层模组时必须保留该 DLL；不需要玩家额外安装独立枪口库模组。部署脚本会检查库是否存在，缺失时要求先编译。

枪口库源码默认位于 `E:\ModdevMics\Libraries\WeaponMuzzleFramework\Source\WeaponMuzzleFramework\WeaponMuzzleFramework.csproj`，可通过 MSBuild 属性 `WeaponMuzzleFrameworkProject` 指定其他源码位置。构建只携带枪口库，不复制游戏及其他前置 DLL。

在外层开发目录运行：

```bat
deploy_modSelf.bat
```

部署目录由统一部署工具配置决定，当前 Steam 部署位置：

```text
E:\steam\steamapps\common\RimWorld\Mods\MihoAdfqs
```

## 汇总文件

在内层 Mod 目录运行：

```bat
Abstract.bat
Abstract_New.bat
```
