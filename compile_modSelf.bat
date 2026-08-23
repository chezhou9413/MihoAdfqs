@echo off
chcp 65001 >nul
setlocal EnableExtensions DisableDelayedExpansion

REM 脚本职责：单独编译当前模组，不执行部署。
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Debug"

set "BUILD_SCRIPT=E:\RimModDev\MihoAdfqs\build.bat"
if not exist "%BUILD_SCRIPT%" (
    echo [ERROR] 编译脚本不存在: %BUILD_SCRIPT%
    pause
    exit /b 1
)
call "%BUILD_SCRIPT%" %CONFIGURATION%
set "BUILD_ERROR=%ERRORLEVEL%"
if not "%BUILD_ERROR%"=="0" (
    echo [ERROR] 编译失败，退出码: %BUILD_ERROR%
    pause
    exit /b %BUILD_ERROR%
)
echo [SUCCESS] 编译完成。
pause
exit /b 0
