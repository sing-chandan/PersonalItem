import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import { GoodsReceiptNoteService } from '../../services/goods-receipt-note.service';
import {
  PurchaseOrder,
  PurchaseOrderStatus,
  getPurchaseOrderStatusLabel,
  getPurchaseOrderStatusClass
} from '../../models/purchase-order.model';
import { GoodsReceiptNote } from '../../models/goods-receipt-note.model';

@Component({
  selector: 'app-purchase-order-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './purchase-order-detail.component.html',
  styleUrls: ['./purchase-order-detail.component.css']
})
export class PurchaseOrderDetailComponent implements OnInit {
  purchaseOrder: PurchaseOrder | null = null;
  grns: GoodsReceiptNote[] = [];
  loading: boolean = false;
  error: string | null = null;

  PurchaseOrderStatus = PurchaseOrderStatus;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private purchaseOrderService: PurchaseOrderService,
    private grnService: GoodsReceiptNoteService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPurchaseOrder(id);
      this.loadGRNs(id);
    }
  }

  loadPurchaseOrder(id: string): void {
    this.loading = true;
    this.error = null;

    this.purchaseOrderService.getById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.purchaseOrder = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load purchase order. Please try again.';
        this.loading = false;
        console.error('Error loading purchase order:', err);
      }
    });
  }

  loadGRNs(purchaseOrderId: string): void {
    this.grnService.getByPurchaseOrder(purchaseOrderId).subscribe({
      next: (response) => {
        if (response.success) {
          this.grns = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading GRNs:', err);
      }
    });
  }

  getStatusLabel(status: PurchaseOrderStatus): string {
    return getPurchaseOrderStatusLabel(status);
  }

  getStatusClass(status: PurchaseOrderStatus): string {
    return getPurchaseOrderStatusClass(status);
  }

  approvePurchaseOrder(): void {
    if (!this.purchaseOrder || !confirm('Are you sure you want to approve this purchase order?')) {
      return;
    }

    this.purchaseOrderService.approve(this.purchaseOrder.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrder(this.purchaseOrder!.id);
        } else {
          alert(`Failed to approve: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to approve purchase order. Please try again.');
        console.error('Error approving purchase order:', err);
      }
    });
  }

  closePurchaseOrder(): void {
    if (!this.purchaseOrder || !confirm('Are you sure you want to close this purchase order?')) {
      return;
    }

    this.purchaseOrderService.close(this.purchaseOrder.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrder(this.purchaseOrder!.id);
        } else {
          alert(`Failed to close: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to close purchase order. Please try again.');
        console.error('Error closing purchase order:', err);
      }
    });
  }

  cancelPurchaseOrder(): void {
    if (!this.purchaseOrder || !confirm('Are you sure you want to cancel this purchase order?')) {
      return;
    }

    this.purchaseOrderService.cancel(this.purchaseOrder.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadPurchaseOrder(this.purchaseOrder!.id);
        } else {
          alert(`Failed to cancel: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to cancel purchase order. Please try again.');
        console.error('Error canceling purchase order:', err);
      }
    });
  }

  createGRN(): void {
    if (!this.purchaseOrder) return;
    this.router.navigate(['/purchases/goods-receipt-notes/new'], {
      queryParams: { purchaseOrderId: this.purchaseOrder.id }
    });
  }

  getReceivedPercentage(item: any): number {
    if (item.quantity === 0) return 0;
    return (item.receivedQuantity / item.quantity) * 100;
  }

  getReceivedPercentageClass(item: any): string {
    const percentage = this.getReceivedPercentage(item);
    if (percentage === 0) return 'bg-secondary';
    if (percentage < 100) return 'bg-warning';
    return 'bg-success';
  }

  back(): void {
    this.router.navigate(['/purchases/purchase-orders']);
  }
}
