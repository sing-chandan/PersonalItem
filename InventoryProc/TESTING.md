# Testing Guide for InventoryProc

## Overview

This project uses comprehensive unit and integration testing to ensure code quality and catch bugs early.

## Backend Testing (.NET)

### Test Framework Stack
- **xUnit**: Test framework
- **Moq**: Mocking library
- **FluentAssertions**: Assertion library
- **EF Core InMemory**: In-memory database for testing

### Running Backend Tests

```bash
# Run all tests
cd backend
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html

# Run specific test project
dotnet test tests/InventoryProc.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProductServiceTests"

# Run tests in watch mode
dotnet watch test
```

### Test Structure

```
backend/tests/InventoryProc.Tests/
├── Services/
│   ├── AuthenticationServiceTests.cs
│   ├── CurrentTenantServiceTests.cs
│   └── ProductServiceTests.cs
├── Domain/
│   └── ProductTests.cs
├── Common/
│   └── ResultTests.cs
└── Security/
    └── PasswordHashingTests.cs
```

### Writing Backend Tests

**Example: Service Test**
```csharp
public class MyServiceTests : IDisposable
{
    private readonly DbContext _context;
    private readonly Mock<IDependency> _dependencyMock;
    private readonly MyService _sut; // System Under Test

    public MyServiceTests()
    {
        // Arrange - Setup test dependencies
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new DbContext(options);
        _dependencyMock = new Mock<IDependency>();
        _sut = new MyService(_context, _dependencyMock.Object);
    }

    [Fact]
    public async Task MethodName_WhenCondition_ShouldExpectedBehavior()
    {
        // Arrange
        var input = new InputData();

        // Act
        var result = await _sut.MethodName(input);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## Frontend Testing (Angular)

### Test Framework Stack
- **Vitest**: Fast unit test runner
- **Testing Library**: Component testing utilities
- **Playwright**: E2E testing

### Running Frontend Tests

```bash
# Run all tests
cd frontend
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Run E2E tests
npm run test:e2e
```

### Test Structure

```
frontend/src/app/
├── features/
│   └── products/
│       ├── pages/
│       │   └── product-list/
│       │       ├── product-list.component.ts
│       │       └── product-list.component.spec.ts
│       └── services/
│           ├── product.service.ts
│           └── product.service.spec.ts
└── core/
    └── services/
        ├── auth.service.ts
        └── auth.service.spec.ts
```

### Writing Frontend Tests

**Example: Component Test**
```typescript
describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let mockService: jest.Mocked<MyService>;

  beforeEach(async () => {
    mockService = {
      getData: jest.fn()
    } as any;

    await TestBed.configureTestingModule({
      imports: [MyComponent],
      providers: [
        { provide: MyService, useValue: mockService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  it('should load data on init', (done) => {
    // Arrange
    const mockData = [{ id: 1, name: 'Test' }];
    mockService.getData.mockReturnValue(of({ success: true, data: mockData }));

    // Act
    component.ngOnInit();

    // Assert
    setTimeout(() => {
      expect(component.data).toEqual(mockData);
      done();
    }, 0);
  });
});
```

**Example: Service Test**
```typescript
describe('MyService', () => {
  let service: MyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MyService]
    });

    service = TestBed.inject(MyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch data from API', (done) => {
    // Arrange
    const mockData = [{ id: 1 }];

    // Act
    service.getData().subscribe(response => {
      // Assert
      expect(response.data).toEqual(mockData);
      done();
    });

    const req = httpMock.expectOne('http://localhost:5005/api/data');
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: mockData });
  });
});
```

## Test Coverage Goals

### Backend
- **Services**: 80%+ coverage
- **Domain Logic**: 90%+ coverage
- **Controllers**: 70%+ coverage
- **DTOs/Models**: 100% coverage

### Frontend
- **Services**: 80%+ coverage
- **Components**: 70%+ coverage
- **Utilities**: 90%+ coverage

## Testing Best Practices

### 1. **AAA Pattern**
Always structure tests using Arrange-Act-Assert:
```csharp
[Fact]
public async Task TestName()
{
    // Arrange - Setup test data and dependencies
    var input = new Input();

    // Act - Execute the method being tested
    var result = await _sut.Method(input);

    // Assert - Verify the outcome
    result.Should().BeTrue();
}
```

### 2. **Test Naming Convention**
Use descriptive names that explain the scenario:
```
MethodName_WhenCondition_ShouldExpectedBehavior
```

Examples:
- `Login_WhenValidCredentials_ShouldReturnToken`
- `GetProducts_WhenTenantIdMismatch_ShouldReturnEmpty`
- `CreateProduct_WhenCodeExists_ShouldReturnFailure`

### 3. **Test One Thing**
Each test should verify one specific behavior.

### 4. **Mock External Dependencies**
- Always mock HTTP calls
- Mock database for unit tests (use InMemory for integration tests)
- Mock time/random for predictable tests

### 5. **Test Edge Cases**
- Null/empty inputs
- Large datasets
- Concurrent operations
- Permission scenarios
- Error conditions

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Tests

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - name: Run tests
        run: |
          cd backend
          dotnet test --logger "trx;LogFileName=test-results.trx" /p:CollectCoverage=true

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      - name: Install dependencies
        run: |
          cd frontend
          npm ci
      - name: Run tests
        run: |
          cd frontend
          npm test
```

## Test Data Builders

For complex test scenarios, use builder pattern:

```csharp
public class ProductTestBuilder
{
    private string _name = "Test Product";
    private string _code = "TEST001";
    private Guid _tenantId = Guid.NewGuid();

    public ProductTestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Product Build()
    {
        return new Product(_tenantId, _name, _code, "PCS", 100, 150, 200, Guid.NewGuid());
    }
}

// Usage
var product = new ProductTestBuilder()
    .WithName("Custom Product")
    .Build();
```

## Debugging Tests

### Backend
```bash
# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Debug specific test in VS Code
# Set breakpoint and use "Debug Test" CodeLens
```

### Frontend
```bash
# Run in debug mode
npm test -- --inspect-brk

# Then open chrome://inspect in browser
```

## Key Test Files Created

### Backend
- ✅ `CurrentTenantServiceTests.cs` - Tests tenant resolution from JWT
- ✅ `AuthenticationServiceTests.cs` - Tests login/register with tenant filter bypass
- ✅ `ProductServiceTests.cs` - Tests product CRUD with tenant isolation

### Frontend
- ✅ `product-list.component.spec.ts` - Tests product list rendering and filtering
- ✅ `product.service.spec.ts` - Tests HTTP API calls

## Next Steps

1. **Add more test coverage**:
   - Dashboard components
   - Sales/Invoice services
   - User management
   - Reports

2. **Add integration tests**:
   - Full API endpoint tests
   - Database migration tests
   - Authentication flow tests

3. **Add E2E tests**:
   - User login flow
   - Product CRUD operations
   - Invoice creation

4. **Set up test automation**:
   - Pre-commit hooks to run tests
   - CI/CD pipeline with test gates
   - Automated coverage reports

## Running All Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npm test

# Both (from root)
./run-all-tests.sh  # Create this script
```

## Test Commands Summary

| Command | Description |
|---------|-------------|
| `dotnet test` | Run all backend tests |
| `dotnet test --filter "FullyQualifiedName~ServiceTests"` | Run specific test class |
| `dotnet watch test` | Run tests in watch mode |
| `npm test` | Run all frontend tests |
| `npm run test:watch` | Run frontend tests in watch mode |
| `npm run test:coverage` | Generate coverage report |

---

**Remember**: Tests are documentation! Write tests that explain HOW your code should behave.
