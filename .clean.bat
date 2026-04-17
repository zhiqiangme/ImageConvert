@echo off
setlocal enabledelayedexpansion

cd /d "%~dp0"

dir /b "*.slnx" >nul 2>nul
if errorlevel 1 (
    echo 未检测到 .slnx 文件，未执行删除。
    pause
    exit /b 0
)

echo 检测到 .slnx 文件，开始删除 bin 和 obj 文件夹...

for /d /r %%D in (bin obj) do (
    if /i "%%~nxD"=="bin" (
        echo 删除: %%D
        rd /s /q "%%D"
    )
    if /i "%%~nxD"=="obj" (
        echo 删除: %%D
        rd /s /q "%%D"
    )
)

echo 删除完成。