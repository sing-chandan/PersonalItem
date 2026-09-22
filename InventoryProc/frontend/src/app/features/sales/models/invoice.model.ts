export interface Invoice {
  id: string;
  tenantId: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerId: string;
  customerName: string;
  customerGSTNumber?: string;
  billingAddress?: string;
  shippingAddress?: string;
  salesOrderId?: string;
  salesOrderNumber?: string;
  subTotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  paymentStatus: InvoicePaymentStatus;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  items: InvoiceItem[];
  payments: InvoicePayment[];
}

export interface InvoiceItem {
  id: string;
  invoiceId: string;
  productId: string;
  productName: string;
  productCode: string;
  hsnCode?: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  totalPrice: number;
  taxRate: number;
  taxAmount: number;
  description?: string;
}

export interface InvoicePayment {
  id: string;
  invoiceId: string;
  amount: number;
  paymentMethod: string;
  paymentDate: string;
  reference?: string;
  notes?: string;
}

export enum InvoicePaymentStatus {
  Unpaid = 'Unpaid',
  PartiallyPaid = 'PartiallyPaid',
  Paid = 'Paid',
  Overdue = 'Overdue',
  Cancelled = 'Cancelled',
}

export interface CreateInvoiceRequest {
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerId: string;
  customerName: string;
  customerGSTNumber?: string;
  billingAddress?: string;
  shippingAddress?: string;
  salesOrderId?: string;
  salesOrderNumber?: string;
  notes?: string;
  items: CreateInvoiceItemRequest[];
}

export interface CreateInvoiceItemRequest {
  productId: string;
  productName: string;
  productCode: string;
  hsnCode?: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  taxRate: number;
  description?: string;
}

export interface RecordPaymentRequest {
  amount: number;
  paymentMethod: string;
  paymentDate: string;
  reference?: string;
  notes?: string;
}
