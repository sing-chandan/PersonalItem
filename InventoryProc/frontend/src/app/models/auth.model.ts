export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface AuthResponse {
  token: string;
  user: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    tenantId: string;
  };
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  companyName: string;
}
