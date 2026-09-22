import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InventoryReportRequest, InventoryReportResponse } from '../models/inventory-report.model';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class InventoryReportService {
  private readonly apiUrl = 'http://localhost:5005/api/reports';

  constructor(private http: HttpClient) {}

  getInventoryReport(request: InventoryReportRequest): Observable<ApiResponse<InventoryReportResponse>> {
    return this.http.post<ApiResponse<InventoryReportResponse>>(`${this.apiUrl}/inventory`, request);
  }
}
