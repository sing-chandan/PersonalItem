# InventoryProc Testing Strategy

## Overview
Comprehensive automated testing strategy covering frontend E2E tests, backend unit tests, and integration tests.

## Testing Layers

### 1. Frontend E2E Testing (Playwright)
**Tool**: Playwright
**Purpose**: Test user workflows and UI interactions

#### Test Scenarios:

**Authentication Tests:**
- ✅ User registration with valid data
- ✅ User login with valid credentials
- ✅ Login validation errors
- ✅ Logout functionality
- ✅ Protected route redirection

**Product Management Tests:**
- ✅ Create new product
- ✅ View product list
- ✅ Search/filter products
- ✅ Edit existing product
- ✅ Delete product
- ✅ Excel import/export
- ✅ Category management
- ✅ Brand management
- ✅ Low stock alerts

**Customer Management Tests:**
- ✅ Create new customer
- ✅ View customer list
- ✅ Search customers
- ✅ Edit customer details
- ✅ Delete customer
- ✅ Credit management

**Sales Order Tests:**
- ✅ Create sales order
- ✅ View orders list
- ✅ Order workflow: Draft → Confirmed → Shipped → Delivered
- ✅ Cancel order
- ✅ Apply discount
- ✅ View customer orders

### 2. Backend Unit Tests (xUnit)
**Tool**: xUnit + Moq
**Purpose**: Test business logic and API endpoints

#### Test Coverage:
- Service layer tests (CRUD operations)
- Entity validation tests
- Repository pattern tests
- Workflow state machine tests
- Authentication/Authorization tests
- Data validation tests

### 3. Integration Tests
**Tool**: WebApplicationFactory + xUnit
**Purpose**: Test full stack integration

#### Test Coverage:
- API endpoint integration
- Database operations
- Multi-tenancy isolation
- JWT token validation
- CORS configuration

### 4. API Testing (REST)
**Tool**: Postman/Newman (optional)
**Purpose**: API contract testing

## Test Data Strategy

### Seed Data:
```
Tenant: Test Company (Auto-created on registration)
User: test@example.com / Test@123
Products: 10 sample products
Categories: 5 categories
Brands: 3 brands
Customers: 5 customers
Sales Orders: 3 orders in different states
```

### Test Database:
- Separate test database
- Reset before each test suite
- Automatic cleanup after tests

## Test Execution

### Local Development:
```bash
# Frontend E2E tests
npm run test:e2e

# Backend unit tests
dotnet test

# All tests
npm run test:all
```

### CI/CD Pipeline:
```bash
# Automated on every push
1. Build backend
2. Build frontend
3. Run backend unit tests
4. Start backend server
5. Start frontend server
6. Run E2E tests
7. Generate coverage report
8. Stop servers
```

## Test Reports

### Coverage Targets:
- Backend: 80% code coverage
- Frontend: 70% code coverage
- E2E: All critical user flows

### Report Formats:
- HTML reports for E2E tests
- JUnit XML for CI/CD integration
- Coverage reports (lcov format)

## Test Environment

### Backend:
- API: http://localhost:5005
- Database: SQL Server (test database)
- Test data: Auto-seeded

### Frontend:
- URL: http://localhost:4200
- Test browser: Chromium (headless)
- Viewport: 1920x1080

## Continuous Improvement

### Regular Updates:
- Add tests for new features
- Update tests when requirements change
- Refactor tests to reduce duplication
- Monitor test execution time
- Fix flaky tests immediately

## Test Isolation

### Principles:
- Each test is independent
- No shared state between tests
- Clean database state
- Unique test data per test
- Parallel execution safe
