export interface Brand {
  id: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  productCount: number;
  createdAt: Date;
}

export interface CreateBrandRequest {
  name: string;
  code: string;
  description?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
}
