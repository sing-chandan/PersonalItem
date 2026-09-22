import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InvoiceService } from '../../services/invoice.service';
import { Invoice, InvoicePaymentStatus } from '../../models/invoice.model';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss'],
})
export class InvoiceListComponent implements OnInit {
  invoices: Invoice[] = [];
  filteredInvoices: Invoice[] = [];
  loading = false;
  InvoicePaymentStatus = InvoicePaymentStatus;
  filterStatus: string = 'all';

  constructor(
    private invoiceService: InvoiceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.loading = true;
    this.invoiceService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.invoices = response.data;
          this.applyFilter();
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading invoices:', error);
        alert('Failed to load invoices');
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadOverdueInvoices(): void {
    this.loading = true;
    this.invoiceService.getOverdueInvoices().subscribe({
      next: (response) => {
        if (response.success) {
          this.invoices = response.data;
          this.filteredInvoices = this.invoices;
          this.filterStatus = 'overdue';
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading overdue invoices:', error);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadUnpaidInvoices(): void {
    this.loading = true;
    this.invoiceService.getUnpaidInvoices().subscribe({
      next: (response) => {
        if (response.success) {
          this.invoices = response.data;
          this.filteredInvoices = this.invoices;
          this.filterStatus = 'unpaid';
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading unpaid invoices:', error);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  applyFilter(): void {
    switch (this.filterStatus) {
      case 'all':
        this.filteredInvoices = this.invoices;
        break;
      case 'paid':
        this.filteredInvoices = this.invoices.filter(
          (inv) => inv.paymentStatus === InvoicePaymentStatus.Paid
        );
        break;
      case 'unpaid':
        this.filteredInvoices = this.invoices.filter(
          (inv) => inv.paymentStatus === InvoicePaymentStatus.Unpaid
        );
        break;
      case 'partially-paid':
        this.filteredInvoices = this.invoices.filter(
          (inv) => inv.paymentStatus === InvoicePaymentStatus.PartiallyPaid
        );
        break;
      case 'overdue':
        this.filteredInvoices = this.invoices.filter(
          (inv) => inv.paymentStatus === InvoicePaymentStatus.Overdue
        );
        break;
      default:
        this.filteredInvoices = this.invoices;
    }
  }

  getStatusClass(status: InvoicePaymentStatus): string {
    switch (status) {
      case InvoicePaymentStatus.Unpaid:
        return 'status-unpaid';
      case InvoicePaymentStatus.PartiallyPaid:
        return 'status-partially-paid';
      case InvoicePaymentStatus.Paid:
        return 'status-paid';
      case InvoicePaymentStatus.Overdue:
        return 'status-overdue';
      case InvoicePaymentStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  isOverdue(invoice: Invoice): boolean {
    const dueDate = new Date(invoice.dueDate);
    const today = new Date();
    return dueDate < today && invoice.paymentStatus !== InvoicePaymentStatus.Paid;
  }

  getCountByStatus(status: InvoicePaymentStatus): number {
    return this.invoices.filter((inv) => inv.paymentStatus === status).length;
  }
}
