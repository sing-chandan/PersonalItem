import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import {
  PurchaseOrder,
  PurchaseOrderStatus,
  getPurchaseOrderStatusLabel,
  getPurchaseOrderStatusClass
} from '../../models/purchase-order.model';

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './purchase-order-list.component.html',
  styleUrls: ['./purchase-order-list.component.css']
})
export class PurchaseOrderListComponent implements OnInit {
  purchaseOrders: PurchaseOrder[] = [];
  filteredOrders: PurchaseOrder[] = [];
  searchTerm: string = '';
  statusFilter: PurchaseOrderStatus | null = null;
  loading: boolean = false;
  error: string | null = null;

  PurchaseOrderStatus = PurchaseOrderStatus;

  constructor(
    private purchaseOrderService: PurchaseOrderService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPurchaseOrders();
  }

  loadPurchaseOrders(): void {
    this.loading = true;
    this.error = null;

    this.purchaseOrderService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.purchaseOrders = response.data;
          this.filteredOrders = this.purchaseOrders;
          this.applyFilters();
        } else {
          this.error = response.message;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load purchase orders. Please try again.';
        this.loading = false;
        console.error('Error loading purchase orders:', err);
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    let filtered = this.purchaseOrders;

    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(order =>
        order.orderNumber.toLowerCase().includes(term) ||
        order.vendorName.toLowerCase().includes(term)
      );
    }

    if (this.statusFilter !== null) {
      filtered = filtered.filter(order => order.status === this.statusFilter);
    }

    this.filteredOrders = filtered;
    this.cdr.detectChanges();
  }

  onStatusFilterChange(event: any): void {
    const value = event.target.value;
    this.statusFilter = value === '' ? null : parseInt(value);
    this.applyFilters();
  }

  getStatusLabel(status: PurchaseOrderStatus): string {
    return getPurchaseOrderStatusLabel(status);
  }

  getStatusClass(status: PurchaseOrderStatus): string {
    return getPurchaseOrderStatusClass(status);
  }

  approvePurchaseOrder(id: string, orderNumber: string): void {
    if (!confirm(`Are you sure you want to approve purchase order "${orderNumber}"?`)) {
      return;
    }

    this.purchaseOrderService.approve(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrders();
        } else {
          alert(`Failed to approve purchase order: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to approve purchase order. Please try again.');
        console.error('Error approving purchase order:', err);
      }
    });
  }

  cancelPurchaseOrder(id: string, orderNumber: string): void {
    if (!confirm(`Are you sure you want to cancel purchase order "${orderNumber}"?`)) {
      return;
    }

    this.purchaseOrderService.cancel(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrders();
        } else {
          alert(`Failed to cancel purchase order: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to cancel purchase order. Please try again.');
        console.error('Error canceling purchase order:', err);
      }
    });
  }

  closePurchaseOrder(id: string, orderNumber: string): void {
    if (!confirm(`Are you sure you want to close purchase order "${orderNumber}"?`)) {
      return;
    }

    this.purchaseOrderService.close(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrders();
        } else {
          alert(`Failed to close purchase order: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to close purchase order. Please try again.');
        console.error('Error closing purchase order:', err);
      }
    });
  }

  deletePurchaseOrder(id: string, orderNumber: string): void {
    if (!confirm(`Are you sure you want to delete purchase order "${orderNumber}"?`)) {
      return;
    }

    this.purchaseOrderService.delete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrders();
        } else {
          alert(`Failed to delete purchase order: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to delete purchase order. Please try again.');
        console.error('Error deleting purchase order:', err);
      }
    });
  }
}
