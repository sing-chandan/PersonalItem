import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import {
  User,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  ApiResponse
} from '../../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly API_URL = 'http://localhost:5005/api/auth';
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {}

  get isAuthenticated(): boolean {
    return !!this.getToken();
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.storeAuthData(response.data);
          }
        })
      );
  }

  register(data: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/register`, data)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.storeAuthData(response.data);
          }
        })
      );
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${this.API_URL}/logout`, {}).subscribe({
        complete: () => this.clearAuthData()
      });
    } else {
      this.clearAuthData();
    }
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API_URL}/refresh`, { refreshToken: refreshToken })
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.storeAuthData(response.data);
          }
        })
      );
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  private storeAuthData(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);

    const user: User = {
      userId: authResponse.userId,
      tenantId: authResponse.tenantId,
      email: authResponse.email,
      firstName: authResponse.firstName,
      lastName: authResponse.lastName,
      role: authResponse.role
    };

    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (userJson) {
      try {
        return JSON.parse(userJson) as User;
      } catch {
        return null;
      }
    }
    return null;
  }
}
