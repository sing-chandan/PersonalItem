import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SalesReportService } from '../../services/sales-report.service';
import { SalesReportRequest, SalesReportResponse } from '../../models/sales-report.model';

@Component({
  selector: 'app-sales-report',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './sales-report.component.html',
  styleUrls: ['./sales-report.component.css']
})
export class SalesReportComponent implements OnInit {
  filterForm: FormGroup;
  report: SalesReportResponse | null = null;
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private salesReportService: SalesReportService
  ) {
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

    this.filterForm = this.fb.group({
      startDate: [firstDayOfMonth.toISOString().substring(0, 10)],
      endDate: [today.toISOString().substring(0, 10)],
      customerId: [''],
      productId: ['']
    });
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    this.error = null;

    const request: SalesReportRequest = {
      startDate: this.filterForm.value.startDate ? new Date(this.filterForm.value.startDate) : undefined,
      endDate: this.filterForm.value.endDate ? new Date(this.filterForm.value.endDate) : undefined,
      customerId: this.filterForm.value.customerId || undefined,
      productId: this.filterForm.value.productId || undefined
    };

    this.salesReportService.getSalesReport(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.report = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load sales report. Please try again.';
        this.loading = false;
        console.error('Error loading sales report:', err);
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
      customerId: '',
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
