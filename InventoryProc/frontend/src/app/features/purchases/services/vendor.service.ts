import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vendor, CreateVendorRequest, UpdateVendorRequest } from '../models/vendor.model';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class VendorService {
  private readonly apiUrl = 'http://localhost:5005/api/vendors';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Vendor[]>> {
    return this.http.get<ApiResponse<Vendor[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<Vendor>> {
    return this.http.get<ApiResponse<Vendor>>(`${this.apiUrl}/${id}`);
  }

  getByCode(code: string): Observable<ApiResponse<Vendor>> {
    return this.http.get<ApiResponse<Vendor>>(`${this.apiUrl}/code/${code}`);
  }

  search(searchTerm: string): Observable<ApiResponse<Vendor[]>> {
    return this.http.get<ApiResponse<Vendor[]>>(`${this.apiUrl}/search?searchTerm=${encodeURIComponent(searchTerm)}`);
  }

  create(request: CreateVendorRequest): Observable<ApiResponse<Vendor>> {
    return this.http.post<ApiResponse<Vendor>>(this.apiUrl, request);
  }

  update(id: string, request: UpdateVendorRequest): Observable<ApiResponse<Vendor>> {
    return this.http.put<ApiResponse<Vendor>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
