import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SalesReportRequest, SalesReportResponse } from '../models/sales-report.model';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class SalesReportService {
  private readonly apiUrl = 'http://localhost:5005/api/reports';

  constructor(private http: HttpClient) {}

  getSalesReport(request: SalesReportRequest): Observable<ApiResponse<SalesReportResponse>> {
    return this.http.post<ApiResponse<SalesReportResponse>>(`${this.apiUrl}/sales`, request);
  }
}
