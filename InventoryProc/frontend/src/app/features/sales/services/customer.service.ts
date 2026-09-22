import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Customer,
  CreateCustomerRequest,
  UpdateCustomerRequest,
} from '../models/customer.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private readonly apiUrl = 'http://localhost:5005/api/customers';

  constructor(private http: HttpClient) {}

  getAll(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  getByCode(customerCode: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/code/${customerCode}`);
  }

  search(searchTerm: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/search`, {
      params: { searchTerm },
    });
  }

  create(request: CreateCustomerRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  update(id: string, request: UpdateCustomerRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
