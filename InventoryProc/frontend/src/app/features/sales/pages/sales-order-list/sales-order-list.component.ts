import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SalesOrderService } from '../../services/sales-order.service';
import { SalesOrder, SalesOrderStatus } from '../../models/sales-order.model';

@Component({
  selector: 'app-sales-order-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sales-order-list.component.html',
  styleUrls: ['./sales-order-list.component.scss'],
})
export class SalesOrderListComponent implements OnInit {
  orders: SalesOrder[] = [];
  loading = false;
  SalesOrderStatus = SalesOrderStatus;

  constructor(
    private salesOrderService: SalesOrderService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.salesOrderService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.orders = response.data;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        alert('Failed to load orders');
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  getStatusClass(status: SalesOrderStatus): string {
    switch (status) {
      case SalesOrderStatus.Draft:
        return 'status-draft';
      case SalesOrderStatus.Confirmed:
        return 'status-confirmed';
      case SalesOrderStatus.Shipped:
        return 'status-shipped';
      case SalesOrderStatus.Delivered:
        return 'status-delivered';
      case SalesOrderStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  confirmOrder(order: SalesOrder): void {
    if (!confirm(`Confirm order ${order.orderNumber}?`)) {
      return;
    }

    this.salesOrderService.confirm(order.id).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Order confirmed successfully');
          this.loadOrders();
        }
      },
      error: (error) => {
        console.error('Error confirming order:', error);
        alert('Failed to confirm order');
      },
    });
  }

  cancelOrder(order: SalesOrder): void {
    if (!confirm(`Cancel order ${order.orderNumber}?`)) {
      return;
    }

    this.salesOrderService.cancel(order.id).subscribe({
      next: (response) => {
        if (response.success) {
          alert('Order cancelled successfully');
          this.loadOrders();
        }
      },
      error: (error) => {
        console.error('Error cancelling order:', error);
        alert('Failed to cancel order');
      },
    });
  }
}
