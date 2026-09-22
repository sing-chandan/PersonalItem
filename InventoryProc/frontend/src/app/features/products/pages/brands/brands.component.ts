import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BrandService } from '../../services/brand.service';
import { Brand } from '../../../../models/brand.model';

@Component({
  selector: 'app-brands',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './brands.component.html',
  styleUrls: ['./brands.component.scss']
})
export class BrandsComponent implements OnInit {
  brands: Brand[] = [];
  brandForm: FormGroup;
  loading: boolean = false;
  error: string = '';
  isEditMode: boolean = false;
  editingId: string | null = null;
  showForm: boolean = false;

  constructor(
    private fb: FormBuilder,
    private brandService: BrandService
  ) {
    this.brandForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', Validators.maxLength(1000)]
    });
  }

  ngOnInit(): void {
    this.loadBrands();
  }

  loadBrands(): void {
    this.loading = true;
    this.brandService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.brands = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load brands';
        this.loading = false;
        console.error(err);
      }
    });
  }

  showAddForm(): void {
    this.isEditMode = false;
    this.editingId = null;
    this.brandForm.reset();
    this.brandForm.get('code')?.enable();
    this.showForm = true;
  }

  editBrand(brand: Brand): void {
    this.isEditMode = true;
    this.editingId = brand.id;
    this.brandForm.patchValue({
      name: brand.name,
      code: brand.code,
      description: brand.description
    });
    this.brandForm.get('code')?.disable();
    this.showForm = true;
  }

  onSubmit(): void {
    if (this.brandForm.invalid) {
      this.brandForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';

    const formValue = this.brandForm.getRawValue();

    if (this.isEditMode && this.editingId) {
      this.brandService.update(this.editingId, formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadBrands();
            this.cancelEdit();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to update brand';
          this.loading = false;
        }
      });
    } else {
      this.brandService.create(formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadBrands();
            this.cancelEdit();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to create brand';
          this.loading = false;
        }
      });
    }
  }

  deleteBrand(id: string): void {
    if (confirm('Are you sure you want to delete this brand?')) {
      this.brandService.delete(id).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadBrands();
          }
        },
        error: (err) => {
          this.error = 'Failed to delete brand';
          console.error(err);
        }
      });
    }
  }

  cancelEdit(): void {
    this.showForm = false;
    this.isEditMode = false;
    this.editingId = null;
    this.brandForm.reset();
    this.error = '';
  }
}
