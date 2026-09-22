export interface SalesReportRequest {
  startDate?: Date;
  endDate?: Date;
  customerId?: string;
  productId?: string;
}

export interface SalesReportResponse {
  totalRevenue: number;
  totalOrders: number;
  totalInvoices: number;
  totalPaid: number;
  totalOutstanding: number;
  salesByPeriod: SalesByPeriod[];
  topCustomers: TopCustomer[];
  topProducts: TopProduct[];
}

export interface SalesByPeriod {
  date: Date;
  revenue: number;
  orderCount: number;
}

export interface TopCustomer {
  customerId: string;
  customerName: string;
  email: string;
  totalRevenue: number;
  orderCount: number;
}

export interface TopProduct {
  productId: string;
  productCode: string;
  productName: string;
  quantitySold: number;
  revenue: number;
}
