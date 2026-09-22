# Inventory Management System - Automation Testing

This directory contains automation scripts for testing and capturing screenshots of the Inventory Management System UI.

## Purpose

- **UI Verification**: Capture screenshots to verify CSS updates and styling consistency
- **Automated Testing**: Can be extended for automated UI testing
- **Visual Regression**: Compare screenshots across different versions
- **Documentation**: Generate visual documentation of the application

## Prerequisites

1. **Python 3.8+** installed
2. **Google Chrome** browser installed
3. **ChromeDriver** (will be installed automatically via webdriver-manager)

## Setup

### 1. Install Python Dependencies

```bash
cd automation
pip install -r requirements.txt
```

Or on Windows:
```cmd
cd automation
pip install -r requirements.txt
```

### 2. Start the Angular Application

Make sure your Angular application is running:

```bash
cd frontend
ng serve
```

The application should be accessible at `http://localhost:4200`

## Usage

### Basic Usage

Capture screenshots of all pages:

```bash
python screenshot_capture.py
```

### With Custom Options

```bash
# Custom URL
python screenshot_capture.py --url http://localhost:4200

# Custom output directory
python screenshot_capture.py --output my_screenshots

# Custom login credentials
python screenshot_capture.py --username admin --password mypassword

# Skip automatic login (for manual login)
python screenshot_capture.py --no-login
```

### Quick Start Scripts

**Windows:**
```cmd
run_screenshot_capture.bat
```

**Linux/Mac:**
```bash
./run_screenshot_capture.sh
```

## Output

Screenshots are saved in the `screenshots/` directory with timestamp:

```
screenshots/
└── 20260917_143022/
    ├── 00_products_list_reference.png
    ├── 01_dashboard.png
    ├── 02_purchases_vendors_list.png
    ├── 03_purchases_vendors_add.png
    ├── ...
    └── REPORT.txt
```

## What Gets Captured

### Purchases Module (6 pages)
- Vendors List
- Add Vendor
- Purchase Orders List
- New Purchase Order
- GRN List
- New GRN

### Reports Module (3 pages)
- Sales Report
- Purchase Report
- Inventory Report

### Users Module (5 pages)
- User List
- Add User
- User Profile
- User Activity
- Activity Logs

### Reference
- Products List (for comparison)

## Troubleshooting

### Chrome Driver Issues

If you get ChromeDriver errors, the script will attempt to download the correct driver automatically. If it fails:

1. Check your Chrome browser version
2. Manually download ChromeDriver from: https://chromedriver.chromium.org/
3. Place it in your system PATH

### Login Issues

If automatic login fails:
1. Use `--no-login` flag
2. Manually login within 10 seconds
3. Script will proceed to capture screenshots

### Page Load Issues

If pages don't load properly:
1. Check that Angular app is running
2. Increase wait times in the script
3. Check browser console for errors

## Customization

### Adding New Pages

Edit `screenshot_capture.py` and add to the `pages` list:

```python
{
    "name": "My_New_Page",
    "url": "/my-route",
    "selector": ".my-container-class",
    "wait_time": 2
}
```

### Changing Browser Options

Modify the Chrome options in `__init__` method:

```python
# For headless mode (no browser window)
options.add_argument('--headless')

# For different window size
self.driver.set_window_size(1366, 768)
```

## Integration with CI/CD

This script can be integrated into your CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Capture Screenshots
  run: |
    cd automation
    pip install -r requirements.txt
    python screenshot_capture.py --no-login
```

## Future Enhancements

- [ ] Visual regression testing with image comparison
- [ ] Mobile responsive screenshot capture
- [ ] PDF report generation
- [ ] Integration with test frameworks (pytest, unittest)
- [ ] Slack/email notifications
- [ ] Docker support

## License

Part of the Inventory Management System project.
