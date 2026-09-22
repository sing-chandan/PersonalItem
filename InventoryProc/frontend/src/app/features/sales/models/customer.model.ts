export interface Customer {
  id: string;
  tenantId: string;
  customerCode: string;
  companyName: string;
  contactPerson: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  pincode?: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit?: number;
  creditDays?: number;
  outstandingBalance: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCustomerRequest {
  customerCode: string;
  companyName: string;
  contactPerson: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  pincode?: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit?: number;
  creditDays?: number;
}

export interface UpdateCustomerRequest {
  companyName: string;
  contactPerson: string;
  email?: string;
  phone?: string;
  mobile?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  pincode?: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit?: number;
  creditDays?: number;
}
