import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Invoice,
  CreateInvoiceRequest,
  RecordPaymentRequest,
} from '../models/invoice.model';

@Injectable({
  providedIn: 'root',
})
export class InvoiceService {
  private readonly apiUrl = 'http://localhost:5005/api/invoices';

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getByInvoiceNumber(invoiceNumber: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/number/${invoiceNumber}`);
  }

  getByCustomer(customerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/customer/${customerId}`);
  }

  getOverdueInvoices(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/overdue`);
  }

  getUnpaidInvoices(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/unpaid`);
  }

  create(request: CreateInvoiceRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  createFromSalesOrder(orderId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/from-order/${orderId}`, {});
  }

  recordPayment(id: string, request: RecordPaymentRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/payment`, request);
  }

  voidInvoice(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/void`, {});
  }

  delete(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
