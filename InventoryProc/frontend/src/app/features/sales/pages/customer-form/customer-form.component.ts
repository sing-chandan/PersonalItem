import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../services/customer.service';
import {
  CreateCustomerRequest,
  UpdateCustomerRequest,
  Customer,
} from '../../models/customer.model';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-form.component.html',
  styleUrls: ['./customer-form.component.scss'],
})
export class CustomerFormComponent implements OnInit {
  isEditMode = false;
  customerId: string | null = null;
  loading = false;
  saving = false;

  customer: CreateCustomerRequest = {
    customerCode: '',
    companyName: '',
    contactPerson: '',
    email: '',
    phone: '',
    mobile: '',
    address: '',
    city: '',
    state: '',
    country: '',
    pincode: '',
    gstNumber: '',
    panNumber: '',
    creditLimit: 0,
    creditDays: 0,
  };

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id');
    if (this.customerId) {
      this.isEditMode = true;
      this.loadCustomer(this.customerId);
    }
  }

  loadCustomer(id: string): void {
    this.loading = true;
    this.customerService.getById(id).subscribe({
      next: (response) => {
        if (response.success) {
          const data: Customer = response.data;
          this.customer = {
            customerCode: data.customerCode,
            companyName: data.companyName,
            contactPerson: data.contactPerson,
            email: data.email,
            phone: data.phone,
            mobile: data.mobile,
            address: data.address,
            city: data.city,
            state: data.state,
            country: data.country,
            pincode: data.pincode,
            gstNumber: data.gstNumber,
            panNumber: data.panNumber,
            creditLimit: data.creditLimit,
            creditDays: data.creditDays,
          };
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading customer:', error);
        alert('Failed to load customer');
        this.loading = false;
        this.router.navigate(['/sales/customers']);
      },
    });
  }

  onSubmit(): void {
    if (!this.validateForm()) {
      return;
    }

    this.saving = true;

    if (this.isEditMode && this.customerId) {
      const updateRequest: UpdateCustomerRequest = {
        companyName: this.customer.companyName,
        contactPerson: this.customer.contactPerson,
        email: this.customer.email,
        phone: this.customer.phone,
        mobile: this.customer.mobile,
        address: this.customer.address,
        city: this.customer.city,
        state: this.customer.state,
        country: this.customer.country,
        pincode: this.customer.pincode,
        gstNumber: this.customer.gstNumber,
        panNumber: this.customer.panNumber,
        creditLimit: this.customer.creditLimit,
        creditDays: this.customer.creditDays,
      };

      this.customerService.update(this.customerId, updateRequest).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Customer updated successfully');
            this.router.navigate(['/sales/customers']);
          } else {
            alert(response.message || 'Failed to update customer');
          }
          this.saving = false;
        },
        error: (error) => {
          console.error('Error updating customer:', error);
          alert(error.error?.message || 'Failed to update customer');
          this.saving = false;
        },
      });
    } else {
      this.customerService.create(this.customer).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Customer created successfully');
            this.router.navigate(['/sales/customers']);
          } else {
            alert(response.message || 'Failed to create customer');
          }
          this.saving = false;
        },
        error: (error) => {
          console.error('Error creating customer:', error);
          alert(error.error?.message || 'Failed to create customer');
          this.saving = false;
        },
      });
    }
  }

  validateForm(): boolean {
    if (!this.customer.customerCode?.trim()) {
      alert('Customer Code is required');
      return false;
    }
    if (!this.customer.companyName?.trim()) {
      alert('Company Name is required');
      return false;
    }
    if (!this.customer.contactPerson?.trim()) {
      alert('Contact Person is required');
      return false;
    }
    return true;
  }

  cancel(): void {
    this.router.navigate(['/sales/customers']);
  }
}
