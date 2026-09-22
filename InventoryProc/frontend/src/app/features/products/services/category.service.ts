import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../../../models/category.model';
import { ApiResponse } from '../../../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = 'http://localhost:5005/api/categories';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl);
  }

  getById(id: string): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, request);
  }

  update(id: string, request: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
