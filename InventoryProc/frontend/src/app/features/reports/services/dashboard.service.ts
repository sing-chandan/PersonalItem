import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardSummary } from '../models/dashboard.model';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = 'http://localhost:5005/api/reports';

  constructor(private http: HttpClient) {}

  getDashboardSummary(): Observable<ApiResponse<DashboardSummary>> {
    return this.http.get<ApiResponse<DashboardSummary>>(`${this.apiUrl}/dashboard`);
  }
}
