# Testing Implementation Plan - InventoryProc
**Date:** September 22, 2026
**Version:** 1.0
**Status:** Ready to Implement

---

## 📋 Table of Contents

1. [Current Status](#current-status)
2. [Testing Strategy Overview](#testing-strategy-overview)
3. [Backend Unit Tests](#backend-unit-tests)
4. [Frontend Unit Tests](#frontend-unit-tests)
5. [E2E Automation Tests](#e2e-automation-tests)
6. [Test Data Management](#test-data-management)
7. [Coverage Goals](#coverage-goals)
8. [CI/CD Integration](#cicd-integration)
9. [Implementation Timeline](#implementation-timeline)

---

## 1. Current Status

### ✅ What We Have

**Backend:**
- ✅ Test project setup: `InventoryProc.Tests`
- ✅ Testing frameworks installed:
  - xUnit 2.9.2
  - Moq 4.20.72
  - FluentAssertions 8.10.0
  - EF Core InMemory 9.0.0
- ✅ Existing tests:
  - `ProductServiceTests.cs` (5 tests) ✅
  - `AuthenticationServiceTests.cs`
  - `CurrentTenantServiceTests.cs`
  - `ProductTests.cs` (Domain)
  - `ResultTests.cs` (Common)
  - `PasswordHashingTests.cs`

**Frontend:**
- ✅ Jasmine/Karma setup (default Angular)
- ✅ Existing tests:
  - `product.service.spec.ts` (5 tests) ✅
  - `product-list.component.spec.ts`
  - `app.spec.ts`

**E2E Tests:**
- ❌ Not set up yet

---

### 📊 Current Coverage

| Category | Backend | Frontend | E2E |
|----------|---------|----------|-----|
| **Unit Tests** | ~10 tests | ~10 tests | 0 tests |
| **Coverage** | ~5% | ~5% | 0% |
| **Status** | ⚠️ Needs expansion | ⚠️ Needs expansion | ❌ Not started |

---

## 2. Testing Strategy Overview

### 2.1 Test Pyramid

```
        /\
       /E2E\         ← 10% (Critical user flows)
      /------\
     /        \
    / Integration\   ← 20% (API + Database)
   /--------------\
  /                \
 /   Unit Tests     \ ← 70% (Services, Components, Logic)
/____________________\
```

### 2.2 Testing Approach

**Backend:**
1. **Unit Tests** - Service layer, domain logic, utilities
2. **Integration Tests** - API endpoints with test database
3. **Performance Tests** - Load testing (optional)

**Frontend:**
1. **Unit Tests** - Components, services, pipes
2. **Integration Tests** - Component interactions
3. **E2E Tests** - User workflows with Playwright

---

## 3. Backend Unit Tests

### 3.1 Test Structure

```
backend/tests/InventoryProc.Tests/
├── Common/
│   ├── ResultTests.cs ✅
│   └── PaginationTests.cs (NEW)
├── Domain/
│   ├── ProductTests.cs ✅
│   ├── CustomerTests.cs (NEW)
│   ├── SalesOrderTests.cs (NEW)
│   ├── InvoiceTests.cs (NEW)
│   └── PurchaseOrderTests.cs (NEW)
├── Services/
│   ├── Products/
│   │   ├── ProductServiceTests.cs ✅
│   │   ├── CategoryServiceTests.cs (NEW)
│   │   └── BrandServiceTests.cs (NEW)
│   ├── Sales/
│   │   ├── CustomerServiceTests.cs (NEW)
│   │   ├── SalesOrderServiceTests.cs (NEW)
│   │   └── InvoiceServiceTests.cs (NEW)
│   ├── Purchases/
│   │   ├── VendorServiceTests.cs (NEW)
│   │   ├── PurchaseOrderServiceTests.cs (NEW)
│   │   └── GRNServiceTests.cs (NEW)
│   ├── Reports/
│   │   └── DashboardServiceTests.cs (NEW)
│   └── Auth/
│       └── AuthenticationServiceTests.cs ✅
├── Security/
│   └── PasswordHashingTests.cs ✅
├── Integration/
│   ├── ProductsControllerTests.cs (NEW)
│   ├── CustomersControllerTests.cs (NEW)
│   └── SalesOrdersControllerTests.cs (NEW)
└── Helpers/
    ├── TestDbContextFactory.cs (NEW)
    └── TestDataBuilder.cs (NEW)
```

### 3.2 Priority Services to Test (High → Low)

#### Phase 1: Critical Business Logic (Week 1-2)
1. **ProductService** ✅ (Expand to 15+ tests)
2. **CustomerService** (15+ tests)
3. **SalesOrderService** (20+ tests - complex workflow)
4. **InvoiceService** (15+ tests)
5. **AuthenticationService** ✅ (Expand)

#### Phase 2: Purchase & Inventory (Week 3)
6. **VendorService** (10+ tests)
7. **PurchaseOrderService** (20+ tests)
8. **GRNService** (15+ tests)
9. **CategoryService** (8+ tests)
10. **BrandService** (8+ tests)

#### Phase 3: Reports & Supporting (Week 4)
11. **DashboardService** (10+ tests)
12. **SalesReportService** (8+ tests)
13. **InventoryReportService** (8+ tests)
14. **UserManagementService** (10+ tests)

### 3.3 Sample Test Template

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace InventoryProc.Tests.Services.Sales;

public class CustomerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _userServiceMock;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly CustomerService _sut; // System Under Test
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public CustomerServiceTests()
    {
        // Arrange - Setup
        _tenantId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _tenantServiceMock = new Mock<ICurrentTenantService>();
        _tenantServiceMock.Setup(x => x.TenantId).Returns(_tenantId);

        _userServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock.Setup(x => x.UserId).Returns(_userId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantServiceMock.Object, _userServiceMock.Object);
        _loggerMock = new Mock<ILogger<CustomerService>>();
        _sut = new CustomerService(_context, _tenantServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenCustomersExist_ShouldReturnCustomers()
    {
        // Arrange
        var customer = new Customer(_tenantId, "CUST001", "Test Customer", "test@example.com")
        {
            CreatedBy = _userId
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCustomersAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().CustomerCode.Should().Be("CUST001");
    }

    [Fact]
    public async Task CreateCustomerAsync_WhenCodeExists_ShouldReturnFailure()
    {
        // Arrange
        var existingCustomer = new Customer(_tenantId, "DUPLICATE", "Existing", "exist@test.com")
        {
            CreatedBy = _userId
        };
        _context.Customers.Add(existingCustomer);
        await _context.SaveChangesAsync();

        var request = new CreateCustomerRequest
        {
            CustomerCode = "DUPLICATE",
            CustomerName = "New Customer",
            Email = "new@test.com"
        };

        // Act
        var result = await _sut.CreateCustomerAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateCustomerAsync_WhenNameInvalid_ShouldReturnFailure(string invalidName)
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            CustomerCode = "CUST001",
            CustomerName = invalidName,
            Email = "test@test.com"
        };

        // Act
        var result = await _sut.CreateCustomerAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### 3.4 Test Categories

**Use Attributes to Organize:**
```csharp
[Trait("Category", "Unit")]
[Trait("Module", "Sales")]
public class CustomerServiceTests { }

[Trait("Category", "Integration")]
[Trait("Module", "API")]
public class CustomersControllerTests { }
```

**Run by Category:**
```bash
# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only Sales module tests
dotnet test --filter "Module=Sales"
```

---

## 4. Frontend Unit Tests

### 4.1 Test Structure

```
frontend/src/app/
├── core/
│   ├── services/
│   │   ├── auth.service.spec.ts (NEW)
│   │   └── role.service.spec.ts (NEW)
│   ├── guards/
│   │   └── auth.guard.spec.ts (NEW)
│   └── interceptors/
│       └── auth.interceptor.spec.ts (NEW)
├── features/
│   ├── products/
│   │   ├── services/
│   │   │   ├── product.service.spec.ts ✅
│   │   │   ├── category.service.spec.ts (NEW)
│   │   │   └── brand.service.spec.ts (NEW)
│   │   └── pages/
│   │       ├── product-list/
│   │       │   └── product-list.component.spec.ts ✅
│   │       └── product-form/
│   │           └── product-form.component.spec.ts (NEW)
│   ├── sales/
│   │   ├── services/
│   │   │   ├── customer.service.spec.ts (NEW)
│   │   │   ├── sales-order.service.spec.ts (NEW)
│   │   │   └── invoice.service.spec.ts (NEW)
│   │   └── pages/
│   │       ├── customer-list.component.spec.ts (NEW)
│   │       ├── customer-form.component.spec.ts (NEW)
│   │       ├── sales-order-list.component.spec.ts (NEW)
│   │       └── invoice-list.component.spec.ts (NEW)
│   ├── purchases/
│   │   ├── services/
│   │   │   └── vendor.service.spec.ts (NEW)
│   │   └── components/
│   │       └── vendor-list.component.spec.ts (NEW)
│   ├── reports/
│   │   └── services/
│   │       └── dashboard.service.spec.ts (NEW)
│   └── dashboard/
│       └── dashboard.component.spec.ts (NEW)
└── shared/
    ├── directives/
    │   └── has-permission.directive.spec.ts (NEW)
    └── layouts/
        └── main-layout.component.spec.ts (NEW)
```

### 4.2 Priority Components/Services (High → Low)

#### Phase 1: Core & Auth (Week 1)
1. **auth.service.spec.ts** (15+ tests)
2. **auth.guard.spec.ts** (8+ tests)
3. **auth.interceptor.spec.ts** (10+ tests)

#### Phase 2: Products & Sales (Week 2)
4. **product.service.spec.ts** ✅ (Expand to 15+ tests)
5. **customer.service.spec.ts** (12+ tests)
6. **sales-order.service.spec.ts** (15+ tests)
7. **product-list.component.spec.ts** ✅ (Expand)
8. **customer-list.component.spec.ts** (12+ tests)

#### Phase 3: Forms & Complex Components (Week 3)
9. **product-form.component.spec.ts** (20+ tests)
10. **customer-form.component.spec.ts** (15+ tests)
11. **sales-order-form.component.spec.ts** (20+ tests)
12. **dashboard.component.spec.ts** (15+ tests)

### 4.3 Sample Test Template

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { CustomerListComponent } from './customer-list.component';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../models/customer.model';

describe('CustomerListComponent', () => {
  let component: CustomerListComponent;
  let fixture: ComponentFixture<CustomerListComponent>;
  let customerService: jasmine.SpyObj<CustomerService>;

  const mockCustomers: Customer[] = [
    {
      id: '1',
      customerCode: 'CUST001',
      customerName: 'Test Customer',
      email: 'test@example.com',
      phone: '1234567890',
      isActive: true
    }
  ];

  beforeEach(async () => {
    const customerServiceSpy = jasmine.createSpyObj('CustomerService', ['getAll', 'delete']);

    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule,
        ReactiveFormsModule,
        CustomerListComponent // Standalone component
      ],
      providers: [
        { provide: CustomerService, useValue: customerServiceSpy }
      ]
    }).compileComponents();

    customerService = TestBed.inject(CustomerService) as jasmine.SpyObj<CustomerService>;
    fixture = TestBed.createComponent(CustomerListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should load customers on init', () => {
      // Arrange
      customerService.getAll.and.returnValue(of({
        success: true,
        message: 'Success',
        data: mockCustomers
      }));

      // Act
      fixture.detectChanges(); // triggers ngOnInit

      // Assert
      expect(customerService.getAll).toHaveBeenCalled();
      expect(component.customers).toEqual(mockCustomers);
      expect(component.loading).toBeFalse();
    });

    it('should handle error when loading customers fails', () => {
      // Arrange
      const errorMessage = 'Failed to load';
      customerService.getAll.and.returnValue(throwError(() => new Error(errorMessage)));

      // Act
      fixture.detectChanges();

      // Assert
      expect(component.loading).toBeFalse();
      expect(component.error).toBeTruthy();
    });
  });

  describe('searchCustomers', () => {
    it('should filter customers based on search term', () => {
      // Arrange
      component.customers = mockCustomers;
      component.filteredCustomers = mockCustomers;

      // Act
      component.searchTerm = 'Test';
      component.searchCustomers();

      // Assert
      expect(component.filteredCustomers.length).toBe(1);
      expect(component.filteredCustomers[0].customerName).toContain('Test');
    });

    it('should return all customers when search term is empty', () => {
      // Arrange
      component.customers = mockCustomers;

      // Act
      component.searchTerm = '';
      component.searchCustomers();

      // Assert
      expect(component.filteredCustomers).toEqual(mockCustomers);
    });
  });

  describe('deleteCustomer', () => {
    it('should delete customer and refresh list', () => {
      // Arrange
      spyOn(window, 'confirm').and.returnValue(true);
      customerService.delete.and.returnValue(of({
        success: true,
        message: 'Deleted',
        data: undefined as any
      }));
      customerService.getAll.and.returnValue(of({
        success: true,
        message: 'Success',
        data: []
      }));

      // Act
      component.deleteCustomer('1');

      // Assert
      expect(customerService.delete).toHaveBeenCalledWith('1');
      expect(customerService.getAll).toHaveBeenCalled();
    });

    it('should not delete if user cancels confirmation', () => {
      // Arrange
      spyOn(window, 'confirm').and.returnValue(false);

      // Act
      component.deleteCustomer('1');

      // Assert
      expect(customerService.delete).not.toHaveBeenCalled();
    });
  });
});
```

### 4.4 Run Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --code-coverage

# Run specific test file
npm test -- --include='**/customer-list.component.spec.ts'
```

---

## 5. E2E Automation Tests

### 5.1 Framework Selection: Playwright

**Why Playwright?**
- ✅ Fast and reliable
- ✅ Cross-browser (Chromium, Firefox, WebKit)
- ✅ Built-in waiting and auto-retry
- ✅ Video recording and screenshots
- ✅ Better than Selenium/Protractor
- ✅ TypeScript support

**Installation:**
```bash
cd frontend
npm install -D @playwright/test
npx playwright install
```

### 5.2 E2E Test Structure

```
frontend/e2e/
├── fixtures/
│   ├── auth.fixture.ts
│   └── test-data.ts
├── pages/
│   ├── login.page.ts
│   ├── dashboard.page.ts
│   ├── products/
│   │   ├── product-list.page.ts
│   │   └── product-form.page.ts
│   ├── sales/
│   │   ├── customer-list.page.ts
│   │   └── sales-order-form.page.ts
│   └── purchases/
│       └── vendor-list.page.ts
├── tests/
│   ├── auth/
│   │   ├── login.spec.ts
│   │   └── logout.spec.ts
│   ├── products/
│   │   ├── product-crud.spec.ts
│   │   ├── product-search.spec.ts
│   │   └── low-stock-alert.spec.ts
│   ├── sales/
│   │   ├── customer-crud.spec.ts
│   │   ├── sales-order-workflow.spec.ts
│   │   └── invoice-payment.spec.ts
│   └── dashboard/
│       └── dashboard-metrics.spec.ts
├── playwright.config.ts
└── global-setup.ts
```

### 5.3 Playwright Configuration

**playwright.config.ts:**
```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e/tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['json', { outputFile: 'test-results.json' }],
    ['junit', { outputFile: 'test-results.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'mobile',
      use: { ...devices['iPhone 13'] },
    },
  ],
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
  },
});
```

### 5.4 Page Object Model Pattern

**Example: login.page.ts**
```typescript
import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.locator('input[formControlName="email"]');
    this.passwordInput = page.locator('input[formControlName="password"]');
    this.loginButton = page.locator('button[type="submit"]');
    this.errorMessage = page.locator('.alert-danger');
  }

  async goto() {
    await this.page.goto('/login');
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }

  async waitForDashboard() {
    await this.page.waitForURL('/dashboard');
  }
}
```

**Example: product-list.page.ts**
```typescript
import { Page, Locator } from '@playwright/test';

export class ProductListPage {
  readonly page: Page;
  readonly addButton: Locator;
  readonly searchInput: Locator;
  readonly productCards: Locator;
  readonly deleteButton: (productCode: string) => Locator;

  constructor(page: Page) {
    this.page = page;
    this.addButton = page.locator('button:has-text("Add Product")');
    this.searchInput = page.locator('input[placeholder*="Search"]');
    this.productCards = page.locator('.product-card');
    this.deleteButton = (productCode: string) =>
      page.locator(`.product-card:has-text("${productCode}") button:has-text("Delete")`);
  }

  async goto() {
    await this.page.goto('/products');
  }

  async search(term: string) {
    await this.searchInput.fill(term);
    await this.page.waitForTimeout(300); // Debounce
  }

  async getProductCount(): Promise<number> {
    return await this.productCards.count();
  }

  async deleteProduct(productCode: string) {
    this.page.on('dialog', dialog => dialog.accept());
    await this.deleteButton(productCode).click();
    await this.page.waitForResponse(resp => resp.url().includes('/products/'));
  }
}
```

### 5.5 Sample E2E Tests

**auth/login.spec.ts:**
```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';

test.describe('Login Flow', () => {
  let loginPage: LoginPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    await loginPage.goto();
  });

  test('should login with valid credentials', async ({ page }) => {
    // Act
    await loginPage.login('admin@test.com', 'Admin@123');

    // Assert
    await expect(page).toHaveURL(/.*dashboard/);
    await expect(page.locator('h2:has-text("Dashboard")')).toBeVisible();
  });

  test('should show error with invalid credentials', async () => {
    // Act
    await loginPage.login('wrong@test.com', 'wrongpass');

    // Assert
    await expect(loginPage.errorMessage).toBeVisible();
    await expect(loginPage.errorMessage).toContainText('Invalid credentials');
  });

  test('should validate required fields', async () => {
    // Act
    await loginPage.loginButton.click();

    // Assert
    await expect(loginPage.emailInput).toHaveAttribute('aria-invalid', 'true');
    await expect(loginPage.passwordInput).toHaveAttribute('aria-invalid', 'true');
  });
});
```

**products/product-crud.spec.ts:**
```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { ProductListPage } from '../pages/products/product-list.page';
import { ProductFormPage } from '../pages/products/product-form.page';

test.describe('Product CRUD Operations', () => {
  let loginPage: LoginPage;
  let productListPage: ProductListPage;
  let productFormPage: ProductFormPage;

  test.beforeEach(async ({ page }) => {
    // Login first
    loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin@test.com', 'Admin@123');
    await loginPage.waitForDashboard();

    // Navigate to products
    productListPage = new ProductListPage(page);
    await productListPage.goto();
  });

  test('should create a new product', async ({ page }) => {
    // Arrange
    const productData = {
      code: `TEST${Date.now()}`,
      name: 'E2E Test Product',
      category: 'Electronics',
      unit: 'PCS',
      purchasePrice: '100',
      salePrice: '150',
      mrp: '200'
    };

    // Act
    await productListPage.addButton.click();

    productFormPage = new ProductFormPage(page);
    await productFormPage.fillForm(productData);
    await productFormPage.submitButton.click();

    // Assert
    await expect(page.locator('.alert-success')).toBeVisible();
    await expect(page).toHaveURL(/.*products$/);

    // Verify product appears in list
    await productListPage.search(productData.code);
    const count = await productListPage.getProductCount();
    expect(count).toBe(1);
  });

  test('should search products', async () => {
    // Act
    await productListPage.search('Laptop');

    // Assert
    const count = await productListPage.getProductCount();
    expect(count).toBeGreaterThan(0);

    // All visible products should contain search term
    const cards = await productListPage.productCards.all();
    for (const card of cards) {
      const text = await card.textContent();
      expect(text?.toLowerCase()).toContain('laptop');
    }
  });

  test('should delete a product', async ({ page }) => {
    // Arrange
    const productCode = 'DELETE-TEST';
    await productListPage.search(productCode);
    const initialCount = await productListPage.getProductCount();

    // Act
    await productListPage.deleteProduct(productCode);

    // Assert
    await page.waitForTimeout(500);
    const newCount = await productListPage.getProductCount();
    expect(newCount).toBe(initialCount - 1);
  });
});
```

**sales/sales-order-workflow.spec.ts:**
```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { SalesOrderFormPage } from '../pages/sales/sales-order-form.page';
import { SalesOrderDetailPage } from '../pages/sales/sales-order-detail.page';

test.describe('Sales Order Workflow', () => {
  test('should create and confirm sales order', async ({ page }) => {
    // Login
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin@test.com', 'Admin@123');

    // Create sales order
    const orderFormPage = new SalesOrderFormPage(page);
    await orderFormPage.goto();
    await orderFormPage.selectCustomer('Test Customer');
    await orderFormPage.addLineItem('Product A', 10, 100);
    await orderFormPage.saveAsDraft();

    // Assert order created
    await expect(page.locator('.alert-success')).toBeVisible();
    const orderId = await page.locator('[data-order-id]').textContent();

    // Navigate to order detail
    const orderDetailPage = new SalesOrderDetailPage(page);
    await orderDetailPage.goto(orderId!);

    // Confirm order
    await orderDetailPage.confirmButton.click();

    // Assert status changed
    await expect(orderDetailPage.statusBadge).toHaveText('Confirmed');
    await expect(orderDetailPage.shipButton).toBeEnabled();
  });
});
```

### 5.6 Critical User Flows to Test

**Priority 1 (Must Have):**
1. ✅ **Login/Logout** - Authentication flow
2. ✅ **Product CRUD** - Create, Read, Update, Delete
3. ✅ **Customer Management** - Add, edit, delete customers
4. ✅ **Sales Order Workflow** - Draft → Confirm → Ship → Deliver
5. ✅ **Invoice Creation** - From sales order
6. ✅ **Payment Recording** - Record payment on invoice

**Priority 2 (Should Have):**
7. Purchase Order creation
8. GRN recording
9. Search and filters
10. Dashboard metrics display
11. Report generation

**Priority 3 (Nice to Have):**
12. User management
13. Role-based access
14. Excel import/export
15. Low stock alerts

### 5.7 Run E2E Tests

```bash
# Run all E2E tests
npx playwright test

# Run in headed mode (see browser)
npx playwright test --headed

# Run specific test
npx playwright test auth/login.spec.ts

# Run in debug mode
npx playwright test --debug

# Run on specific browser
npx playwright test --project=chromium

# Generate HTML report
npx playwright show-report
```

---

## 6. Test Data Management

### 6.1 Backend Test Data

**Create Test Data Builder:**
```csharp
// backend/tests/InventoryProc.Tests/Helpers/TestDataBuilder.cs
public class TestDataBuilder
{
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public TestDataBuilder(Guid tenantId, Guid userId)
    {
        _tenantId = tenantId;
        _userId = userId;
    }

    public Product CreateProduct(
        string code = "TEST001",
        string name = "Test Product",
        decimal purchasePrice = 100,
        decimal salePrice = 150)
    {
        return new Product(
            _tenantId,
            name,
            code,
            "PCS",
            purchasePrice,
            salePrice,
            salePrice * 1.2m,
            Guid.NewGuid())
        {
            CreatedBy = _userId,
            CurrentStock = 50
        };
    }

    public Customer CreateCustomer(
        string code = "CUST001",
        string name = "Test Customer")
    {
        return new Customer(
            _tenantId,
            code,
            name,
            $"{code.ToLower()}@test.com")
        {
            CreatedBy = _userId,
            Phone = "1234567890",
            CreditLimit = 100000
        };
    }

    // Add more builders...
}
```

### 6.2 Frontend Test Data

**Create Mock Data:**
```typescript
// frontend/e2e/fixtures/test-data.ts
export const testUsers = {
  admin: {
    email: 'admin@test.com',
    password: 'Admin@123',
    role: 'Admin'
  },
  manager: {
    email: 'manager@test.com',
    password: 'Manager@123',
    role: 'Manager'
  }
};

export const testProducts = [
  {
    code: 'PROD001',
    name: 'Test Product 1',
    category: 'Electronics',
    unit: 'PCS',
    purchasePrice: 100,
    salePrice: 150,
    mrp: 200
  }
];

export const testCustomers = [
  {
    code: 'CUST001',
    name: 'Test Customer',
    email: 'customer@test.com',
    phone: '1234567890'
  }
];
```

### 6.3 Database Seeding for Tests

**Create Seed Script:**
```bash
# backend/tests/seed-test-data.sql
-- Create test tenant
INSERT INTO Tenants (Id, TenantName, ContactEmail, IsActive)
VALUES ('11111111-1111-1111-1111-111111111111', 'Test Tenant', 'test@test.com', 1);

-- Create test user
INSERT INTO Users (Id, TenantId, Email, FirstName, LastName, PasswordHash, Role, IsActive)
VALUES ('22222222-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'admin@test.com', 'Test', 'Admin', 'hash', 'Admin', 1);

-- Create test products
INSERT INTO Products (Id, TenantId, Code, Name, Unit, PurchasePrice, SalePrice, MRP, CategoryId, CreatedBy)
VALUES ('33333333-3333-3333-3333-333333333333',
        '11111111-1111-1111-1111-111111111111',
        'TEST001', 'Test Product', 'PCS', 100, 150, 200, NULL,
        '22222222-2222-2222-2222-222222222222');
```

---

## 7. Coverage Goals

### 7.1 Target Coverage

| Category | Target | Current | Gap |
|----------|--------|---------|-----|
| **Backend Unit** | 80% | 5% | 75% ⚠️ |
| **Backend Integration** | 70% | 0% | 70% ⚠️ |
| **Frontend Unit** | 70% | 5% | 65% ⚠️ |
| **E2E Critical Flows** | 100% | 0% | 100% ⚠️ |
| **Overall** | 75% | 3% | 72% ⚠️ |

### 7.2 Generate Coverage Reports

**Backend:**
```bash
cd backend/tests/InventoryProc.Tests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

**Frontend:**
```bash
cd frontend
npm test -- --code-coverage
# Report at: frontend/coverage/index.html
```

### 7.3 Coverage Enforcement

**Add to CI/CD pipeline:**
```yaml
# Fail build if coverage drops below threshold
- name: Check Coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
    if [ $(grep -oP '(?<=line-rate=")[^"]+' coverage/*/coverage.cobertura.xml | awk '{s+=$1}END{print s/NR}') -lt 0.80 ]; then
      echo "Coverage below 80%"
      exit 1
    fi
```

---

## 8. CI/CD Integration

### 8.1 GitHub Actions Workflow

**.github/workflows/test.yml:**
```yaml
name: Run Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./backend

      - name: Build
        run: dotnet build --no-restore
        working-directory: ./backend

      - name: Run Unit Tests
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
        working-directory: ./backend/tests/InventoryProc.Tests

      - name: Upload Coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./backend/tests/InventoryProc.Tests/coverage.cobertura.xml

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci
        working-directory: ./frontend

      - name: Run Unit Tests
        run: npm test -- --code-coverage --watch=false
        working-directory: ./frontend

      - name: Upload Coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./frontend/coverage/lcov.info

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - name: Install dependencies
        run: npm ci
        working-directory: ./frontend

      - name: Install Playwright
        run: npx playwright install --with-deps
        working-directory: ./frontend

      - name: Start Backend
        run: |
          cd backend/src/Host/InventoryProc.API
          dotnet run &
          sleep 10

      - name: Run E2E Tests
        run: npx playwright test
        working-directory: ./frontend

      - name: Upload Playwright Report
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: playwright-report
          path: frontend/playwright-report/
```

---

## 9. Implementation Timeline

### Week 1: Backend Unit Tests - Core (40 hours)
**Day 1-2:** Setup & Helpers
- ✅ Review existing tests
- Create TestDataBuilder
- Create TestDbContextFactory
- Document testing guidelines

**Day 3-5:** Critical Services
- ProductService (15 tests)
- CustomerService (15 tests)
- AuthenticationService (expand to 10 tests)

**Deliverable:** 40+ backend unit tests

---

### Week 2: Backend Unit Tests - Sales & Purchases (40 hours)
**Day 1-2:** Sales Module
- SalesOrderService (20 tests)
- InvoiceService (15 tests)

**Day 3-5:** Purchases Module
- VendorService (10 tests)
- PurchaseOrderService (20 tests)
- GRNService (15 tests)

**Deliverable:** 80+ additional backend tests

---

### Week 3: Frontend Unit Tests (40 hours)
**Day 1-2:** Core & Services
- auth.service.spec.ts (15 tests)
- auth.guard.spec.ts (8 tests)
- product.service.spec.ts (expand to 15 tests)
- customer.service.spec.ts (12 tests)

**Day 3-5:** Components
- product-list.component.spec.ts (expand to 15 tests)
- customer-list.component.spec.ts (12 tests)
- dashboard.component.spec.ts (15 tests)
- product-form.component.spec.ts (20 tests)

**Deliverable:** 110+ frontend unit tests

---

### Week 4: E2E Automation Tests (40 hours)
**Day 1:** Setup
- Install Playwright
- Configure playwright.config.ts
- Create page object models
- Setup auth fixture

**Day 2-3:** Critical Flows
- Login/Logout (5 tests)
- Product CRUD (8 tests)
- Customer Management (6 tests)
- Search & Filters (5 tests)

**Day 4-5:** Complex Workflows
- Sales Order Workflow (10 tests)
- Invoice & Payment (8 tests)
- Dashboard Metrics (5 tests)

**Deliverable:** 45+ E2E tests covering critical user flows

---

### Week 5: Integration Tests & Polish (40 hours)
**Day 1-2:** API Integration Tests
- ProductsController (10 tests)
- CustomersController (8 tests)
- SalesOrdersController (10 tests)

**Day 3-4:** Coverage & Reporting
- Generate coverage reports
- Identify gaps
- Write missing tests
- Achieve 75%+ overall coverage

**Day 5:** CI/CD Integration
- Setup GitHub Actions
- Configure code coverage tools
- Add PR quality gates
- Documentation

**Deliverable:** Complete testing infrastructure with CI/CD

---

## 10. Resources & Tools

### 10.1 Backend Tools
- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **EF Core InMemory** - In-memory database
- **Bogus** - Fake data generator (optional)

### 10.2 Frontend Tools
- **Jasmine** - Test framework
- **Karma** - Test runner
- **Playwright** - E2E testing
- **@angular/testing** - Angular testing utilities

### 10.3 CI/CD Tools
- **GitHub Actions** - CI/CD pipeline
- **Codecov** - Coverage reporting
- **SonarCloud** - Code quality (optional)

---

## 11. Success Criteria

**Testing Implementation Complete When:**
- ✅ 200+ backend unit tests written
- ✅ 150+ frontend unit tests written
- ✅ 45+ E2E tests covering critical flows
- ✅ 75%+ overall code coverage
- ✅ CI/CD pipeline runs all tests on PR
- ✅ Coverage reports generated automatically
- ✅ All critical user flows tested end-to-end
- ✅ Test documentation complete

---

## 12. Next Steps

**Immediate Actions:**
1. ✅ Review this plan with team
2. ⏳ Get approval on timeline (5 weeks)
3. ⏳ Assign developers to testing tasks
4. ⏳ Start Week 1: Backend unit tests

**Week 1 Kickoff:**
- Create TestDataBuilder helper class
- Expand ProductServiceTests to 15+ tests
- Create CustomerServiceTests (15+ tests)
- Create AuthenticationServiceTests (expand)
- Aim for 40+ tests by end of week

---

**Document Owner:** Development Team
**Last Updated:** September 22, 2026
**Next Review:** After Week 1 completion

---

**Ready to start? Let's begin with Week 1! 🚀**
