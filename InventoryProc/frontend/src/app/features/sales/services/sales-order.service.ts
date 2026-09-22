import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateSalesOrderRequest,
  UpdateSalesOrderRequest,
} from '../models/sales-order.model';

@Injectable({
  providedIn: 'root',
})
export class SalesOrderService {
  private readonly apiUrl = 'http://localhost:5005/api/salesorders';

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getByOrderNumber(orderNumber: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/order-number/${orderNumber}`);
  }

  getByCustomer(customerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/customer/${customerId}`);
  }

  create(request: CreateSalesOrderRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  update(id: string, request: UpdateSalesOrderRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  confirm(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/confirm`, {});
  }

  ship(id: string, shippedDate?: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/ship`, {
      shippedDate,
    });
  }

  deliver(id: string, deliveredDate?: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/deliver`, {
      deliveredDate,
    });
  }

  cancel(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/cancel`, {});
  }

  applyDiscount(id: string, discountAmount: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/discount`, {
      discountAmount,
    });
  }
}
