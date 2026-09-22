export interface User {
  userId: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantName: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  mobile?: string;
}

export interface AuthResponse {
  userId: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  token: string;
  refreshToken: string;
  expiresAt: string;
}

export interface ApiResponse<T> {
  data: T | null;
  success: boolean;
  message: string;
  errors: string[];
}
