import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User, CreateUserRequest, UpdateUserRequest, UpdateUserProfileRequest, ChangePasswordRequest, ActivityLog } from '../models/user.model';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private readonly apiUrl = 'http://localhost:5005/api/users';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(this.apiUrl);
  }

  getUserById(id: string): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(this.apiUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/${id}`, request);
  }

  updateProfile(request: UpdateUserProfileRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/profile`, request);
  }

  changePassword(id: string, request: ChangePasswordRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/change-password`, request);
  }

  activateUser(id: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  deleteUser(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  getActivityLogs(pageSize: number = 100, page: number = 1): Observable<ApiResponse<ActivityLog[]>> {
    return this.http.get<ApiResponse<ActivityLog[]>>(`${this.apiUrl}/activity-logs`, {
      params: { pageSize: pageSize.toString(), page: page.toString() }
    });
  }

  getUserActivityLogs(userId: string, pageSize: number = 50): Observable<ApiResponse<ActivityLog[]>> {
    return this.http.get<ApiResponse<ActivityLog[]>>(`${this.apiUrl}/${userId}/activity-logs`, {
      params: { pageSize: pageSize.toString() }
    });
  }
}
