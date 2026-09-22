import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import { VendorService } from '../../services/vendor.service';
import { Vendor } from '../../models/vendor.model';
import {
  CreatePurchaseOrderRequest,
  UpdatePurchaseOrderRequest,
  CreatePurchaseOrderItemRequest,
  UpdatePurchaseOrderItemRequest
} from '../../models/purchase-order.model';
import { HttpClient } from '@angular/common/http';

interface Product {
  id: string;
  code: string;
  name: string;
  currentStock: number;
  unitPrice: number;
}

@Component({
  selector: 'app-purchase-order-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './purchase-order-form.component.html',
  styleUrls: ['./purchase-order-form.component.css']
})
export class PurchaseOrderFormComponent implements OnInit {
  orderForm: FormGroup;
  isEditMode: boolean = false;
  orderId: string | null = null;
  loading: boolean = false;
  error: string | null = null;
  submitted: boolean = false;

  vendors: Vendor[] = [];
  products: Product[] = [];

  constructor(
    private fb: FormBuilder,
    private purchaseOrderService: PurchaseOrderService,
    private vendorService: VendorService,
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.orderForm = this.fb.group({
      orderDate: [new Date().toISOString().substring(0, 10), Validators.required],
      vendorId: ['', Validators.required],
      expectedDeliveryDate: [''],
      notes: [''],
      items: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.orderId && this.orderId !== 'new';

    this.loadVendors();
    this.loadProducts();

    if (this.isEditMode) {
      this.loadPurchaseOrder();
    } else {
      this.addItem();
    }
  }

  get items(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }

  createItemFormGroup(): FormGroup {
    return this.fb.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      taxAmount: [0, [Validators.required, Validators.min(0)]]
    });
  }

  addItem(): void {
    this.items.push(this.createItemFormGroup());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  loadVendors(): void {
    this.vendorService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.vendors = response.data.filter(v => v.isActive);
        }
      },
      error: (err) => {
        console.error('Error loading vendors:', err);
      }
    });
  }

  loadProducts(): void {
    this.http.get<any>('http://localhost:5005/api/products').subscribe({
      next: (response) => {
        if (response.success) {
          this.products = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading products:', err);
      }
    });
  }

  onProductChange(index: number): void {
    const item = this.items.at(index);
    const productId = item.get('productId')?.value;

    if (productId) {
      const product = this.products.find(p => p.id === productId);
      if (product) {
        item.patchValue({
          unitPrice: product.unitPrice,
          taxAmount: 0
        });
      }
    }
  }

  getItemTotal(index: number): number {
    const item = this.items.at(index);
    const quantity = item.get('quantity')?.value || 0;
    const unitPrice = item.get('unitPrice')?.value || 0;
    const taxAmount = item.get('taxAmount')?.value || 0;
    return (quantity * unitPrice) + taxAmount;
  }

  getSubTotal(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) {
      const item = this.items.at(i);
      const quantity = item.get('quantity')?.value || 0;
      const unitPrice = item.get('unitPrice')?.value || 0;
      total += quantity * unitPrice;
    }
    return total;
  }

  getTotalTax(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) {
      const item = this.items.at(i);
      const taxAmount = item.get('taxAmount')?.value || 0;
      total += taxAmount;
    }
    return total;
  }

  getGrandTotal(): number {
    return this.getSubTotal() + this.getTotalTax();
  }

  loadPurchaseOrder(): void {
    if (!this.orderId) return;

    this.loading = true;
    this.purchaseOrderService.getById(this.orderId).subscribe({
      next: (response) => {
        if (response.success) {
          const order = response.data;

          this.orderForm.patchValue({
            orderDate: new Date(order.orderDate).toISOString().substring(0, 10),
            vendorId: order.vendorId,
            expectedDeliveryDate: order.expectedDeliveryDate
              ? new Date(order.expectedDeliveryDate).toISOString().substring(0, 10)
              : '',
            notes: order.notes
          });

          this.items.clear();
          order.items.forEach(item => {
            const itemGroup = this.fb.group({
              productId: [item.productId, Validators.required],
              quantity: [item.quantity, [Validators.required, Validators.min(1)]],
              unitPrice: [item.unitPrice, [Validators.required, Validators.min(0)]],
              taxAmount: [item.taxAmount, [Validators.required, Validators.min(0)]]
            });
            this.items.push(itemGroup);
          });
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

  onSubmit(): void {
    this.submitted = true;

    if (this.orderForm.invalid || this.items.length === 0) {
      return;
    }

    this.loading = true;
    this.error = null;

    if (this.isEditMode) {
      this.updatePurchaseOrder();
    } else {
      this.createPurchaseOrder();
    }
  }

  createPurchaseOrder(): void {
    const formValue = this.orderForm.value;

    const request: CreatePurchaseOrderRequest = {
      orderDate: new Date(formValue.orderDate),
      vendorId: formValue.vendorId,
      items: formValue.items.map((item: any) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        taxAmount: item.taxAmount
      } as CreatePurchaseOrderItemRequest)),
      expectedDeliveryDate: formValue.expectedDeliveryDate ? new Date(formValue.expectedDeliveryDate) : undefined,
      notes: formValue.notes
    };

    this.purchaseOrderService.create(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/purchases/purchase-orders']);
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'Failed to create purchase order. Please try again.';
        this.loading = false;
        console.error('Error creating purchase order:', err);
      }
    });
  }

  updatePurchaseOrder(): void {
    if (!this.orderId) return;

    const formValue = this.orderForm.value;

    const request: UpdatePurchaseOrderRequest = {
      orderDate: new Date(formValue.orderDate),
      vendorId: formValue.vendorId,
      items: formValue.items.map((item: any) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        taxAmount: item.taxAmount
      } as UpdatePurchaseOrderItemRequest)),
      expectedDeliveryDate: formValue.expectedDeliveryDate ? new Date(formValue.expectedDeliveryDate) : undefined,
      notes: formValue.notes
    };

    this.purchaseOrderService.update(this.orderId, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/purchases/purchase-orders']);
        } else {
          this.error = response.message;
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'Failed to update purchase order. Please try again.';
        this.loading = false;
        console.error('Error updating purchase order:', err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases/purchase-orders']);
  }

  get f() {
    return this.orderForm.controls;
  }

  getProductName(productId: string): string {
    const product = this.products.find(p => p.id === productId);
    return product ? `${product.code} - ${product.name}` : '';
  }
}
