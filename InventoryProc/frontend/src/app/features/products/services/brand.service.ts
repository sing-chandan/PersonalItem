import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Brand, CreateBrandRequest, UpdateBrandRequest } from '../../../models/brand.model';
import { ApiResponse } from '../../../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private apiUrl = 'http://localhost:5005/api/brands';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Brand[]>> {
    return this.http.get<ApiResponse<Brand[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<Brand>> {
    return this.http.get<ApiResponse<Brand>>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateBrandRequest): Observable<ApiResponse<Brand>> {
    return this.http.post<ApiResponse<Brand>>(this.apiUrl, request);
  }

  update(id: string, request: UpdateBrandRequest): Observable<ApiResponse<Brand>> {
    return this.http.put<ApiResponse<Brand>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
