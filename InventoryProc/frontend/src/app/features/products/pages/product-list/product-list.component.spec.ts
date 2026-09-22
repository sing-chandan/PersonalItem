import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { ProductListComponent } from './product-list.component';
import { ProductService } from '../../services/product.service';
import { Product } from '../../../../models/product.model';
import { of, throwError } from 'rxjs';

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let productService: jest.Mocked<ProductService>;

  const mockProducts: Product[] = [
    {
      id: '1',
      name: 'Product 1',
      code: 'P001',
      categoryId: 'cat1',
      categoryName: 'Electronics',
      unit: 'PCS',
      purchasePrice: 100,
      salePrice: 150,
      mrp: 200,
      currentStock: 50,
      isActive: true,
      isLowStock: false,
      isOutOfStock: false,
      createdAt: new Date()
    },
    {
      id: '2',
      name: 'Product 2',
      code: 'P002',
      categoryId: 'cat1',
      categoryName: 'Electronics',
      unit: 'PCS',
      purchasePrice: 200,
      salePrice: 250,
      mrp: 300,
      currentStock: 5,
      isActive: true,
      isLowStock: true,
      isOutOfStock: false,
      createdAt: new Date()
    }
  ];

  beforeEach(async () => {
    const productServiceMock = {
      getAll: jest.fn(),
      search: jest.fn(),
      delete: jest.fn()
    };

    await TestBed.configureTestingModule({
      imports: [
        ProductListComponent,
        HttpClientTestingModule,
        RouterTestingModule,
        FormsModule
      ],
      providers: [
        { provide: ProductService, useValue: productServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    productService = TestBed.inject(ProductService) as jest.Mocked<ProductService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('loadProducts', () => {
    it('should load products successfully', (done) => {
      // Arrange
      const apiResponse = {
        success: true,
        message: 'Success',
        data: mockProducts
      };
      productService.getAll.mockReturnValue(of(apiResponse));

      // Act
      component.loadProducts();

      // Assert
      setTimeout(() => {
        expect(component.loading).toBe(false);
        expect(component.products).toEqual(mockProducts);
        expect(component.filteredProducts).toEqual(mockProducts);
        expect(component.error).toBe('');
        done();
      }, 0);
    });

    it('should handle API error', (done) => {
      // Arrange
      const error = { message: 'API Error', error: { message: 'Failed' } };
      productService.getAll.mockReturnValue(throwError(() => error));

      // Act
      component.loadProducts();

      // Assert
      setTimeout(() => {
        expect(component.loading).toBe(false);
        expect(component.error).toContain('Failed to load products');
        expect(component.products).toEqual([]);
        done();
      }, 0);
    });

    it('should handle empty data', (done) => {
      // Arrange
      const apiResponse = {
        success: true,
        message: 'Success',
        data: []
      };
      productService.getAll.mockReturnValue(of(apiResponse));

      // Act
      component.loadProducts();

      // Assert
      setTimeout(() => {
        expect(component.loading).toBe(false);
        expect(component.products).toEqual([]);
        expect(component.filteredProducts).toEqual([]);
        done();
      }, 0);
    });

    it('should handle success=false response', (done) => {
      // Arrange
      const apiResponse = {
        success: false,
        message: 'No access',
        data: null as any
      };
      productService.getAll.mockReturnValue(of(apiResponse));

      // Act
      component.loadProducts();

      // Assert
      setTimeout(() => {
        expect(component.loading).toBe(false);
        expect(component.error).toContain('No access');
        done();
      }, 0);
    });
  });

  describe('applyFilters', () => {
    beforeEach(() => {
      component.products = mockProducts;
    });

    it('should show all products when filter is off', () => {
      // Arrange
      component.showLowStockOnly = false;

      // Act
      component.applyFilters();

      // Assert
      expect(component.filteredProducts).toEqual(mockProducts);
      expect(component.filteredProducts.length).toBe(2);
    });

    it('should show only low stock products when filter is on', () => {
      // Arrange
      component.showLowStockOnly = true;

      // Act
      component.applyFilters();

      // Assert
      expect(component.filteredProducts.length).toBe(1);
      expect(component.filteredProducts[0].isLowStock).toBe(true);
    });
  });

  describe('onSearch', () => {
    it('should search products with valid term', (done) => {
      // Arrange
      component.searchTerm = 'Product';
      const apiResponse = {
        success: true,
        message: 'Success',
        data: [mockProducts[0]]
      };
      productService.search.mockReturnValue(of(apiResponse));

      // Act
      component.onSearch();

      // Assert
      setTimeout(() => {
        expect(productService.search).toHaveBeenCalledWith('Product');
        expect(component.products.length).toBe(1);
        done();
      }, 0);
    });

    it('should call loadProducts when search term is empty', () => {
      // Arrange
      component.searchTerm = '';
      const spy = jest.spyOn(component, 'loadProducts');

      // Act
      component.onSearch();

      // Assert
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('deleteProduct', () => {
    it('should delete product on confirmation', (done) => {
      // Arrange
      const productId = '1';
      global.confirm = jest.fn(() => true);
      const apiResponse = {
        success: true,
        message: 'Deleted',
        data: undefined as any
      };
      productService.delete.mockReturnValue(of(apiResponse));
      const loadProductsSpy = jest.spyOn(component, 'loadProducts').mockImplementation(() => {});

      // Act
      component.deleteProduct(productId);

      // Assert
      setTimeout(() => {
        expect(productService.delete).toHaveBeenCalledWith(productId);
        expect(loadProductsSpy).toHaveBeenCalled();
        done();
      }, 0);
    });

    it('should not delete product when cancelled', () => {
      // Arrange
      const productId = '1';
      global.confirm = jest.fn(() => false);

      // Act
      component.deleteProduct(productId);

      // Assert
      expect(productService.delete).not.toHaveBeenCalled();
    });
  });

  describe('getStockStatusClass', () => {
    it('should return out-of-stock for out of stock product', () => {
      const product = { ...mockProducts[0], isOutOfStock: true };
      expect(component.getStockStatusClass(product)).toBe('out-of-stock');
    });

    it('should return low-stock for low stock product', () => {
      const product = { ...mockProducts[0], isLowStock: true, isOutOfStock: false };
      expect(component.getStockStatusClass(product)).toBe('low-stock');
    });

    it('should return in-stock for normal stock', () => {
      const product = { ...mockProducts[0], isLowStock: false, isOutOfStock: false };
      expect(component.getStockStatusClass(product)).toBe('in-stock');
    });
  });
});
