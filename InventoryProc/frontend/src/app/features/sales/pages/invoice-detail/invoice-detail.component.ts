import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { InvoiceService } from '../../services/invoice.service';
import { Invoice, InvoicePaymentStatus, RecordPaymentRequest } from '../../models/invoice.model';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.scss']
})
export class InvoiceDetailComponent implements OnInit {
  invoice: Invoice | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  InvoicePaymentStatus = InvoicePaymentStatus;
  showPaymentModal = false;
  paymentForm!: FormGroup;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private invoiceService: InvoiceService,
    private fb: FormBuilder
  ) {
    this.initializePaymentForm();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadInvoice(id);
    }
  }

  initializePaymentForm(): void {
    this.paymentForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(0.01)]],
      paymentMethod: ['Cash', Validators.required],
      paymentDate: [new Date().toISOString().split('T')[0], Validators.required],
      reference: [''],
      notes: ['']
    });
  }

  loadInvoice(id: string): void {
    this.isLoading = true;
    this.invoiceService.getById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.invoice = response.data;
        } else {
          this.errorMessage = response.message || 'Failed to load invoice';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading invoice:', error);
        this.errorMessage = 'Failed to load invoice';
        this.isLoading = false;
      }
    });
  }

  getStatusBadgeClass(status: InvoicePaymentStatus): string {
    switch (status) {
      case InvoicePaymentStatus.Unpaid:
        return 'badge-unpaid';
      case InvoicePaymentStatus.PartiallyPaid:
        return 'badge-partially-paid';
      case InvoicePaymentStatus.Paid:
        return 'badge-paid';
      case InvoicePaymentStatus.Overdue:
        return 'badge-overdue';
      case InvoicePaymentStatus.Cancelled:
        return 'badge-cancelled';
      default:
        return '';
    }
  }

  isOverdue(): boolean {
    if (!this.invoice) return false;
    const dueDate = new Date(this.invoice.dueDate);
    const today = new Date();
    return dueDate < today && this.invoice.paymentStatus !== InvoicePaymentStatus.Paid;
  }

  canRecordPayment(): boolean {
    return this.invoice?.paymentStatus !== InvoicePaymentStatus.Paid &&
           this.invoice?.paymentStatus !== InvoicePaymentStatus.Cancelled;
  }

  openPaymentModal(): void {
    if (this.invoice) {
      this.paymentForm.patchValue({
        amount: this.invoice.balanceAmount
      });
      this.showPaymentModal = true;
      this.errorMessage = '';
      this.successMessage = '';
    }
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.paymentForm.reset({
      amount: '',
      paymentMethod: 'Cash',
      paymentDate: new Date().toISOString().split('T')[0],
      reference: '',
      notes: ''
    });
  }

  recordPayment(): void {
    if (this.paymentForm.invalid || !this.invoice) {
      return;
    }

    const formValue = this.paymentForm.value;
    const request: RecordPaymentRequest = {
      amount: parseFloat(formValue.amount),
      paymentMethod: formValue.paymentMethod,
      paymentDate: formValue.paymentDate,
      reference: formValue.reference || undefined,
      notes: formValue.notes || undefined
    };

    // Validate amount doesn't exceed balance
    if (request.amount > this.invoice.balanceAmount) {
      this.errorMessage = 'Payment amount cannot exceed balance amount';
      return;
    }

    this.isLoading = true;
    this.invoiceService.recordPayment(this.invoice.id, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Payment recorded successfully';
          this.closePaymentModal();
          this.loadInvoice(this.invoice!.id);
        } else {
          this.errorMessage = response.message || 'Failed to record payment';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error recording payment:', error);
        this.errorMessage = error.error?.message || 'Failed to record payment';
        this.isLoading = false;
      }
    });
  }

  voidInvoice(): void {
    if (!this.invoice) return;

    if (confirm('Are you sure you want to void this invoice? This action cannot be undone.')) {
      this.isLoading = true;
      this.invoiceService.voidInvoice(this.invoice.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Invoice voided successfully';
            this.loadInvoice(this.invoice!.id);
          } else {
            this.errorMessage = response.message || 'Failed to void invoice';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error voiding invoice:', error);
          this.errorMessage = error.error?.message || 'Failed to void invoice';
          this.isLoading = false;
        }
      });
    }
  }

  deleteInvoice(): void {
    if (!this.invoice) return;

    if (confirm('Are you sure you want to delete this invoice? This action cannot be undone.')) {
      this.isLoading = true;
      this.invoiceService.delete(this.invoice.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/sales/invoices']);
          } else {
            this.errorMessage = response.message || 'Failed to delete invoice';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error deleting invoice:', error);
          this.errorMessage = error.error?.message || 'Failed to delete invoice';
          this.isLoading = false;
        }
      });
    }
  }

  viewSalesOrder(): void {
    if (this.invoice?.salesOrderId) {
      this.router.navigate(['/sales/orders', this.invoice.salesOrderId]);
    }
  }

  back(): void {
    this.router.navigate(['/sales/invoices']);
  }
}
