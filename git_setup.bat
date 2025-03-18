@echo off
ECHO VR Interview System Git Setup Script
ECHO ==================================

ECHO.
ECHO This script will help you set up the git repository for the VR Interview System Unity client.
ECHO.

REM Check if git is installed
WHERE git >nul 2>nul
IF %ERRORLEVEL% NEQ 0 (
    ECHO Error: Git is not installed or not in the PATH.
    ECHO Please install Git from https://git-scm.com/ and try again.
    EXIT /B 1
)

ECHO Git is installed and available.
ECHO.

REM Check if git-lfs is installed
WHERE git-lfs >nul 2>nul
IF %ERRORLEVEL% NEQ 0 (
    ECHO Warning: Git LFS is not installed or not in the PATH.
    ECHO It's recommended to install Git LFS for handling large files.
    ECHO Visit https://git-lfs.github.com/ for installation instructions.
    ECHO.
    SET /P CONTINUE=Do you want to continue without Git LFS? (Y/N): 
    IF /I "%CONTINUE%" NEQ "Y" EXIT /B 1
) ELSE (
    ECHO Git LFS is installed and available.
    ECHO Initializing Git LFS...
    git lfs install
)

ECHO.
ECHO Please enter your git user information:
SET /P NAME=Your Name: 
SET /P EMAIL=Your Email: 

ECHO.
ECHO Configuring git user...
git config user.name "%NAME%"
git config user.email "%EMAIL%"

ECHO.
ECHO Please enter the URL of your remote repository:
ECHO (e.g., https://github.com/yourusername/vr-interview-system-client.git)
SET /P REMOTE_URL=Remote URL: 

ECHO.
ECHO Adding remote repository...
git remote add origin %REMOTE_URL%

ECHO.
ECHO Verifying remote setup...
git remote -v

ECHO.
ECHO Setup complete!
ECHO.
ECHO Next steps:
ECHO 1. Create an initial commit:
ECHO    git_commit.bat "Initial commit of VR Interview System Unity client"
ECHO 2. Push to remote repository:
ECHO    git push -u origin main
ECHO.
ECHO Refer to GIT_SETUP.md for more detailed instructions.
