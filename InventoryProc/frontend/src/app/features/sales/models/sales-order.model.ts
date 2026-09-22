export interface SalesOrder {
  id: string;
  tenantId: string;
  orderNumber: string;
  orderDate: string;
  customerId: string;
  customerName: string;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  notes?: string;
  status: SalesOrderStatus;
  shippingAddress?: string;
  billingAddress?: string;
  shippedDate?: string;
  deliveredDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  items: SalesOrderItem[];
}

export interface SalesOrderItem {
  id: string;
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  totalPrice: number;
  taxRate: number;
  taxAmount: number;
  notes?: string;
}

export interface CreateSalesOrderRequest {
  orderNumber: string;
  orderDate: string;
  customerId: string;
  customerName: string;
  notes?: string;
  shippingAddress?: string;
  billingAddress?: string;
  items: CreateSalesOrderItemRequest[];
}

export interface CreateSalesOrderItemRequest {
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  taxRate: number;
  notes?: string;
}

export interface UpdateSalesOrderRequest {
  orderDate: string;
  notes?: string;
  shippingAddress?: string;
  billingAddress?: string;
  items: UpdateSalesOrderItemRequest[];
}

export interface UpdateSalesOrderItemRequest {
  id?: string;
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  taxRate: number;
  notes?: string;
}

export enum SalesOrderStatus {
  Draft = 'Draft',
  Confirmed = 'Confirmed',
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled',
}
