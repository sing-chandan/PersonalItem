import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { SalesOrderService } from '../../services/sales-order.service';
import { CustomerService } from '../../services/customer.service';
import { ProductService } from '../../../products/services/product.service';
import { CreateSalesOrderRequest, CreateSalesOrderItemRequest } from '../../models/sales-order.model';

@Component({
  selector: 'app-sales-order-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './sales-order-form.component.html',
  styleUrls: ['./sales-order-form.component.scss']
})
export class SalesOrderFormComponent implements OnInit {
  orderForm!: FormGroup;
  customers: any[] = [];
  products: any[] = [];
  filteredProducts: any[] = [];
  isEditMode = false;
  orderId: string | null = null;
  isLoading = false;
  errorMessage = '';
  searchTerm = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private salesOrderService: SalesOrderService,
    private customerService: CustomerService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadCustomers();
    this.loadProducts();

    this.orderId = this.route.snapshot.paramMap.get('id');
    if (this.orderId) {
      this.isEditMode = true;
      this.loadOrderForEdit(this.orderId);
    } else {
      // Add one empty line item for new orders
      this.addLineItem();
    }
  }

  initializeForm(): void {
    this.orderForm = this.fb.group({
      orderNumber: ['', Validators.required],
      orderDate: [new Date().toISOString().split('T')[0], Validators.required],
      customerId: ['', Validators.required],
      customerName: [''],
      notes: [''],
      shippingAddress: [''],
      billingAddress: [''],
      items: this.fb.array([])
    });

    // Auto-populate customer name when customer is selected
    this.orderForm.get('customerId')?.valueChanges.subscribe(customerId => {
      const customer = this.customers.find(c => c.id === customerId);
      if (customer) {
        this.orderForm.patchValue({
          customerName: customer.companyName,
          shippingAddress: customer.address || '',
          billingAddress: customer.address || ''
        });
      }
    });
  }

  get items(): FormArray {
    return this.orderForm.get('items') as FormArray;
  }

  createLineItem(item?: any): FormGroup {
    const lineItem = this.fb.group({
      productId: [item?.productId || '', Validators.required],
      productName: [item?.productName || ''],
      productCode: [item?.productCode || ''],
      quantity: [item?.quantity || 1, [Validators.required, Validators.min(1)]],
      unit: [item?.unit || 'PCS'],
      unitPrice: [item?.unitPrice || 0, [Validators.required, Validators.min(0)]],
      taxRate: [item?.taxRate || 0, [Validators.min(0), Validators.max(100)]],
      notes: [item?.notes || '']
    });

    // Auto-populate product details when product is selected
    lineItem.get('productId')?.valueChanges.subscribe(productId => {
      const product = this.products.find(p => p.id === productId);
      if (product) {
        lineItem.patchValue({
          productName: product.name,
          productCode: product.code,
          unit: product.unit,
          unitPrice: product.salePrice,
          taxRate: product.taxRate || 0
        });
      }
    });

    // Recalculate totals when quantity or price changes
    lineItem.valueChanges.subscribe(() => {
      this.calculateTotals();
    });

    return lineItem;
  }

  addLineItem(): void {
    this.items.push(this.createLineItem());
  }

  removeLineItem(index: number): void {
    if (this.items.length > 1) {
      this.items.removeAt(index);
      this.calculateTotals();
    }
  }

  loadCustomers(): void {
    this.customerService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.customers = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading customers:', error);
      }
    });
  }

  loadProducts(): void {
    this.productService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.products = response.data;
          this.filteredProducts = this.products;
        }
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });
  }

  searchProducts(): void {
    if (this.searchTerm.trim()) {
      this.productService.search(this.searchTerm).subscribe({
        next: (response) => {
          if (response.success) {
            this.filteredProducts = response.data;
          }
        },
        error: (error) => {
          console.error('Error searching products:', error);
        }
      });
    } else {
      this.filteredProducts = this.products;
    }
  }

  loadOrderForEdit(id: string): void {
    this.isLoading = true;
    this.salesOrderService.getById(id).subscribe({
      next: (response) => {
        if (response.success) {
          const order = response.data;

          // Populate form
          this.orderForm.patchValue({
            orderNumber: order.orderNumber,
            orderDate: order.orderDate.split('T')[0],
            customerId: order.customerId,
            customerName: order.customerName,
            notes: order.notes,
            shippingAddress: order.shippingAddress,
            billingAddress: order.billingAddress
          });

          // Populate line items
          order.items.forEach((item: any) => {
            this.items.push(this.createLineItem(item));
          });

          this.calculateTotals();
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

  calculateTotals(): { subTotal: number; taxAmount: number; totalAmount: number } {
    let subTotal = 0;
    let taxAmount = 0;

    this.items.controls.forEach(control => {
      const quantity = control.get('quantity')?.value || 0;
      const unitPrice = control.get('unitPrice')?.value || 0;
      const taxRate = control.get('taxRate')?.value || 0;

      const lineTotal = quantity * unitPrice;
      const lineTax = lineTotal * (taxRate / 100);

      subTotal += lineTotal;
      taxAmount += lineTax;
    });

    const totalAmount = subTotal + taxAmount;

    return { subTotal, taxAmount, totalAmount };
  }

  getLineTotal(index: number): number {
    const item = this.items.at(index);
    const quantity = item.get('quantity')?.value || 0;
    const unitPrice = item.get('unitPrice')?.value || 0;
    return quantity * unitPrice;
  }

  getLineTax(index: number): number {
    const item = this.items.at(index);
    const quantity = item.get('quantity')?.value || 0;
    const unitPrice = item.get('unitPrice')?.value || 0;
    const taxRate = item.get('taxRate')?.value || 0;
    const lineTotal = quantity * unitPrice;
    return lineTotal * (taxRate / 100);
  }

  onSubmit(): void {
    if (this.orderForm.invalid) {
      this.errorMessage = 'Please fill all required fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formValue = this.orderForm.value;
    const request: CreateSalesOrderRequest = {
      orderNumber: formValue.orderNumber,
      orderDate: formValue.orderDate,
      customerId: formValue.customerId,
      customerName: formValue.customerName,
      notes: formValue.notes,
      shippingAddress: formValue.shippingAddress,
      billingAddress: formValue.billingAddress,
      items: formValue.items.map((item: any) => ({
        productId: item.productId,
        productName: item.productName,
        productCode: item.productCode,
        quantity: item.quantity,
        unit: item.unit,
        unitPrice: item.unitPrice,
        taxRate: item.taxRate,
        notes: item.notes
      } as CreateSalesOrderItemRequest))
    };

    const operation = this.isEditMode && this.orderId
      ? this.salesOrderService.update(this.orderId, request)
      : this.salesOrderService.create(request);

    operation.subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/sales/orders']);
        } else {
          this.errorMessage = response.message || 'Failed to save order';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error saving order:', error);
        this.errorMessage = error.error?.message || 'Failed to save order';
        this.isLoading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/sales/orders']);
  }
}
