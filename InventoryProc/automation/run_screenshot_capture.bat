@echo off
echo ================================================================
echo Inventory Management System - Screenshot Capture
echo ================================================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ from https://www.python.org/
    pause
    exit /b 1
)

REM Check if requirements are installed
echo Checking dependencies...
pip show selenium >nul 2>&1
if errorlevel 1 (
    echo Installing dependencies...
    pip install -r requirements.txt
    if errorlevel 1 (
        echo ERROR: Failed to install dependencies
        pause
        exit /b 1
    )
)

echo.
echo Starting screenshot capture...
echo Make sure the Angular application is running at http://localhost:4200
echo.
echo Press Ctrl+C to cancel, or wait 5 seconds to continue...
timeout /t 5

REM Run the screenshot capture script
python screenshot_capture.py --url http://localhost:4200

echo.
echo ================================================================
echo Screenshot capture complete!
echo Check the 'screenshots' folder for results
echo ================================================================
pause
