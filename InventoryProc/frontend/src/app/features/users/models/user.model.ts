export enum UserRole {
  Admin = 0,
  Manager = 1,
  SalesStaff = 2,
  Viewer = 3
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  mobile?: string;
  role: UserRole;
  roleName: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  mobile?: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  mobile?: string;
  role: UserRole;
  isActive: boolean;
}

export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
  mobile?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ActivityLog {
  id: string;
  userId: string;
  userEmail: string;
  action: string;
  entityType: string;
  entityId?: string;
  description?: string;
  ipAddress?: string;
  timestamp: Date;
}

export function getRoleName(role: UserRole): string {
  switch (role) {
    case UserRole.Admin:
      return 'Admin';
    case UserRole.Manager:
      return 'Manager';
    case UserRole.SalesStaff:
      return 'Sales Staff';
    case UserRole.Viewer:
      return 'Viewer';
    default:
      return 'Unknown';
  }
}

export function getRoleOptions(): { value: UserRole; label: string }[] {
  return [
    { value: UserRole.Admin, label: 'Admin' },
    { value: UserRole.Manager, label: 'Manager' },
    { value: UserRole.SalesStaff, label: 'Sales Staff' },
    { value: UserRole.Viewer, label: 'Viewer' }
  ];
}
