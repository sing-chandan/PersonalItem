import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GoodsReceiptNoteService } from '../../services/goods-receipt-note.service';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import { CreateGoodsReceiptNoteRequest, CreateGoodsReceiptNoteItemRequest } from '../../models/goods-receipt-note.model';
import { PurchaseOrder, PurchaseOrderStatus } from '../../models/purchase-order.model';

@Component({
  selector: 'app-grn-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './grn-form.component.html',
  styleUrls: ['./grn-form.component.css']
})
export class GRNFormComponent implements OnInit {
  grnForm: FormGroup;
  loading: boolean = false;
  error: string | null = null;
  submitted: boolean = false;

  purchaseOrder: PurchaseOrder | null = null;
  purchaseOrders: PurchaseOrder[] = [];

  constructor(
    private fb: FormBuilder,
    private grnService: GoodsReceiptNoteService,
    private purchaseOrderService: PurchaseOrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.grnForm = this.fb.group({
      grnDate: [new Date().toISOString().substring(0, 10), Validators.required],
      purchaseOrderId: ['', Validators.required],
      receivedBy: ['', Validators.required],
      notes: [''],
      items: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.loadPurchaseOrders();

    const purchaseOrderId = this.route.snapshot.queryParamMap.get('purchaseOrderId');
    if (purchaseOrderId) {
      this.grnForm.patchValue({ purchaseOrderId });
      this.onPurchaseOrderChange();
    }
  }

  get items(): FormArray {
    return this.grnForm.get('items') as FormArray;
  }

  loadPurchaseOrders(): void {
    this.purchaseOrderService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.purchaseOrders = response.data.filter(po =>
            po.status === PurchaseOrderStatus.Approved ||
            po.status === PurchaseOrderStatus.PartiallyReceived
          );
        }
      },
      error: (err) => {
        console.error('Error loading purchase orders:', err);
      }
    });
  }

  onPurchaseOrderChange(): void {
    const purchaseOrderId = this.grnForm.get('purchaseOrderId')?.value;
    if (!purchaseOrderId) {
      this.purchaseOrder = null;
      this.items.clear();
      return;
    }

    this.loading = true;
    this.purchaseOrderService.getById(purchaseOrderId).subscribe({
      next: (response) => {
        if (response.success) {
          this.purchaseOrder = response.data;
          this.populateItems();
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

  populateItems(): void {
    if (!this.purchaseOrder) return;

    this.items.clear();

    this.purchaseOrder.items.forEach(item => {
      const pendingQuantity = item.quantity - item.receivedQuantity;

      if (pendingQuantity > 0) {
        const itemGroup = this.fb.group({
          purchaseOrderItemId: [item.id, Validators.required],
          productCode: [item.productCode],
          productName: [item.productName],
          orderedQuantity: [item.quantity],
          receivedQuantity: [item.receivedQuantity],
          pendingQuantity: [pendingQuantity],
          receivedNow: [pendingQuantity, [Validators.required, Validators.min(0)]],
          acceptedQuantity: [pendingQuantity, [Validators.required, Validators.min(0)]],
          rejectedQuantity: [0, [Validators.required, Validators.min(0)]],
          remarks: ['']
        });

        itemGroup.get('receivedNow')?.valueChanges.subscribe(() => {
          this.updateAcceptedRejected(itemGroup);
        });

        this.items.push(itemGroup);
      }
    });
  }

  updateAcceptedRejected(itemGroup: FormGroup): void {
    const receivedNow = itemGroup.get('receivedNow')?.value || 0;
    const rejectedQuantity = itemGroup.get('rejectedQuantity')?.value || 0;
    const acceptedQuantity = receivedNow - rejectedQuantity;

    itemGroup.patchValue({
      acceptedQuantity: Math.max(0, acceptedQuantity)
    }, { emitEvent: false });
  }

  onRejectedChange(itemGroup: FormGroup): void {
    this.updateAcceptedRejected(itemGroup);
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.grnForm.invalid || this.items.length === 0) {
      return;
    }

    this.loading = true;
    this.error = null;

    const formValue = this.grnForm.value;

    const request: CreateGoodsReceiptNoteRequest = {
      grnDate: new Date(formValue.grnDate),
      purchaseOrderId: formValue.purchaseOrderId,
      receivedBy: formValue.receivedBy,
      notes: formValue.notes,
      items: formValue.items.map((item: any) => ({
        purchaseOrderItemId: item.purchaseOrderItemId,
        receivedQuantity: item.receivedNow,
        acceptedQuantity: item.acceptedQuantity,
        rejectedQuantity: item.rejectedQuantity,
        remarks: item.remarks
      } as CreateGoodsReceiptNoteItemRequest))
    };

    this.grnService.create(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/purchases/goods-receipt-notes']);
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'Failed to create GRN. Please try again.';
        this.loading = false;
        console.error('Error creating GRN:', err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases/goods-receipt-notes']);
  }

  get f() {
    return this.grnForm.controls;
  }
}
