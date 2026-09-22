import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { SalesOrderService } from '../../services/sales-order.service';
import { SalesOrder, SalesOrderStatus } from '../../models/sales-order.model';

@Component({
  selector: 'app-sales-order-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sales-order-detail.component.html',
  styleUrls: ['./sales-order-detail.component.scss']
})
export class SalesOrderDetailComponent implements OnInit {
  order: SalesOrder | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  SalesOrderStatus = SalesOrderStatus;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private salesOrderService: SalesOrderService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(id);
    }
  }

  loadOrder(id: string): void {
    this.isLoading = true;
    this.salesOrderService.getById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.order = response.data;
        } else {
          this.errorMessage = response.message || 'Failed to load order';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.errorMessage = 'Failed to load order';
        this.isLoading = false;
      }
    });
  }

  canConfirm(): boolean {
    return this.order?.status === SalesOrderStatus.Draft;
  }

  canShip(): boolean {
    return this.order?.status === SalesOrderStatus.Confirmed;
  }

  canDeliver(): boolean {
    return this.order?.status === SalesOrderStatus.Shipped;
  }

  canCancel(): boolean {
    return this.order?.status !== SalesOrderStatus.Cancelled &&
           this.order?.status !== SalesOrderStatus.Delivered;
  }

  canGenerateInvoice(): boolean {
    return this.order?.status === SalesOrderStatus.Confirmed ||
           this.order?.status === SalesOrderStatus.Shipped ||
           this.order?.status === SalesOrderStatus.Delivered;
  }

  confirmOrder(): void {
    if (!this.order || !this.canConfirm()) return;

    if (confirm('Are you sure you want to confirm this order?')) {
      this.isLoading = true;
      this.salesOrderService.confirm(this.order.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Order confirmed successfully';
            this.loadOrder(this.order!.id);
          } else {
            this.errorMessage = response.message || 'Failed to confirm order';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error confirming order:', error);
          this.errorMessage = error.error?.message || 'Failed to confirm order';
          this.isLoading = false;
        }
      });
    }
  }

  shipOrder(): void {
    if (!this.order || !this.canShip()) return;

    if (confirm('Are you sure you want to mark this order as shipped?')) {
      this.isLoading = true;
      this.salesOrderService.ship(this.order.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Order marked as shipped';
            this.loadOrder(this.order!.id);
          } else {
            this.errorMessage = response.message || 'Failed to ship order';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error shipping order:', error);
          this.errorMessage = error.error?.message || 'Failed to ship order';
          this.isLoading = false;
        }
      });
    }
  }

  deliverOrder(): void {
    if (!this.order || !this.canDeliver()) return;

    if (confirm('Are you sure you want to mark this order as delivered?')) {
      this.isLoading = true;
      this.salesOrderService.deliver(this.order.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Order marked as delivered';
            this.loadOrder(this.order!.id);
          } else {
            this.errorMessage = response.message || 'Failed to deliver order';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error delivering order:', error);
          this.errorMessage = error.error?.message || 'Failed to deliver order';
          this.isLoading = false;
        }
      });
    }
  }

  cancelOrder(): void {
    if (!this.order || !this.canCancel()) return;

    if (confirm('Are you sure you want to cancel this order? This action cannot be undone.')) {
      this.isLoading = true;
      this.salesOrderService.cancel(this.order.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Order cancelled successfully';
            this.loadOrder(this.order!.id);
          } else {
            this.errorMessage = response.message || 'Failed to cancel order';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error cancelling order:', error);
          this.errorMessage = error.error?.message || 'Failed to cancel order';
          this.isLoading = false;
        }
      });
    }
  }

  generateInvoice(): void {
    if (!this.order || !this.canGenerateInvoice()) return;

    // This will be implemented when invoice service is created
    this.router.navigate(['/sales/invoices/create-from-order', this.order.id]);
  }

  editOrder(): void {
    if (this.order && this.order.status === SalesOrderStatus.Draft) {
      this.router.navigate(['/sales/orders/edit', this.order.id]);
    }
  }

  deleteOrder(): void {
    if (!this.order) return;

    if (confirm('Are you sure you want to delete this order? This action cannot be undone.')) {
      this.isLoading = true;
      this.salesOrderService.delete(this.order.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/sales/orders']);
          } else {
            this.errorMessage = response.message || 'Failed to delete order';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error deleting order:', error);
          this.errorMessage = error.error?.message || 'Failed to delete order';
          this.isLoading = false;
        }
      });
    }
  }

  getStatusBadgeClass(status: SalesOrderStatus): string {
    switch (status) {
      case SalesOrderStatus.Draft:
        return 'badge-draft';
      case SalesOrderStatus.Confirmed:
        return 'badge-confirmed';
      case SalesOrderStatus.Shipped:
        return 'badge-shipped';
      case SalesOrderStatus.Delivered:
        return 'badge-delivered';
      case SalesOrderStatus.Cancelled:
        return 'badge-cancelled';
      default:
        return '';
    }
  }

  back(): void {
    this.router.navigate(['/sales/orders']);
  }
}
