@echo off
ECHO Unity Git Utilities
ECHO =================

IF "%~1"=="" (
    GOTO :MENU
) ELSE (
    IF "%~1"=="check" GOTO :CHECK_UNITY_SETTINGS
    IF "%~1"=="clean" GOTO :CLEAN_TEMP_FILES
    IF "%~1"=="status" GOTO :CHECK_STATUS
    IF "%~1"=="config" GOTO :SETUP_SMART_MERGE
    GOTO :MENU
)

:MENU
ECHO.
ECHO Choose an option:
ECHO 1. Check Unity version control settings
ECHO 2. Clean temporary Unity files
ECHO 3. Check status of Unity-specific files
ECHO 4. Configure Unity Smart Merge
ECHO 5. Exit
ECHO.
SET /P CHOICE=Enter option (1-5): 

IF "%CHOICE%"=="1" GOTO :CHECK_UNITY_SETTINGS
IF "%CHOICE%"=="2" GOTO :CLEAN_TEMP_FILES
IF "%CHOICE%"=="3" GOTO :CHECK_STATUS
IF "%CHOICE%"=="4" GOTO :SETUP_SMART_MERGE
IF "%CHOICE%"=="5" EXIT /B 0
ECHO Invalid option. Please try again.
GOTO :MENU

:CHECK_UNITY_SETTINGS
ECHO.
ECHO Checking Unity version control settings...
ECHO.
IF EXIST "ProjectSettings\EditorSettings.asset" (
    FINDSTR "m_ExternalVersionControlSupport: Visible Meta Files" "ProjectSettings\EditorSettings.asset" >nul
    IF %ERRORLEVEL% NEQ 0 (
        ECHO WARNING: Unity may not be set to use "Visible Meta Files".
        ECHO Please check Editor Settings and set Version Control Mode to "Visible Meta Files".
    ) ELSE (
        ECHO Version Control Mode: OK (Visible Meta Files)
    )
    
    FINDSTR "m_SerializationMode: 2" "ProjectSettings\EditorSettings.asset" >nul
    IF %ERRORLEVEL% NEQ 0 (
        ECHO WARNING: Unity may not be set to use "Force Text" serialization.
        ECHO Please check Editor Settings and set Asset Serialization Mode to "Force Text".
    ) ELSE (
        ECHO Asset Serialization Mode: OK (Force Text)
    )
) ELSE (
    ECHO Cannot find EditorSettings.asset. Unable to check Unity settings.
)
GOTO :EOF

:CLEAN_TEMP_FILES
ECHO.
ECHO Cleaning temporary Unity files...
ECHO This will not affect your project, only remove files not needed for version control.
ECHO.
SET /P CONFIRM=Are you sure you want to continue? (Y/N): 
IF /I "%CONFIRM%" NEQ "Y" GOTO :EOF

IF EXIST "Temp" (
    ECHO Removing Temp folder...
    RMDIR /S /Q "Temp"
)
IF EXIST "Library" (
    ECHO Removing Library folder...
    RMDIR /S /Q "Library"
)
IF EXIST "Logs" (
    ECHO Removing Logs folder...
    RMDIR /S /Q "Logs"
)
IF EXIST "obj" (
    ECHO Removing obj folder...
    RMDIR /S /Q "obj"
)
ECHO Clean complete.
GOTO :EOF

:CHECK_STATUS
ECHO.
ECHO Checking status of Unity-specific files...
ECHO.
ECHO ===== Meta Files =====
git status --porcelain | FINDSTR "\.meta$"
ECHO.
ECHO ===== Scene Files =====
git status --porcelain | FINDSTR "\.unity$"
ECHO.
ECHO ===== Prefab Files =====
git status --porcelain | FINDSTR "\.prefab$"
ECHO.
ECHO ===== Asset Files =====
git status --porcelain | FINDSTR "\.asset$"
ECHO.
ECHO ===== Other Files =====
git status --short
GOTO :EOF

:SETUP_SMART_MERGE
ECHO.
ECHO Setting up Unity Smart Merge...
ECHO.
SET UNITY_PATH=
FOR /F "tokens=*" %%G IN ('WHERE /R "C:\Program Files" Unity.exe 2^>NUL') DO (
    SET UNITY_PATH=%%G
    GOTO :FOUND_UNITY
)
FOR /F "tokens=*" %%G IN ('WHERE /R "C:\Program Files\Unity\Hub\Editor" Unity.exe 2^>NUL') DO (
    SET UNITY_PATH=%%G
    GOTO :FOUND_UNITY
)
:FOUND_UNITY

IF "%UNITY_PATH%"=="" (
    ECHO Could not find Unity installation automatically.
    ECHO.
    ECHO Please enter the path to your Unity installation directory:
    ECHO (e.g., C:\Program Files\Unity\Hub\Editor\2022.3.0f1)
    SET /P UNITY_INSTALL=Path: 
    SET UNITY_PATH=%UNITY_INSTALL%\Editor\Unity.exe
) ELSE (
    FOR %%F IN ("%UNITY_PATH%") DO SET UNITY_INSTALL=%%~dpF
)

IF EXIST "%UNITY_INSTALL%\Data\Tools\UnityYAMLMerge.exe" (
    ECHO Found UnityYAMLMerge at: %UNITY_INSTALL%\Data\Tools\UnityYAMLMerge.exe
    
    ECHO.
    ECHO Configuring git to use UnityYAMLMerge...
    git config merge.tool unityyamlmerge
    git config mergetool.unityyamlmerge.cmd "\""%UNITY_INSTALL%\Data\Tools\UnityYAMLMerge.exe\"" merge -p "\"$BASE\"" "\"$REMOTE\"" "\"$LOCAL\"" "\"$MERGED\"""
    git config mergetool.unityyamlmerge.trustExitCode false
    git config mergetool.keepBackup false
    
    ECHO.
    ECHO Unity Smart Merge configured successfully!
    ECHO When you have merge conflicts, run: git mergetool
) ELSE (
    ECHO Could not find UnityYAMLMerge.exe in the specified Unity installation.
    ECHO Please make sure you've specified the correct Unity installation path.
)
GOTO :EOF
