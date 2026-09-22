export interface DashboardSummary {
  sales: SalesSummary;
  purchases: PurchaseSummary;
  inventory: InventorySummary;
  financial: FinancialSummary;
  recentActivities: RecentActivity[];
  topCustomers: TopCustomer[];
  topProducts: TopProduct[];
  monthlyTrend: MonthlyTrend[];
}

export interface SalesSummary {
  todayRevenue: number;
  monthRevenue: number;
  yearRevenue: number;
  todayOrders: number;
  monthOrders: number;
  pendingInvoices: number;
  outstandingAmount: number;
}

export interface PurchaseSummary {
  todayPurchases: number;
  monthPurchases: number;
  yearPurchases: number;
  todayPOs: number;
  monthPOs: number;
  pendingPOs: number;
  pendingGRNs: number;
}

export interface InventorySummary {
  totalProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalStockValue: number;
}

export interface FinancialSummary {
  totalRevenue: number;
  totalExpenses: number;
  netProfit: number;
  profitMargin: number;
  accountsReceivable: number;
  accountsPayable: number;
}

export interface RecentActivity {
  timestamp: Date;
  activityType: string;
  description: string;
  reference: string;
  amount?: number;
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

export interface MonthlyTrend {
  month: string;
  sales: number;
  purchases: number;
  profit: number;
}
