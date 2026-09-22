export enum GRNStatus {
  Draft = 0,
  Completed = 1,
  Cancelled = 2
}

export interface GoodsReceiptNoteItem {
  id: string;
  goodsReceiptNoteId: string;
  purchaseOrderItemId: string;
  productId: string;
  productName: string;
  productCode: string;
  orderedQuantity: number;
  receivedQuantity: number;
  acceptedQuantity: number;
  rejectedQuantity: number;
  remarks?: string;
}

export interface GoodsReceiptNote {
  id: string;
  tenantId: string;
  grnNumber: string;
  grnDate: Date;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  status: GRNStatus;
  items: GoodsReceiptNoteItem[];
  receivedBy: string;
  completedDate?: Date;
  notes?: string;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface CreateGoodsReceiptNoteItemRequest {
  purchaseOrderItemId: string;
  receivedQuantity: number;
  acceptedQuantity: number;
  rejectedQuantity: number;
  remarks?: string;
}

export interface CreateGoodsReceiptNoteRequest {
  grnDate: Date;
  purchaseOrderId: string;
  items: CreateGoodsReceiptNoteItemRequest[];
  receivedBy: string;
  notes?: string;
}

export function getGRNStatusLabel(status: GRNStatus): string {
  switch (status) {
    case GRNStatus.Draft:
      return 'Draft';
    case GRNStatus.Completed:
      return 'Completed';
    case GRNStatus.Cancelled:
      return 'Cancelled';
    default:
      return 'Unknown';
  }
}

export function getGRNStatusClass(status: GRNStatus): string {
  switch (status) {
    case GRNStatus.Draft:
      return 'badge bg-secondary';
    case GRNStatus.Completed:
      return 'badge bg-success';
    case GRNStatus.Cancelled:
      return 'badge bg-danger';
    default:
      return 'badge bg-secondary';
  }
}
