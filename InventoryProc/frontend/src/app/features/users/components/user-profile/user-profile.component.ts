import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserManagementService } from '../../services/user-management.service';
import { AuthService } from '../../../../core/services/auth.service';
import { getRoleName } from '../../models/user.model';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  currentUser: any = null;
  loading = false;
  submitting = false;
  changingPassword = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private userManagementService: UserManagementService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.initForms();
    this.loadCurrentUser();
  }

  initForms(): void {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      mobile: ['']
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  loadCurrentUser(): void {
    this.currentUser = this.authService.currentUser;
    if (this.currentUser) {
      this.profileForm.patchValue({
        firstName: this.currentUser.firstName,
        lastName: this.currentUser.lastName,
        mobile: this.currentUser.mobile
      });
    }
  }

  getRoleName(role: number): string {
    return getRoleName(role);
  }

  onUpdateProfile(): void {
    if (this.profileForm.invalid) {
      Object.keys(this.profileForm.controls).forEach(key => {
        this.profileForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.submitting = true;
    this.error = null;
    this.successMessage = null;

    this.userManagementService.updateProfile(this.profileForm.value).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Profile updated successfully';
          // Reload current user from auth service
          this.loadCurrentUser();
        } else {
          this.error = response.message;
        }
        this.submitting = false;
      },
      error: (err) => {
        this.error = 'Failed to update profile';
        this.submitting = false;
        console.error('Error updating profile:', err);
      }
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) {
      Object.keys(this.passwordForm.controls).forEach(key => {
        this.passwordForm.get(key)?.markAsTouched();
      });
      return;
    }

    if (this.passwordForm.value.newPassword !== this.passwordForm.value.confirmPassword) {
      this.error = 'New password and confirm password do not match';
      return;
    }

    this.changingPassword = true;
    this.error = null;
    this.successMessage = null;

    const request = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.userManagementService.changePassword(this.currentUser.userId, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Password changed successfully';
          this.passwordForm.reset();
        } else {
          this.error = response.message;
        }
        this.changingPassword = false;
      },
      error: (err) => {
        this.error = 'Failed to change password';
        this.changingPassword = false;
        console.error('Error changing password:', err);
      }
    });
  }
}
