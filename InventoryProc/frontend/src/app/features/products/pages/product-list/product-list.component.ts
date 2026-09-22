import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ProductService } from '../../services/product.service';
import { Product } from '../../../../models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  searchTerm: string = '';
  loading: boolean = false;
  error: string = '';
  showLowStockOnly: boolean = false;
  uploading: boolean = false;

  constructor(
    private productService: ProductService,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    console.log('=== ProductListComponent ngOnInit ===');
    this.loadProducts();
  }

  loadProducts(): void {
    console.log('=== loadProducts called ===');
    this.loading = true;
    this.error = '';

    this.productService.getAll().subscribe({
      next: (response) => {
        console.log('=== API Response received ===');
        console.log('Full response:', response);
        console.log('response.success:', response.success);
        console.log('response.data:', response.data);
        console.log('response.data type:', typeof response.data);
        console.log('response.data is array:', Array.isArray(response.data));

        if (response.success && response.data) {
          this.products = response.data;
          console.log('Products assigned:', this.products.length, 'items');
          this.applyFilters();
        } else {
          console.warn('Response check failed:', {
            success: response.success,
            hasData: !!response.data
          });
          this.error = response.message || 'No products returned from API';
        }
        this.loading = false;
        console.log('Loading set to false, filteredProducts:', this.filteredProducts.length);
        // Manually trigger change detection
        this.cdr.detectChanges();
        console.log('Change detection triggered');
      },
      error: (err) => {
        console.error('=== API Error ===');
        console.error('Error object:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
        this.error = 'Failed to load products: ' + (err.error?.message || err.message);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    if (this.searchTerm.trim()) {
      this.loading = true;
      this.productService.search(this.searchTerm).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.products = response.data;
            this.applyFilters();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Search failed';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.loadProducts();
    }
  }

  toggleLowStockFilter(): void {
    this.showLowStockOnly = !this.showLowStockOnly;
    this.applyFilters();
  }

  applyFilters(): void {
    console.log('=== applyFilters called ===');
    console.log('showLowStockOnly:', this.showLowStockOnly);
    console.log('products.length:', this.products.length);

    if (this.showLowStockOnly) {
      this.filteredProducts = this.products.filter(p => p.isLowStock);
    } else {
      this.filteredProducts = this.products;
    }

    console.log('filteredProducts.length:', this.filteredProducts.length);
    console.log('Sample product:', this.filteredProducts[0]);
  }

  deleteProduct(id: string): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.delete(id).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadProducts();
          }
        },
        error: (err) => {
          this.error = 'Failed to delete product';
          console.error(err);
        }
      });
    }
  }

  getStockStatusClass(product: Product): string {
    if (product.isOutOfStock) return 'out-of-stock';
    if (product.isLowStock) return 'low-stock';
    return 'in-stock';
  }

  downloadTemplate(): void {
    const url = 'http://localhost:5005/api/products/template';
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const a = document.createElement('a');
        const url = window.URL.createObjectURL(blob);
        a.href = url;
        a.download = 'Products_Template.xlsx';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.error = 'Failed to download template';
        console.error(err);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.uploadExcel(file);
    }
  }

  uploadExcel(file: File): void {
    const formData = new FormData();
    formData.append('file', file);

    this.uploading = true;
    this.error = '';

    const url = 'http://localhost:5005/api/products/import';
    this.http.post(url, formData).subscribe({
      next: (response: any) => {
        alert(`Import completed!\nSuccess: ${response.successCount}\nErrors: ${response.errorCount}`);
        if (response.errors && response.errors.length > 0) {
          console.error('Import errors:', response.errors);
        }
        this.uploading = false;
        this.loadProducts();
      },
      error: (err) => {
        this.error = 'Failed to import products';
        this.uploading = false;
        console.error(err);
      }
    });
  }
}
