# Quick Start Guide

## Step-by-Step Instructions

### 1. First Time Setup

```bash
# Navigate to automation directory
cd automation

# Install dependencies
pip install -r requirements.txt
```

### 2. Start Your Application

```bash
# In a separate terminal, start the Angular app
cd frontend
ng serve
```

Wait until you see: `✔ Compiled successfully`

### 3. Run Screenshot Capture

**Option A: Using the script (Windows)**
```cmd
run_screenshot_capture.bat
```

**Option B: Using Python directly**
```bash
python screenshot_capture.py
```

**Option C: With custom settings**
```bash
python screenshot_capture.py --username admin --password yourpassword
```

### 4. View Results

Screenshots will be saved in `screenshots/YYYYMMDD_HHMMSS/`

Open the folder and review:
- `REPORT.txt` - Summary of capture session
- `00_products_list_reference.png` - Your reference design
- All other screenshots for comparison

## Expected Behavior

The script will:
1. Open Chrome browser (you'll see it)
2. Login to the application
3. Navigate to each page
4. Wait 2 seconds for page to load
5. Capture screenshot
6. Move to next page
7. Generate report

**Time**: ~2-3 minutes for all pages

## Common Issues & Solutions

### "chromedriver not found"
**Solution**: The script will auto-download it. If that fails:
```bash
pip install webdriver-manager --upgrade
```

### "Connection refused"
**Solution**: Make sure Angular app is running:
```bash
cd frontend
ng serve
```

### "Login failed"
**Solution**: Use manual login:
```bash
python screenshot_capture.py --no-login
# Then manually login within 10 seconds
```

### Browser opens but nothing happens
**Solution**: Check console output for errors. Try:
```bash
python screenshot_capture.py --url http://localhost:4200
```

## What to Check in Screenshots

After capture, review each screenshot for:

✅ **Gradient Headers**: Purple gradient on h1 titles
✅ **Rounded Corners**: 12px border-radius on cards
✅ **Thick Borders**: 2px borders on form controls
✅ **Button Styling**: Gradient primary buttons
✅ **Consistent Spacing**: Padding and margins
✅ **Card Shadows**: 0 2px 8px rgba shadows

Compare with `00_products_list_reference.png` - all pages should have similar styling!

## Tips

- Run this after making CSS changes to verify updates
- Compare screenshots before/after changes
- Keep successful captures for documentation
- Use timestamp folders to track different versions

## Need Help?

Check the full [README.md](README.md) for detailed documentation.
