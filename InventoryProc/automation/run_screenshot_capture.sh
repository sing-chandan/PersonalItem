#!/bin/bash

echo "================================================================"
echo "Inventory Management System - Screenshot Capture"
echo "================================================================"
echo ""

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 is not installed"
    echo "Please install Python 3.8+ from https://www.python.org/"
    exit 1
fi

# Check if requirements are installed
echo "Checking dependencies..."
if ! python3 -c "import selenium" &> /dev/null; then
    echo "Installing dependencies..."
    pip3 install -r requirements.txt
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to install dependencies"
        exit 1
    fi
fi

echo ""
echo "Starting screenshot capture..."
echo "Make sure the Angular application is running at http://localhost:4200"
echo ""
echo "Press Ctrl+C to cancel, or wait 5 seconds to continue..."
sleep 5

# Run the screenshot capture script
python3 screenshot_capture.py --url http://localhost:4200

echo ""
echo "================================================================"
echo "Screenshot capture complete!"
echo "Check the 'screenshots' folder for results"
echo "================================================================"
