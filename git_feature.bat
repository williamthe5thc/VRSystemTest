@echo off
ECHO VR Interview System Feature Branch Script
ECHO =====================================

REM Check if feature name parameter was provided
IF "%~1"=="" (
    ECHO Error: Please provide a feature name.
    ECHO Usage: git_feature.bat feature-name
    EXIT /B 1
)

REM Convert spaces to hyphens and lowercase
SET feature_name=%~1
SET feature_name=%feature_name: =-%
SET feature_name=%feature_name:"=%

ECHO.
ECHO Creating feature branch: feature/%feature_name%

REM Make sure we're up to date with main branch
ECHO.
ECHO Fetching latest changes...
git fetch

ECHO.
ECHO Checking out main branch...
git checkout main

ECHO.
ECHO Pulling latest changes...
git pull

ECHO.
ECHO Creating new feature branch...
git checkout -b feature/%feature_name%

ECHO.
ECHO Feature branch created successfully!
ECHO You are now working on: feature/%feature_name%
ECHO.
ECHO When you're ready to commit your changes:
ECHO 1. Use git_commit.bat "Your commit message"
ECHO 2. Push your branch with: git push -u origin feature/%feature_name%
ECHO 3. Create a pull request to merge your changes to main
