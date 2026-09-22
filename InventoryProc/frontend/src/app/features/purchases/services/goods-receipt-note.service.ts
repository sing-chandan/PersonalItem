import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GoodsReceiptNote, CreateGoodsReceiptNoteRequest } from '../models/goods-receipt-note.model';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class GoodsReceiptNoteService {
  private readonly apiUrl = 'http://localhost:5005/api/goodsreceiptnotes';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<GoodsReceiptNote[]>> {
    return this.http.get<ApiResponse<GoodsReceiptNote[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<GoodsReceiptNote>> {
    return this.http.get<ApiResponse<GoodsReceiptNote>>(`${this.apiUrl}/${id}`);
  }

  getByGRNNumber(grnNumber: string): Observable<ApiResponse<GoodsReceiptNote>> {
    return this.http.get<ApiResponse<GoodsReceiptNote>>(`${this.apiUrl}/grn-number/${grnNumber}`);
  }

  getByPurchaseOrder(purchaseOrderId: string): Observable<ApiResponse<GoodsReceiptNote[]>> {
    return this.http.get<ApiResponse<GoodsReceiptNote[]>>(`${this.apiUrl}/purchase-order/${purchaseOrderId}`);
  }

  create(request: CreateGoodsReceiptNoteRequest): Observable<ApiResponse<GoodsReceiptNote>> {
    return this.http.post<ApiResponse<GoodsReceiptNote>>(this.apiUrl, request);
  }

  complete(id: string): Observable<ApiResponse<GoodsReceiptNote>> {
    return this.http.post<ApiResponse<GoodsReceiptNote>>(`${this.apiUrl}/${id}/complete`, {});
  }

  cancel(id: string): Observable<ApiResponse<GoodsReceiptNote>> {
    return this.http.post<ApiResponse<GoodsReceiptNote>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
