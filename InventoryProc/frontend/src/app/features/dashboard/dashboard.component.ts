import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../reports/services/dashboard.service';
import { DashboardSummary } from '../reports/models/dashboard.model';
import { BaseChartDirective } from 'ng2-charts';
import {
  Chart,
  ChartConfiguration,
  ChartData,
  ChartType,
  LinearScale,
  CategoryScale,
  BarElement,
  PointElement,
  LineElement,
  ArcElement,
  Legend,
  Title,
  Tooltip,
  Filler,
  BarController,
  LineController,
  PieController,
  DoughnutController
} from 'chart.js';

// Register Chart.js components
Chart.register(
  LinearScale,
  CategoryScale,
  BarElement,
  PointElement,
  LineElement,
  ArcElement,
  Legend,
  Title,
  Tooltip,
  Filler,
  BarController,
  LineController,
  PieController,
  DoughnutController
);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  authService = inject(AuthService);
  private router = inject(Router);
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);

  dashboardSummary: DashboardSummary | null = null;
  loading: boolean = false;
  error: string | null = null;

  // Bar Chart - Monthly Trend
  public barChartType: ChartType = 'bar';
  public barChartData: ChartData<'bar'> = { labels: [], datasets: [] };
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: true, position: 'top' },
      title: { display: true, text: 'Monthly Sales vs Purchases Trend' }
    },
    scales: {
      y: { beginAtZero: true }
    }
  };

  // Pie Chart - Top Products
  public pieChartType: ChartType = 'pie';
  public pieChartData: ChartData<'pie'> = { labels: [], datasets: [] };
  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: true, position: 'right' },
      title: { display: true, text: 'Top 5 Products by Sales' }
    }
  };

  // Doughnut Chart - Top Customers
  public doughnutChartType: ChartType = 'doughnut';
  public doughnutChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  public doughnutChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: true, position: 'right' },
      title: { display: true, text: 'Top 5 Customers by Sales' }
    }
  };

  get currentUser() {
    return this.authService.currentUser;
  }

  ngOnInit(): void {
    this.loadDashboardSummary();
  }

  loadDashboardSummary(): void {
    console.log('=== loadDashboardSummary called ===');
    this.loading = true;
    this.error = null;

    this.dashboardService.getDashboardSummary().subscribe({
      next: (response) => {
        console.log('=== Dashboard API Response ===', response);
        if (response.success && response.data) {
          this.dashboardSummary = response.data;
          console.log('Dashboard summary assigned:', this.dashboardSummary);
          this.initializeCharts();
          console.log('Charts initialized');
        } else {
          this.error = response.message || 'Failed to load dashboard data';
          console.error('Dashboard response not successful:', response);
        }
        this.loading = false;
        // Trigger change detection to ensure UI updates
        this.cdr.detectChanges();
        console.log('Change detection triggered');
      },
      error: (err) => {
        this.error = 'Failed to load dashboard data. Please try again.';
        this.loading = false;
        console.error('Error loading dashboard:', err);
        this.cdr.detectChanges();
      }
    });
  }

  private initializeCharts(): void {
    console.log('=== initializeCharts called ===');
    if (!this.dashboardSummary) {
      console.warn('No dashboard summary available');
      return;
    }

    console.log('Monthly Trend Data:', this.dashboardSummary.monthlyTrend);
    console.log('Top Products Data:', this.dashboardSummary.topProducts);
    console.log('Top Customers Data:', this.dashboardSummary.topCustomers);

    // Bar Chart - Monthly Trend
    this.barChartData = {
      labels: this.dashboardSummary.monthlyTrend.map(m => m.month),
      datasets: [
        {
          label: 'Sales',
          data: this.dashboardSummary.monthlyTrend.map(m => m.sales),
          backgroundColor: 'rgba(102, 126, 234, 0.7)',
          borderColor: 'rgba(102, 126, 234, 1)',
          borderWidth: 1
        },
        {
          label: 'Purchases',
          data: this.dashboardSummary.monthlyTrend.map(m => m.purchases),
          backgroundColor: 'rgba(255, 152, 0, 0.7)',
          borderColor: 'rgba(255, 152, 0, 1)',
          borderWidth: 1
        },
        {
          label: 'Profit',
          data: this.dashboardSummary.monthlyTrend.map(m => m.profit),
          backgroundColor: 'rgba(40, 167, 69, 0.7)',
          borderColor: 'rgba(40, 167, 69, 1)',
          borderWidth: 1
        }
      ]
    };
    console.log('Bar chart data:', this.barChartData);

    // Pie Chart - Top Products
    this.pieChartData = {
      labels: this.dashboardSummary.topProducts.map(p => p.productName),
      datasets: [{
        data: this.dashboardSummary.topProducts.map(p => p.revenue),
        backgroundColor: [
          'rgba(102, 126, 234, 0.8)',
          'rgba(40, 167, 69, 0.8)',
          'rgba(255, 193, 7, 0.8)',
          'rgba(220, 53, 69, 0.8)',
          'rgba(23, 162, 184, 0.8)'
        ],
        borderColor: [
          'rgba(102, 126, 234, 1)',
          'rgba(40, 167, 69, 1)',
          'rgba(255, 193, 7, 1)',
          'rgba(220, 53, 69, 1)',
          'rgba(23, 162, 184, 1)'
        ],
        borderWidth: 2
      }]
    };
    console.log('Pie chart data:', this.pieChartData);

    // Doughnut Chart - Top Customers
    this.doughnutChartData = {
      labels: this.dashboardSummary.topCustomers.map(c => c.customerName),
      datasets: [{
        data: this.dashboardSummary.topCustomers.map(c => c.totalRevenue),
        backgroundColor: [
          'rgba(102, 126, 234, 0.8)',
          'rgba(40, 167, 69, 0.8)',
          'rgba(255, 193, 7, 0.8)',
          'rgba(220, 53, 69, 0.8)',
          'rgba(23, 162, 184, 0.8)'
        ],
        borderColor: [
          'rgba(102, 126, 234, 1)',
          'rgba(40, 167, 69, 1)',
          'rgba(255, 193, 7, 1)',
          'rgba(220, 53, 69, 1)',
          'rgba(23, 162, 184, 1)'
        ],
        borderWidth: 2
      }]
    };
    console.log('Doughnut chart data:', this.doughnutChartData);
  }

  formatCurrency(value: number): string {
    return '₹' + value.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatNumber(value: number): string {
    return value.toLocaleString('en-IN');
  }

  formatPercentage(value: number): string {
    return value.toFixed(2) + '%';
  }

  logout(): void {
    this.authService.logout();
  }
}
