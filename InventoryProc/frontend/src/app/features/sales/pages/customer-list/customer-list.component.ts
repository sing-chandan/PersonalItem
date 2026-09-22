import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import { Customer } from '../../models/customer.model';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss'],
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  loading = false;
  searchTerm = '';

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.loading = true;
    this.customerService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.customers = response.data;
          this.filteredCustomers = response.data;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        alert('Failed to load customers');
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onSearch(): void {
    if (!this.searchTerm.trim()) {
      this.filteredCustomers = this.customers;
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.customerService.search(this.searchTerm).subscribe({
      next: (response) => {
        if (response.success) {
          this.filteredCustomers = response.data;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error searching customers:', error);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.filteredCustomers = this.customers;
  }

  editCustomer(id: string): void {
    this.router.navigate(['/sales/customers/edit', id]);
  }

  deleteCustomer(customer: Customer): void {
    if (
      !confirm(
        `Are you sure you want to delete customer "${customer.companyName}"?`
      )
    ) {
      return;
    }

    this.customerService.delete(customer.id).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Customer deleted successfully');
          this.loadCustomers();
        } else {
          alert(response.message || 'Failed to delete customer');
        }
      },
      error: (error) => {
        console.error('Error deleting customer:', error);
        alert('Failed to delete customer');
      },
    });
  }

  viewOrders(customerId: string): void {
    this.router.navigate(['/sales/orders'], {
      queryParams: { customerId },
    });
  }
}
