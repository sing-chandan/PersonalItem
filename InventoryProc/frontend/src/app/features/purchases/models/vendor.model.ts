export interface Vendor {
  id: string;
  tenantId: string;
  code: string;
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  mobile: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit: number;
  paymentTermDays: number;
  outstandingBalance: number;
  bankName?: string;
  bankAccountNumber?: string;
  bankIFSCCode?: string;
  isActive: boolean;
  createdAt: Date;
  createdBy: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface CreateVendorRequest {
  code: string;
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  mobile: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit: number;
  paymentTermDays: number;
  bankName?: string;
  bankAccountNumber?: string;
  bankIFSCCode?: string;
}

export interface UpdateVendorRequest {
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  mobile: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  gstNumber?: string;
  panNumber?: string;
  creditLimit: number;
  paymentTermDays: number;
  bankName?: string;
  bankAccountNumber?: string;
  bankIFSCCode?: string;
  isActive: boolean;
}
