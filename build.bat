@echo off
chcp 65001 > nul
setlocal

REM 脚本职责：编译主程序集及随包枪口库，再编译条件加载的表情兼容程序集。
set "PROJECT_FILE=%~dp0MihoAdfqs\1.6\Source\MihoAdfqs\MihoAdfqs.csproj"
set "PROJECT_DIR=%~dp0MihoAdfqs\1.6\Source\MihoAdfqs"
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Debug"

if not exist "%PROJECT_FILE%" (
    echo 未找到项目文件: %PROJECT_FILE%
    exit /b 1
)

pushd "%PROJECT_DIR%"
echo 正在编译 MihoAdfqs (%CONFIGURATION%)...
msbuild "%PROJECT_FILE%" /p:Configuration=%CONFIGURATION% /p:Platform="AnyCPU"
set "BUILD_EXIT_CODE=%ERRORLEVEL%"
popd

if not "%BUILD_EXIT_CODE%"=="0" (
    echo 编译失败，退出码: %BUILD_EXIT_CODE%
    exit /b %BUILD_EXIT_CODE%
)

msbuild "%~dp0MihoAdfqs\FacialAnimation\Source\MihoAdfqs.FA\MihoAdfqs.FA.csproj" /p:Configuration=%CONFIGURATION% /p:Platform="AnyCPU"
if errorlevel 1 exit /b 1
echo 编译完成。
exit /b 0
