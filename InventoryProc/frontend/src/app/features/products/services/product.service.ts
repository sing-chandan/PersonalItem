import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, CreateProductRequest, UpdateProductRequest, StockAdjustmentRequest } from '../../../models/product.model';
import { ApiResponse } from '../../../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = 'http://localhost:5005/api/products';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`);
  }

  search(searchTerm: string): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/search`, {
      params: { searchTerm }
    });
  }

  getLowStock(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/low-stock`);
  }

  create(request: CreateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(this.apiUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  adjustStock(id: string, request: StockAdjustmentRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/adjust-stock`, request);
  }
}
