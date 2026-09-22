export interface Product {
  id: string;
  name: string;
  code: string;
  description?: string;
  barcode?: string;
  categoryId: string;
  categoryName: string;
  brandId?: string;
  brandName?: string;
  unit: string;
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  currentStock: number;
  minStockLevel?: number;
  maxStockLevel?: number;
  taxType?: string;
  taxRate?: number;
  hsnCode?: string;
  isActive: boolean;
  isLowStock: boolean;
  isOutOfStock: boolean;
  createdAt: Date;
}

export interface CreateProductRequest {
  name: string;
  code: string;
  description?: string;
  barcode?: string;
  categoryId: string;
  brandId?: string;
  unit: string;
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  minStockLevel?: number;
  maxStockLevel?: number;
  taxType?: string;
  taxRate?: number;
  hsnCode?: string;
}

export interface UpdateProductRequest {
  name: string;
  description?: string;
  barcode?: string;
  categoryId: string;
  brandId?: string;
  unit: string;
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  minStockLevel?: number;
  maxStockLevel?: number;
  taxType?: string;
  taxRate?: number;
  hsnCode?: string;
}

export interface StockAdjustmentRequest {
  quantity: number;
  reason?: string;
}
