import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { BrandService } from '../../services/brand.service';
import { Category } from '../../../../models/category.model';
import { Brand } from '../../../../models/brand.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  isEditMode: boolean = false;
  productId: string | null = null;
  loading: boolean = false;
  error: string = '';
  categories: Category[] = [];
  brands: Brand[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private brandService: BrandService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', Validators.maxLength(1000)],
      barcode: ['', Validators.maxLength(100)],
      categoryId: ['', Validators.required],
      brandId: [''],
      unit: ['PCS', Validators.required],
      purchasePrice: [0, [Validators.required, Validators.min(0)]],
      salePrice: [0, [Validators.required, Validators.min(0)]],
      mrp: [0, [Validators.required, Validators.min(0)]],
      minStockLevel: [0, Validators.min(0)],
      maxStockLevel: [0, Validators.min(0)],
      taxType: [''],
      taxRate: [0, [Validators.min(0), Validators.max(100)]],
      hsnCode: ['', Validators.maxLength(20)]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadBrands();

    this.productId = this.route.snapshot.paramMap.get('id');
    if (this.productId) {
      this.isEditMode = true;
      this.loadProduct(this.productId);
      this.productForm.get('code')?.disable();
    }
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.categories = response.data;
        }
      },
      error: (err) => console.error('Failed to load categories', err)
    });
  }

  loadBrands(): void {
    this.brandService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.brands = response.data;
        }
      },
      error: (err) => console.error('Failed to load brands', err)
    });
  }

  loadProduct(id: string): void {
    this.loading = true;
    this.productService.getById(id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const product = response.data;
          this.productForm.patchValue({
            name: product.name,
            code: product.code,
            description: product.description,
            barcode: product.barcode,
            categoryId: product.categoryId,
            brandId: product.brandId,
            unit: product.unit,
            purchasePrice: product.purchasePrice,
            salePrice: product.salePrice,
            mrp: product.mrp,
            minStockLevel: product.minStockLevel,
            maxStockLevel: product.maxStockLevel,
            taxType: product.taxType,
            taxRate: product.taxRate,
            hsnCode: product.hsnCode
          });
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load product';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';

    const formValue = this.productForm.getRawValue();

    if (this.isEditMode && this.productId) {
      this.productService.update(this.productId, formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/products/list']);
          }
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to update product';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.productService.create(formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/products/list']);
          }
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to create product';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/products/list']);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.productForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['maxLength']) return `Maximum length exceeded`;
      if (field.errors['min']) return `Value must be greater than or equal to ${field.errors['min'].min}`;
      if (field.errors['max']) return `Value must be less than or equal to ${field.errors['max'].max}`;
    }
    return '';
  }
}
