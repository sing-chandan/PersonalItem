export interface Category {
  id: string;
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  isActive: boolean;
  productCount: number;
  createdAt: Date;
}

export interface CreateCategoryRequest {
  name: string;
  code: string;
  description?: string;
  parentCategoryId?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  parentCategoryId?: string;
}
