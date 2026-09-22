import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PurchaseOrder,
  CreatePurchaseOrderRequest,
  UpdatePurchaseOrderRequest,
  ApproveOrderRequest,
  PurchaseOrderApplyDiscountRequest
} from '../models/purchase-order.model';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {
  private readonly apiUrl = 'http://localhost:5005/api/purchaseorders';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<PurchaseOrder[]>> {
    return this.http.get<ApiResponse<PurchaseOrder[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.get<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}`);
  }

  getByOrderNumber(orderNumber: string): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.get<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/order-number/${orderNumber}`);
  }

  getByVendor(vendorId: string): Observable<ApiResponse<PurchaseOrder[]>> {
    return this.http.get<ApiResponse<PurchaseOrder[]>>(`${this.apiUrl}/vendor/${vendorId}`);
  }

  create(request: CreatePurchaseOrderRequest): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.post<ApiResponse<PurchaseOrder>>(this.apiUrl, request);
  }

  update(id: string, request: UpdatePurchaseOrderRequest): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.put<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  approve(id: string, request?: ApproveOrderRequest): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}/approve`, request || {});
  }

  close(id: string): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}/close`, {});
  }

  cancel(id: string): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  applyDiscount(id: string, request: PurchaseOrderApplyDiscountRequest): Observable<ApiResponse<PurchaseOrder>> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.apiUrl}/${id}/discount`, request);
  }
}
