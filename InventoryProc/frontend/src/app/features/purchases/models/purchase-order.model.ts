export enum PurchaseOrderStatus {
  Draft = 0,
  Approved = 1,
  PartiallyReceived = 2,
  FullyReceived = 3,
  Closed = 4,
  Cancelled = 5
}

export interface PurchaseOrderItem {
  id: string;
  purchaseOrderId: string;
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  receivedQuantity: number;
  unitPrice: number;
  taxAmount: number;
  totalAmount: number;
}

export interface PurchaseOrder {
  id: string;
  tenantId: string;
  orderNumber: string;
  orderDate: Date;
  vendorId: string;
  vendorName: string;
  status: PurchaseOrderStatus;
  items: PurchaseOrderItem[];
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  expectedDeliveryDate?: Date;
  approvedBy?: string;
  approvedDate?: Date;
  notes?: string;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface CreatePurchaseOrderItemRequest {
  productId: string;
  quantity: number;
  unitPrice: number;
  taxAmount: number;
}

export interface CreatePurchaseOrderRequest {
  orderDate: Date;
  vendorId: string;
  items: CreatePurchaseOrderItemRequest[];
  expectedDeliveryDate?: Date;
  notes?: string;
}

export interface UpdatePurchaseOrderItemRequest {
  productId: string;
  quantity: number;
  unitPrice: number;
  taxAmount: number;
}

export interface UpdatePurchaseOrderRequest {
  orderDate: Date;
  vendorId: string;
  items: UpdatePurchaseOrderItemRequest[];
  expectedDeliveryDate?: Date;
  notes?: string;
}

export interface ApproveOrderRequest {
  approvedBy: string;
}

export interface PurchaseOrderApplyDiscountRequest {
  discountAmount: number;
}

export function getPurchaseOrderStatusLabel(status: PurchaseOrderStatus): string {
  switch (status) {
    case PurchaseOrderStatus.Draft:
      return 'Draft';
    case PurchaseOrderStatus.Approved:
      return 'Approved';
    case PurchaseOrderStatus.PartiallyReceived:
      return 'Partially Received';
    case PurchaseOrderStatus.FullyReceived:
      return 'Fully Received';
    case PurchaseOrderStatus.Closed:
      return 'Closed';
    case PurchaseOrderStatus.Cancelled:
      return 'Cancelled';
    default:
      return 'Unknown';
  }
}

export function getPurchaseOrderStatusClass(status: PurchaseOrderStatus): string {
  switch (status) {
    case PurchaseOrderStatus.Draft:
      return 'badge bg-secondary';
    case PurchaseOrderStatus.Approved:
      return 'badge bg-primary';
    case PurchaseOrderStatus.PartiallyReceived:
      return 'badge bg-warning';
    case PurchaseOrderStatus.FullyReceived:
      return 'badge bg-info';
    case PurchaseOrderStatus.Closed:
      return 'badge bg-success';
    case PurchaseOrderStatus.Cancelled:
      return 'badge bg-danger';
    default:
      return 'badge bg-secondary';
  }
}
