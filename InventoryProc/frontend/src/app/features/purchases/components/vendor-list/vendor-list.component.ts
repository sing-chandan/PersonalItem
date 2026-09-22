import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VendorService } from '../../services/vendor.service';
import { Vendor } from '../../models/vendor.model';

@Component({
  selector: 'app-vendor-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './vendor-list.component.html',
  styleUrls: ['./vendor-list.component.css']
})
export class VendorListComponent implements OnInit {
  vendors: Vendor[] = [];
  filteredVendors: Vendor[] = [];
  searchTerm: string = '';
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private vendorService: VendorService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadVendors();
  }

  loadVendors(): void {
    this.loading = true;
    this.error = null;

    this.vendorService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.vendors = response.data;
          this.filteredVendors = this.vendors;
        } else {
          this.error = response.message;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load vendors. Please try again.';
        this.loading = false;
        console.error('Error loading vendors:', err);
        this.cdr.detectChanges();
      }
    });
  }

  filterVendors(): void {
    if (!this.searchTerm.trim()) {
      this.filteredVendors = this.vendors;
      this.cdr.detectChanges();
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredVendors = this.vendors.filter(vendor =>
      vendor.code.toLowerCase().includes(term) ||
      vendor.companyName.toLowerCase().includes(term) ||
      vendor.email.toLowerCase().includes(term) ||
      vendor.phone.includes(term)
    );
    this.cdr.detectChanges();
  }

  deleteVendor(id: string, companyName: string): void {
    if (!confirm(`Are you sure you want to delete vendor "${companyName}"?`)) {
      return;
    }

    this.vendorService.delete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadVendors();
        } else {
          alert(`Failed to delete vendor: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to delete vendor. Please try again.');
        console.error('Error deleting vendor:', err);
      }
    });
  }

  getCreditStatus(vendor: Vendor): string {
    if (vendor.outstandingBalance >= vendor.creditLimit) {
      return 'Over Limit';
    }
    const utilization = (vendor.outstandingBalance / vendor.creditLimit) * 100;
    if (utilization >= 80) {
      return 'Near Limit';
    }
    return 'Good';
  }

  getCreditStatusClass(vendor: Vendor): string {
    const status = this.getCreditStatus(vendor);
    switch (status) {
      case 'Over Limit':
        return 'badge bg-danger';
      case 'Near Limit':
        return 'badge bg-warning';
      default:
        return 'badge bg-success';
    }
  }
}
