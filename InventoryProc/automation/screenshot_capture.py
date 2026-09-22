"""
Automated Screenshot Capture for Inventory Management System
Captures screenshots of all pages to verify UI/CSS updates
"""

import os
import time
from datetime import datetime
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, NoSuchElementException
from pathlib import Path

class InventoryScreenshotCapture:
    def __init__(self, base_url="http://localhost:4200", output_dir="screenshots"):
        """
        Initialize the screenshot capture automation

        Args:
            base_url: Base URL of the Angular application
            output_dir: Directory to save screenshots
        """
        self.base_url = base_url
        self.output_dir = Path(output_dir)
        self.timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.screenshot_dir = self.output_dir / self.timestamp
        self.screenshot_dir.mkdir(parents=True, exist_ok=True)

        # Initialize Chrome driver with options
        options = webdriver.ChromeOptions()
        options.add_argument('--start-maximized')
        options.add_argument('--disable-blink-features=AutomationControlled')
        # Uncomment for headless mode
        # options.add_argument('--headless')

        self.driver = webdriver.Chrome(options=options)
        self.driver.set_window_size(1920, 1080)
        self.wait = WebDriverWait(self.driver, 10)

        print(f"Screenshots will be saved to: {self.screenshot_dir}")

    def login(self, username="admin", password="admin"):
        """
        Login to the application

        Args:
            username: Login username
            password: Login password
        """
        try:
            print(f"Navigating to login page: {self.base_url}/login")
            self.driver.get(f"{self.base_url}/login")

            # Wait for login form
            self.wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, "input[type='text'], input[name='username']")))

            # Find and fill login form
            username_field = self.driver.find_element(By.CSS_SELECTOR, "input[type='text'], input[name='username']")
            password_field = self.driver.find_element(By.CSS_SELECTOR, "input[type='password'], input[name='password']")

            username_field.clear()
            username_field.send_keys(username)
            password_field.clear()
            password_field.send_keys(password)

            # Submit login
            login_button = self.driver.find_element(By.CSS_SELECTOR, "button[type='submit']")
            login_button.click()

            # Wait for redirect to dashboard
            time.sleep(2)
            print("Login successful")

        except Exception as e:
            print(f"Login failed: {str(e)}")
            print("You may need to login manually. Waiting 10 seconds...")
            time.sleep(10)

    def capture_screenshot(self, page_name, url_path="", wait_selector=None, wait_time=2):
        """
        Capture screenshot of a specific page

        Args:
            page_name: Descriptive name for the screenshot
            url_path: URL path to navigate to (appended to base_url)
            wait_selector: CSS selector to wait for before capturing
            wait_time: Additional wait time in seconds
        """
        try:
            full_url = f"{self.base_url}{url_path}" if url_path else self.driver.current_url
            print(f"\nCapturing: {page_name}")
            print(f"  URL: {full_url}")

            if url_path:
                self.driver.get(full_url)

            # Wait for specific element or default wait
            if wait_selector:
                try:
                    self.wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, wait_selector)))
                except TimeoutException:
                    print(f"  Warning: Selector '{wait_selector}' not found, capturing anyway")

            # Additional wait for Angular to render
            time.sleep(wait_time)

            # Scroll to top
            self.driver.execute_script("window.scrollTo(0, 0);")
            time.sleep(0.5)

            # Capture screenshot
            filename = f"{page_name.lower().replace(' ', '_').replace('/', '_')}.png"
            filepath = self.screenshot_dir / filename
            self.driver.save_screenshot(str(filepath))
            print(f"  ✓ Saved: {filename}")

            return True

        except Exception as e:
            print(f"  ✗ Failed to capture {page_name}: {str(e)}")
            return False

    def capture_all_pages(self):
        """
        Capture screenshots of all pages with updated CSS
        """
        print("\n" + "="*70)
        print("STARTING SCREENSHOT CAPTURE")
        print("="*70)

        results = {
            "successful": [],
            "failed": []
        }

        # Define all pages to capture
        pages = [
            # Dashboard
            {
                "name": "01_Dashboard",
                "url": "/dashboard",
                "selector": ".dashboard-container, .container-fluid"
            },

            # PURCHASES MODULE
            {
                "name": "02_Purchases_Vendors_List",
                "url": "/purchases/vendors",
                "selector": ".vendor-list-container, .container-fluid"
            },
            {
                "name": "03_Purchases_Vendors_Add",
                "url": "/purchases/vendors/add",
                "selector": ".vendor-form-container, .container-fluid"
            },
            {
                "name": "04_Purchases_Orders_List",
                "url": "/purchases/orders",
                "selector": ".po-list-container, .container-fluid"
            },
            {
                "name": "05_Purchases_Orders_New",
                "url": "/purchases/orders/new",
                "selector": ".po-form-container, .container-fluid"
            },
            {
                "name": "06_Purchases_GRN_List",
                "url": "/purchases/grn",
                "selector": ".grn-list-container, .container-fluid"
            },
            {
                "name": "07_Purchases_GRN_New",
                "url": "/purchases/grn/new",
                "selector": ".grn-form-container, .container-fluid"
            },

            # REPORTS MODULE
            {
                "name": "08_Reports_Sales",
                "url": "/reports/sales",
                "selector": ".sales-report-container, .container-fluid"
            },
            {
                "name": "09_Reports_Purchase",
                "url": "/reports/purchase",
                "selector": ".purchase-report-container, .container-fluid"
            },
            {
                "name": "10_Reports_Inventory",
                "url": "/reports/inventory",
                "selector": ".inventory-report-container, .container-fluid"
            },

            # USERS MODULE
            {
                "name": "11_Users_List",
                "url": "/users",
                "selector": ".user-list-container, .container-fluid"
            },
            {
                "name": "12_Users_Add",
                "url": "/users/add",
                "selector": ".user-form-container, .container-fluid"
            },
            {
                "name": "13_Users_Profile",
                "url": "/users/profile",
                "selector": ".user-profile-container, .container-fluid"
            },
            {
                "name": "14_Users_Activity",
                "url": "/users/activity",
                "selector": ".user-activity-container, .container-fluid"
            },
            {
                "name": "15_Users_Activity_Logs",
                "url": "/users/activity-logs",
                "selector": ".activity-logs-container, .container-fluid"
            },

            # PRODUCTS (Reference)
            {
                "name": "00_Products_List_REFERENCE",
                "url": "/products",
                "selector": ".products-container, .container-fluid"
            },
        ]

        # Capture each page
        for page in pages:
            success = self.capture_screenshot(
                page_name=page["name"],
                url_path=page["url"],
                wait_selector=page.get("selector"),
                wait_time=page.get("wait_time", 2)
            )

            if success:
                results["successful"].append(page["name"])
            else:
                results["failed"].append(page["name"])

        return results

    def generate_report(self, results):
        """
        Generate a summary report of the screenshot capture session
        """
        report_path = self.screenshot_dir / "REPORT.txt"

        report = f"""
SCREENSHOT CAPTURE REPORT
Generated: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}
================================================================================

SUMMARY
-------
Total Pages: {len(results['successful']) + len(results['failed'])}
Successful: {len(results['successful'])}
Failed: {len(results['failed'])}

SUCCESSFUL CAPTURES ({len(results['successful'])})
----------------------------------------------------
"""
        for page in results["successful"]:
            report += f"✓ {page}\n"

        if results["failed"]:
            report += f"\nFAILED CAPTURES ({len(results['failed'])})\n"
            report += "-" * 50 + "\n"
            for page in results["failed"]:
                report += f"✗ {page}\n"

        report += f"""
================================================================================
NEXT STEPS
----------
1. Review screenshots in: {self.screenshot_dir}
2. Compare with Products page reference (00_Products_List_REFERENCE.png)
3. Verify gradient headers, rounded corners, and consistent styling
4. Check responsive behavior by resizing browser window
5. Update CSS if any inconsistencies are found

SCREENSHOTS LOCATION
--------------------
{self.screenshot_dir.absolute()}
"""

        with open(report_path, 'w') as f:
            f.write(report)

        print("\n" + "="*70)
        print(report)
        print("="*70)

        return report_path

    def close(self):
        """Close the browser"""
        if self.driver:
            self.driver.quit()
            print("\nBrowser closed")


def main():
    """Main execution function"""
    import argparse

    parser = argparse.ArgumentParser(description='Capture screenshots of Inventory Management System pages')
    parser.add_argument('--url', default='http://localhost:4200', help='Base URL of the application')
    parser.add_argument('--output', default='screenshots', help='Output directory for screenshots')
    parser.add_argument('--username', default='admin', help='Login username')
    parser.add_argument('--password', default='admin', help='Login password')
    parser.add_argument('--no-login', action='store_true', help='Skip login step')

    args = parser.parse_args()

    capture = None
    try:
        # Initialize
        capture = InventoryScreenshotCapture(
            base_url=args.url,
            output_dir=args.output
        )

        # Login if needed
        if not args.no_login:
            capture.login(username=args.username, password=args.password)
        else:
            print("Skipping login - navigate manually if needed")
            print("Waiting 10 seconds...")
            time.sleep(10)

        # Capture all pages
        results = capture.capture_all_pages()

        # Generate report
        report_path = capture.generate_report(results)
        print(f"\n✓ Report saved to: {report_path}")

    except KeyboardInterrupt:
        print("\n\nCapture interrupted by user")
    except Exception as e:
        print(f"\n\nError: {str(e)}")
        import traceback
        traceback.print_exc()
    finally:
        if capture:
            capture.close()


if __name__ == "__main__":
    main()
