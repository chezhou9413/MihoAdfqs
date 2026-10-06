@echo off
chcp 65001 >nul
setlocal EnableExtensions DisableDelayedExpansion

REM 脚本职责：只部署当前模组，不执行编译。需要编译时请先运行 compile_modSelf.bat。
set "DEPLOY_EXE=E:\aow\ModPack\xmlremove\bin\Release\net8.0-windows\xmlremove.exe"
set "SOURCE_MOD=E:\RimModDev\MihoAdfqs\MihoAdfqs"
set "TARGET_NAME=MihoAdfqs"

if not exist "%DEPLOY_EXE%" (
    echo [ERROR] 统一部署程序不存在: %DEPLOY_EXE%
    pause
    exit /b 1
)

if not exist "%SOURCE_MOD%\About\About.xml" (
    echo [ERROR] 源模组缺少 About.xml: %SOURCE_MOD%
    pause
    exit /b 1
)

REM 枪口库必须随主程序集携带，缺失时停止部署，避免发布不可加载的模组。
if not exist "%SOURCE_MOD%\1.6\Assemblies\WeaponMuzzleFramework.dll" (
    echo [ERROR] 缺少枪口修正库，请先运行 compile_modSelf.bat。
    pause
    exit /b 1
)

"%DEPLOY_EXE%" deploy --source "%SOURCE_MOD%" --target-name "%TARGET_NAME%"
set "DEPLOY_ERROR=%ERRORLEVEL%"

if not "%DEPLOY_ERROR%"=="0" (
    echo [ERROR] 部署失败，退出码: %DEPLOY_ERROR%
    pause
    exit /b %DEPLOY_ERROR%
)

echo [SUCCESS] 部署完成: %TARGET_NAME%
pause
exit /b 0
