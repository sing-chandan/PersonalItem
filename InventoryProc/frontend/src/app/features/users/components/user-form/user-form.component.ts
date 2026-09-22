import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { getRoleOptions, UserRole } from '../../models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit {
  userForm!: FormGroup;
  isEditMode = false;
  userId: string | null = null;
  loading = false;
  submitting = false;
  error: string | null = null;
  roleOptions = getRoleOptions();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private userManagementService: UserManagementService
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.userId;

    this.initForm();

    if (this.isEditMode && this.userId) {
      this.loadUser(this.userId);
    }
  }

  initForm(): void {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', this.isEditMode ? [] : [Validators.required, Validators.minLength(6)]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      mobile: [''],
      role: [UserRole.SalesStaff, Validators.required],
      isActive: [true]
    });

    // Disable email in edit mode
    if (this.isEditMode) {
      this.userForm.get('email')?.disable();
    }
  }

  loadUser(id: string): void {
    this.loading = true;
    this.error = null;
    this.userManagementService.getUserById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.userForm.patchValue({
            email: response.data.email,
            firstName: response.data.firstName,
            lastName: response.data.lastName,
            mobile: response.data.mobile,
            role: response.data.role,
            isActive: response.data.isActive
          });
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load user';
        this.loading = false;
        console.error('Error loading user:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      Object.keys(this.userForm.controls).forEach(key => {
        this.userForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.submitting = true;
    this.error = null;

    if (this.isEditMode && this.userId) {
      const updateRequest = {
        firstName: this.userForm.value.firstName,
        lastName: this.userForm.value.lastName,
        mobile: this.userForm.value.mobile,
        role: this.userForm.value.role,
        isActive: this.userForm.value.isActive
      };

      this.userManagementService.updateUser(this.userId, updateRequest).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/users']);
          } else {
            this.error = response.message;
            this.submitting = false;
          }
        },
        error: (err) => {
          this.error = 'Failed to update user';
          this.submitting = false;
          console.error('Error updating user:', err);
        }
      });
    } else {
      const createRequest = {
        email: this.userForm.value.email,
        password: this.userForm.value.password,
        firstName: this.userForm.value.firstName,
        lastName: this.userForm.value.lastName,
        mobile: this.userForm.value.mobile,
        role: this.userForm.value.role
      };

      this.userManagementService.createUser(createRequest).subscribe({
        next: (response) => {
          if (response.success) {
            this.router.navigate(['/users']);
          } else {
            this.error = response.message;
            this.submitting = false;
          }
        },
        error: (err) => {
          this.error = 'Failed to create user';
          this.submitting = false;
          console.error('Error creating user:', err);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/users']);
  }
}
