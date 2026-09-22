# Quick Test Guide - Products Module

## 🚀 Quick Start Testing (When You Return)

### Step 1: Build the Backend
```bash
cd c:\PersonalItem\InventoryProc\backend
dotnet build
```
**Expected**: Build succeeds with 0 errors

### Step 2: Create Database Migration
```bash
cd c:\PersonalItem\InventoryProc\backend\src\BuildingBlocks\InventoryProc.Infrastructure

dotnet ef migrations add AddProductsModule --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"

dotnet ef database update --startup-project "../../Host/InventoryProc.API/InventoryProc.API.csproj"
```
**Expected**: Tables created - Products, Categories, Brands

### Step 3: Start the API
```bash
cd c:\PersonalItem\InventoryProc\backend\src\Host\InventoryProc.API
dotnet run
```
**Expected**:
- API running on http://localhost:5005
- Swagger UI at http://localhost:5005

---

## 🧪 Quick API Test Sequence

### 1. Login (Get Token)
```bash
curl -X POST http://localhost:5005/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@acme.com\",\"password\":\"Admin@123\"}"
```
**Save the token from response!**

### 2. Create Category
```bash
curl -X POST http://localhost:5005/api/categories ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -H "Content-Type: application/json" ^
  -d "{\"name\":\"Electronics\",\"code\":\"ELEC001\",\"description\":\"Electronic items\"}"
```
**Save the category ID from response!**

### 3. Create Product
```bash
curl -X POST http://localhost:5005/api/products ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -H "Content-Type: application/json" ^
  -d "{\"name\":\"Laptop\",\"code\":\"LAP001\",\"categoryId\":\"CATEGORY_ID_HERE\",\"unit\":\"PCS\",\"purchasePrice\":50000,\"salePrice\":60000,\"mrp\":65000,\"minStockLevel\":5,\"taxType\":\"GST\",\"taxRate\":18}"
```
**Save the product ID from response!**

### 4. Adjust Stock
```bash
curl -X POST http://localhost:5005/api/products/PRODUCT_ID_HERE/adjust-stock ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -H "Content-Type: application/json" ^
  -d "{\"quantity\":10,\"reason\":\"Initial stock\",\"notes\":\"Opening stock entry\"}"
```

### 5. Get All Products
```bash
curl -X GET http://localhost:5005/api/products ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```
**Expected**: List with 1 product, stock = 10

### 6. Get Low Stock Products
```bash
curl -X GET http://localhost:5005/api/products/low-stock ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```
**Expected**: 1 product (since stock=10 and min=5, product appears in low stock list)

### 7. Search Products
```bash
curl -X GET "http://localhost:5005/api/products/search?searchTerm=Laptop" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```
**Expected**: 1 product found

---

## 🎯 Using Swagger UI (Easier!)

1. Open browser: http://localhost:5005
2. Click "Authorize" button (top right)
3. Enter: `Bearer YOUR_TOKEN`
4. Click "Authorize", then "Close"
5. Now all endpoints are authorized!
6. Test endpoints by clicking "Try it out"

### Swagger Test Sequence
1. **POST /api/auth/login** - Get token
2. Click **Authorize** - Paste token
3. **POST /api/categories** - Create "Electronics"
4. **POST /api/products** - Create "Laptop"
5. **POST /api/products/{id}/adjust-stock** - Add 10 units
6. **GET /api/products** - See product list
7. **GET /api/products/low-stock** - See low stock alerts

---

## ✅ Verification Checklist

### Backend Health Check
- [ ] `dotnet build` succeeds
- [ ] Migration created successfully
- [ ] Database updated successfully
- [ ] API starts without errors
- [ ] Swagger UI loads
- [ ] Login endpoint works
- [ ] Token authorization works

### Product Management Check
- [ ] Category created successfully
- [ ] Product created successfully
- [ ] Stock adjustment works
- [ ] Get all products returns data
- [ ] Search works correctly
- [ ] Low stock detection works

### Multi-Tenant Check
- [ ] Register second tenant
- [ ] Create products for second tenant
- [ ] Verify tenant 1 cannot see tenant 2's products
- [ ] Logout and login as tenant 2
- [ ] Verify tenant 2 cannot see tenant 1's products

---

## 📊 Expected Database State After Tests

### Tenants Table
- 1 record: "Acme Corporation" (or whatever you registered)

### Users Table
- 1 record: Admin user for Acme

### Categories Table
- 1 record: "Electronics" (ELEC001)

### Products Table
- 1 record: "Laptop" (LAP001)
- CurrentStock = 10
- CategoryId = Electronics ID

### Brands Table
- 0 records (we didn't create any brands yet)

---

## 🐛 Common Issues & Solutions

### Issue 1: Build Fails
**Error**: "Project reference not found"
**Solution**: Check Products.csproj exists at:
```
backend/src/Modules/Products/InventoryProc.Modules.Products.csproj
```

### Issue 2: Migration Fails
**Error**: "Cannot find DbSet"
**Solution**: Verify ApplicationDbContext has:
```csharp
public DbSet<Product> Products => Set<Product>();
public DbSet<Category> Categories => Set<Category>();
public DbSet<Brand> Brands => Set<Brand>();
```

### Issue 3: Services Not Found
**Error**: "No service for type IProductService"
**Solution**: Check Program.cs has:
```csharp
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
```

### Issue 4: Cannot Access Products
**Error**: 401 Unauthorized
**Solution**: Make sure to:
1. Login first to get token
2. Use token in Authorization header
3. Token format: `Bearer {token}`

### Issue 5: Products Not Showing
**Error**: Empty list
**Solution**:
1. Check if product was created successfully
2. Verify you're logged in with correct tenant
3. Check IsActive = true
4. Look in database directly to verify

---

## 🔍 Database Verification Queries

### Check Products
```sql
SELECT * FROM Products WHERE TenantId = 'YOUR_TENANT_ID'
```

### Check with Category Name
```sql
SELECT
    p.Name as ProductName,
    p.Code as ProductCode,
    c.Name as CategoryName,
    p.CurrentStock,
    p.MinStockLevel,
    p.IsActive
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
WHERE p.TenantId = 'YOUR_TENANT_ID'
```

### Check Low Stock
```sql
SELECT
    Name,
    Code,
    CurrentStock,
    MinStockLevel
FROM Products
WHERE
    TenantId = 'YOUR_TENANT_ID'
    AND IsActive = 1
    AND MinStockLevel IS NOT NULL
    AND CurrentStock <= MinStockLevel
```

---

## 🎨 Testing with Postman (Alternative)

### Import Collection Steps
1. Open Postman
2. Create new collection: "InventoryProc"
3. Add requests:
   - Login
   - Create Category
   - Create Product
   - Adjust Stock
   - Get Products
   - etc.

### Environment Variables
Create environment with:
- `base_url`: http://localhost:5005
- `token`: (will be set from login response)
- `category_id`: (will be set from create category response)
- `product_id`: (will be set from create product response)

### Auto Token Management
In Tests tab of Login request:
```javascript
var jsonData = pm.response.json();
pm.environment.set("token", jsonData.data.token);
```

In Headers of all other requests:
```
Authorization: Bearer {{token}}
```

---

## 📈 Performance Testing (Optional)

### Load Test with Apache Bench
```bash
# Login load test
ab -n 1000 -c 10 -p login.json -T application/json http://localhost:5005/api/auth/login

# Get products load test
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" http://localhost:5005/api/products
```

### Expected Performance
- Login: < 100ms
- Get Products: < 50ms
- Create Product: < 100ms
- Search: < 100ms

---

## 🎯 Success Criteria

✅ All endpoints return expected status codes
✅ Products are tenant-isolated
✅ Stock adjustments work correctly
✅ Search finds products
✅ Low stock detection works
✅ Categories work correctly
✅ Swagger documentation is accurate
✅ No unauthorized access possible
✅ Database schema is correct

---

## 🚀 Ready for Frontend?

Once backend testing is complete:
1. ✅ Backend API working
2. ✅ All endpoints tested
3. ✅ Database schema verified
4. ⏭️ Start Angular frontend implementation

**Next**: Create Angular components for Products module!

---

**Quick Reminder**:
- Backend API: http://localhost:5005
- Swagger UI: http://localhost:5005
- Frontend (when ready): http://localhost:4200

Happy Testing! 🎉
