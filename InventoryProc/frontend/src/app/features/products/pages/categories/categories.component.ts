import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../../../models/category.model';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {
  categories: Category[] = [];
  categoryForm: FormGroup;
  loading: boolean = false;
  error: string = '';
  isEditMode: boolean = false;
  editingId: string | null = null;
  showForm: boolean = false;
  uploading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private http: HttpClient
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      code: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', Validators.maxLength(1000)],
      parentCategoryId: ['']
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getAll().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.categories = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load categories';
        this.loading = false;
        console.error(err);
      }
    });
  }

  showAddForm(): void {
    this.isEditMode = false;
    this.editingId = null;
    this.categoryForm.reset();
    this.categoryForm.get('code')?.enable();
    this.showForm = true;
  }

  editCategory(category: Category): void {
    this.isEditMode = true;
    this.editingId = category.id;
    this.categoryForm.patchValue({
      name: category.name,
      code: category.code,
      description: category.description,
      parentCategoryId: category.parentCategoryId
    });
    this.categoryForm.get('code')?.disable();
    this.showForm = true;
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';

    const formValue = this.categoryForm.getRawValue();

    if (this.isEditMode && this.editingId) {
      this.categoryService.update(this.editingId, formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadCategories();
            this.cancelEdit();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to update category';
          this.loading = false;
        }
      });
    } else {
      this.categoryService.create(formValue).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadCategories();
            this.cancelEdit();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.message || 'Failed to create category';
          this.loading = false;
        }
      });
    }
  }

  deleteCategory(id: string): void {
    if (confirm('Are you sure you want to delete this category?')) {
      this.categoryService.delete(id).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadCategories();
          }
        },
        error: (err) => {
          this.error = 'Failed to delete category';
          console.error(err);
        }
      });
    }
  }

  cancelEdit(): void {
    this.showForm = false;
    this.isEditMode = false;
    this.editingId = null;
    this.categoryForm.reset();
    this.error = '';
  }

  downloadTemplate(): void {
    const url = 'http://localhost:5005/api/categories/template';
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const a = document.createElement('a');
        const url = window.URL.createObjectURL(blob);
        a.href = url;
        a.download = 'Categories_Template.xlsx';
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

    const url = 'http://localhost:5005/api/categories/import';
    this.http.post(url, formData).subscribe({
      next: (response: any) => {
        alert(`Import completed!\nSuccess: ${response.successCount}\nErrors: ${response.errorCount}`);
        if (response.errors && response.errors.length > 0) {
          console.error('Import errors:', response.errors);
        }
        this.uploading = false;
        this.loadCategories();
      },
      error: (err) => {
        this.error = 'Failed to import categories';
        this.uploading = false;
        console.error(err);
      }
    });
  }
}
