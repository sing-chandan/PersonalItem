import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { Product } from '../../../models/product.model';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;
  const apiUrl = 'http://localhost:5005/api/products';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });

    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should return products from API', (done) => {
      // Arrange
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
        }
      ];

      const expectedResponse = {
        success: true,
        message: 'Success',
        data: mockProducts
      };

      // Act
      service.getAll().subscribe((response) => {
        // Assert
        expect(response.success).toBe(true);
        expect(response.data).toEqual(mockProducts);
        expect(response.data.length).toBe(1);
        done();
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(expectedResponse);
    });
  });

  describe('getById', () => {
    it('should return single product by id', (done) => {
      // Arrange
      const productId = '123';
      const mockProduct: Product = {
        id: productId,
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
      };

      const expectedResponse = {
        success: true,
        message: 'Success',
        data: mockProduct
      };

      // Act
      service.getById(productId).subscribe((response) => {
        // Assert
        expect(response.success).toBe(true);
        expect(response.data.id).toBe(productId);
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.method).toBe('GET');
      req.flush(expectedResponse);
    });
  });

  describe('search', () => {
    it('should search products with term', (done) => {
      // Arrange
      const searchTerm = 'laptop';
      const mockProducts: Product[] = [];

      const expectedResponse = {
        success: true,
        message: 'Success',
        data: mockProducts
      };

      // Act
      service.search(searchTerm).subscribe((response) => {
        // Assert
        expect(response.success).toBe(true);
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/search?searchTerm=${searchTerm}`);
      expect(req.request.method).toBe('GET');
      req.flush(expectedResponse);
    });
  });

  describe('delete', () => {
    it('should delete product by id', (done) => {
      // Arrange
      const productId = '123';
      const expectedResponse = {
        success: true,
        message: 'Deleted',
        data: undefined as any
      };

      // Act
      service.delete(productId).subscribe((response) => {
        // Assert
        expect(response.success).toBe(true);
        done();
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(expectedResponse);
    });
  });
});
