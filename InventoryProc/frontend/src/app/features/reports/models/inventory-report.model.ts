export interface InventoryReportRequest {
  categoryId?: string;
  brandId?: string;
  lowStockOnly?: boolean;
}

export interface InventoryReportResponse {
  totalProducts: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalStockValue: number;
  stockLevels: StockLevel[];
  lowStockItems: LowStockItem[];
  recentMovements: StockMovement[];
}

export interface StockLevel {
  productId: string;
  productCode: string;
  productName: string;
  categoryName: string;
  currentStock: number;
  minimumStock: number;
  unitPrice: number;
  stockValue: number;
}

export interface LowStockItem {
  productId: string;
  productCode: string;
  productName: string;
  currentStock: number;
  minimumStock: number;
  shortageQuantity: number;
}

export interface StockMovement {
  date: Date;
  productCode: string;
  productName: string;
  movementType: string;
  quantity: number;
  reference: string;
}
