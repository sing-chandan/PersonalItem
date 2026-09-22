import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserManagementService } from '../../services/user-management.service';
import { ActivityLog } from '../../models/user.model';

@Component({
  selector: 'app-activity-logs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './activity-logs.component.html',
  styleUrls: ['./activity-logs.component.scss']
})
export class ActivityLogsComponent implements OnInit {
  activityLogs: ActivityLog[] = [];
  loading = false;
  error: string | null = null;
  pageSize = 100;
  currentPage = 1;

  constructor(private userManagementService: UserManagementService) {}

  ngOnInit(): void {
    this.loadActivityLogs();
  }

  loadActivityLogs(): void {
    this.loading = true;
    this.error = null;
    this.userManagementService.getActivityLogs(this.pageSize, this.currentPage).subscribe({
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
    if (action.includes('Login')) return 'bg-primary';
    if (action.includes('Logout')) return 'bg-secondary';
    return 'bg-secondary';
  }
}
