import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PurchaseReportService } from '../../services/purchase-report.service';
import { PurchaseReportRequest, PurchaseReportResponse } from '../../models/purchase-report.model';

@Component({
  selector: 'app-purchase-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './purchase-report.component.html',
  styleUrls: ['./purchase-report.component.css']
})
export class PurchaseReportComponent implements OnInit {
  filterForm: FormGroup;
  report: PurchaseReportResponse | null = null;
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private purchaseReportService: PurchaseReportService
  ) {
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

    this.filterForm = this.fb.group({
      startDate: [firstDayOfMonth.toISOString().substring(0, 10)],
      endDate: [today.toISOString().substring(0, 10)],
      vendorId: [''],
      productId: ['']
    });
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    this.error = null;

    const request: PurchaseReportRequest = {
      startDate: this.filterForm.value.startDate ? new Date(this.filterForm.value.startDate) : undefined,
      endDate: this.filterForm.value.endDate ? new Date(this.filterForm.value.endDate) : undefined,
      vendorId: this.filterForm.value.vendorId || undefined,
      productId: this.filterForm.value.productId || undefined
    };

    this.purchaseReportService.getPurchaseReport(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.report = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load purchase report. Please try again.';
        this.loading = false;
        console.error('Error loading purchase report:', err);
      }
    });
  }

  onFilter(): void {
    this.loadReport();
  }

  onReset(): void {
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

    this.filterForm.patchValue({
      startDate: firstDayOfMonth.toISOString().substring(0, 10),
      endDate: today.toISOString().substring(0, 10),
      vendorId: '',
      productId: ''
    });

    this.loadReport();
  }

  formatCurrency(value: number): string {
    return '₹' + value.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatNumber(value: number): string {
    return value.toLocaleString('en-IN');
  }
}
