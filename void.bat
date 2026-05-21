@echo off
setlocal

title Void OS Loader
color 0B

set "CONFIG_FILE=%~dp0sys.dat"

if not exist "%CONFIG_FILE%" (
    echo.
    echo ==========================================
    echo   VOID OS - FIRST TIME SETUP
    echo ==========================================
    echo.
    echo Welcome! It looks like you are booting Void OS for the first time.
    echo Please create your administrator credentials below.
    echo.
    set /p "NEW_USER=Create Username: "
    set /p "NEW_PASS=Create Password: "
    
    :: Save credentials to hidden config file
    > "%CONFIG_FILE%" echo %NEW_USER%
    >> "%CONFIG_FILE%" echo %NEW_PASS%
    attrib +h "%CONFIG_FILE%"
    
    echo.
    echo [*] Account created successfully! Please log in.
    echo.
)

:: Read credentials from config
set "SAVED_USER="
set "SAVED_PASS="
for /f "usebackq tokens=*" %%A in ("%CONFIG_FILE%") do (
    if not defined SAVED_USER (
        set "SAVED_USER=%%A"
    ) else if not defined SAVED_PASS (
        set "SAVED_PASS=%%A"
    )
)

echo.
echo ==========================================
echo   VOID OS - BOOT LOADER
echo ==========================================
echo.
set /p "INPUT_USER=Username: "
if not "%INPUT_USER%"=="%SAVED_USER%" (
    color 0C
    echo [!] Access Denied.
    pause
    exit /b 1
)

set /p "INPUT_PASS=Password: "
if not "%INPUT_PASS%"=="%SAVED_PASS%" (
    color 0C
    echo [!] Access Denied.
    pause
    exit /b 1
)

echo [*] Credentials accepted!

:SETUP
echo.
echo Please select a theme for Void OS:
echo [1] Dark Mode
echo [2] Cyber-Punk
echo [3] Classic Linux
set /p "THEME=Choice (1/2/3): "

set "THEME_NAME=Dark"
if "%THEME%"=="1" set "THEME_NAME=Dark"
if "%THEME%"=="2" set "THEME_NAME=Cyber"
if "%THEME%"=="3" set "THEME_NAME=Linux"

echo.
echo [*] Compiling OS Kernel...

set "CS_FILE=%~dp0void_os.cs"
set "EXE_FILE=%~dp0VoidOS.exe"

if not exist "%CS_FILE%" (
    echo [!] Error: Critical kernel source file missing!
    pause
    exit /b 1
)

set "CSC="
for /f "delims=" %%D in ('dir /b /ad /o-d "C:\Windows\Microsoft.NET\Framework64\v4*" 2^>nul') do (
    if exist "C:\Windows\Microsoft.NET\Framework64\%%D\csc.exe" (
        set "CSC=C:\Windows\Microsoft.NET\Framework64\%%D\csc.exe"
        goto :FOUND_CSC
    )
)
for /f "delims=" %%D in ('dir /b /ad /o-d "C:\Windows\Microsoft.NET\Framework\v4*" 2^>nul') do (
    if exist "C:\Windows\Microsoft.NET\Framework\%%D\csc.exe" (
        set "CSC=C:\Windows\Microsoft.NET\Framework\%%D\csc.exe"
        goto :FOUND_CSC
    )
)
:FOUND_CSC

if "%CSC%"=="" (
    echo [!] Error: Could not find C# compiler on this system.
    pause
    exit /b 1
)

:: Compile the C# OS Kernel with custom Icon
"%CSC%" /target:winexe /win32icon:"%~dp0void.ico" /out:"%EXE_FILE%" "%CS_FILE%" >nul 2>&1

if exist "%EXE_FILE%" (
    echo [*] Compilation Successful!
    echo [*] Booting Void OS...
    start /wait "" "%EXE_FILE%" "%THEME_NAME%"
    
    :: Clean up the compiled kernel when the OS shuts down
    del "%EXE_FILE%" >nul 2>&1
) else (
    echo [!] Error compiling OS Kernel.
    pause
)
