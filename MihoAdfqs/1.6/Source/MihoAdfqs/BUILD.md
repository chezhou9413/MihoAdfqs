# MihoAdfqs 本地开发说明

## 目录职责

- `E:\RimModDev\MihoAdfqs` 是外层开发目录，保存构建和部署入口。
- `E:\RimModDev\MihoAdfqs\MihoAdfqs` 是 RimWorld 实际加载的内层 Mod 目录。
- `MihoAdfqs\1.6\Source\MihoAdfqs\MihoAdfqs.csproj` 负责 C# 源码编译。
- `1.6\Assemblies` 保存编译输出的 `MihoAdfqs.dll` 和调试符号。
- `FacialAnimation` 保存可选兼容目录，由 `LoadFolders.xml` 按已启用 Mod 条件加载。
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

在外层开发目录运行：

```bat
deploy_modSelf.bat
```

默认部署位置：

```text
E:\huanshijie\1.6\RimWorld\Mods\MihoAdfqs
```

## 汇总文件

在内层 Mod 目录运行：

```bat
Abstract.bat
Abstract_New.bat
```