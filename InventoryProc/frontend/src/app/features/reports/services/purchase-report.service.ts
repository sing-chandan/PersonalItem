import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PurchaseReportRequest, PurchaseReportResponse } from '../models/purchase-report.model';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseReportService {
  private readonly apiUrl = 'http://localhost:5005/api/reports';

  constructor(private http: HttpClient) {}

  getPurchaseReport(request: PurchaseReportRequest): Observable<ApiResponse<PurchaseReportResponse>> {
    return this.http.post<ApiResponse<PurchaseReportResponse>>(`${this.apiUrl}/purchases`, request);
  }
}
