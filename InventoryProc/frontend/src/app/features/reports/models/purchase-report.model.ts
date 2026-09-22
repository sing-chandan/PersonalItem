export interface PurchaseReportRequest {
  startDate?: Date;
  endDate?: Date;
  vendorId?: string;
  productId?: string;
}

export interface PurchaseReportResponse {
  totalPurchases: number;
  totalPurchaseOrders: number;
  totalGRNs: number;
  totalReceived: number;
  totalPending: number;
  purchasesByPeriod: PurchasesByPeriod[];
  topVendors: TopVendor[];
  topProducts: TopPurchaseProduct[];
}

export interface PurchasesByPeriod {
  date: Date;
  purchaseAmount: number;
  orderCount: number;
}

export interface TopVendor {
  vendorId: string;
  vendorName: string;
  email: string;
  totalPurchases: number;
  orderCount: number;
  outstandingBalance: number;
}

export interface TopPurchaseProduct {
  productId: string;
  productCode: string;
  productName: string;
  quantityPurchased: number;
  purchaseAmount: number;
}
