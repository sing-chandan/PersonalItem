import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { InventoryReportService } from '../../services/inventory-report.service';
import { InventoryReportRequest, InventoryReportResponse } from '../../models/inventory-report.model';

@Component({
  selector: 'app-inventory-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './inventory-report.component.html',
  styleUrls: ['./inventory-report.component.css']
})
export class InventoryReportComponent implements OnInit {
  filterForm: FormGroup;
  report: InventoryReportResponse | null = null;
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private inventoryReportService: InventoryReportService
  ) {
    this.filterForm = this.fb.group({
      categoryId: [''],
      brandId: [''],
      lowStockOnly: [false]
    });
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    this.error = null;

    const request: InventoryReportRequest = {
      categoryId: this.filterForm.value.categoryId || undefined,
      brandId: this.filterForm.value.brandId || undefined,
      lowStockOnly: this.filterForm.value.lowStockOnly || undefined
    };

    this.inventoryReportService.getInventoryReport(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.report = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load inventory report. Please try again.';
        this.loading = false;
        console.error('Error loading inventory report:', err);
      }
    });
  }

  onFilter(): void {
    this.loadReport();
  }

  onReset(): void {
    this.filterForm.patchValue({
      categoryId: '',
      brandId: '',
      lowStockOnly: false
    });

    this.loadReport();
  }

  formatCurrency(value: number): string {
    return '₹' + value.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatNumber(value: number): string {
    return value.toLocaleString('en-IN');
  }

  getStockStatusClass(currentStock: number, minStock: number): string {
    if (currentStock === 0) return 'text-danger fw-bold';
    if (currentStock <= minStock) return 'text-warning fw-bold';
    return 'text-success';
  }
}
