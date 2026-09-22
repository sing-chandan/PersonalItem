import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { ActivityLog, User } from '../../models/user.model';

@Component({
  selector: 'app-user-activity',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-activity.component.html',
  styleUrls: ['./user-activity.component.scss']
})
export class UserActivityComponent implements OnInit {
  userId: string | null = null;
  user: User | null = null;
  activityLogs: ActivityLog[] = [];
  loading = false;
  error: string | null = null;
  pageSize = 50;

  constructor(
    private route: ActivatedRoute,
    private userManagementService: UserManagementService
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (this.userId) {
      this.loadUser(this.userId);
      this.loadActivityLogs(this.userId);
    }
  }

  loadUser(id: string): void {
    this.userManagementService.getUserById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.user = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading user:', err);
      }
    });
  }

  loadActivityLogs(userId: string): void {
    this.loading = true;
    this.error = null;
    this.userManagementService.getUserActivityLogs(userId, this.pageSize).subscribe({
      next: (response) => {
        if (response.success) {
          this.activityLogs = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load activity logs';
        this.loading = false;
        console.error('Error loading activity logs:', err);
      }
    });
  }

  getActionBadgeClass(action: string): string {
    if (action.includes('Create')) return 'bg-success';
    if (action.includes('Update') || action.includes('Edit')) return 'bg-info';
    if (action.includes('Delete')) return 'bg-danger';
    if (action.includes('Activate')) return 'bg-success';
    if (action.includes('Deactivate')) return 'bg-warning';
    return 'bg-secondary';
  }
}
