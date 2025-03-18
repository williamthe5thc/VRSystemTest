@echo off
ECHO VR Interview System Git Commit Script
ECHO ===================================

REM Check if message parameter was provided
IF "%~1"=="" (
    ECHO Error: Please provide a commit message.
    ECHO Usage: git_commit.bat "Your commit message"
    EXIT /B 1
)

ECHO Checking git status...
git status

ECHO.
ECHO Adding files to staging...
git add .

ECHO.
ECHO Committing changes...
git commit -m "%~1"

ECHO.
ECHO Commit complete! Run 'git push' when ready to push changes to remote repository.
