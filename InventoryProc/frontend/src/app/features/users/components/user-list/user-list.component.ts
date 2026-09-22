import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserManagementService } from '../../services/user-management.service';
import { User, getRoleName } from '../../models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  searchTerm: string = '';
  loading = false;
  error: string | null = null;

  constructor(
    private userManagementService: UserManagementService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;
    this.userManagementService.getAllUsers().subscribe({
      next: (response) => {
        if (response.success) {
          this.users = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error('Error loading users:', err);
        this.cdr.detectChanges();
      }
    });
  }

  getRoleName(role: number): string {
    return getRoleName(role);
  }

  filterUsers(): void {
    // Simple search filter - can be enhanced
    if (!this.searchTerm.trim()) {
      return;
    }
    const term = this.searchTerm.toLowerCase();
    // This is just for search box functionality, filtering happens client-side
    // In a real scenario, you'd reload from the server with search params
  }

  onActivate(userId: string): void {
    if (!confirm('Are you sure you want to activate this user?')) {
      return;
    }

    this.userManagementService.activateUser(userId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadUsers();
        } else {
          alert('Failed to activate user: ' + response.message);
        }
      },
      error: (err) => {
        alert('Failed to activate user');
        console.error('Error activating user:', err);
      }
    });
  }

  onDeactivate(userId: string): void {
    if (!confirm('Are you sure you want to deactivate this user?')) {
      return;
    }

    this.userManagementService.deactivateUser(userId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadUsers();
        } else {
          alert('Failed to deactivate user: ' + response.message);
        }
      },
      error: (err) => {
        alert('Failed to deactivate user');
        console.error('Error deactivating user:', err);
      }
    });
  }

  onDelete(userId: string, userName: string): void {
    if (!confirm(`Are you sure you want to delete user "${userName}"? This action cannot be undone.`)) {
      return;
    }

    this.userManagementService.deleteUser(userId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadUsers();
        } else {
          alert('Failed to delete user: ' + response.message);
        }
      },
      error: (err) => {
        alert('Failed to delete user');
        console.error('Error deleting user:', err);
      }
    });
  }
}
