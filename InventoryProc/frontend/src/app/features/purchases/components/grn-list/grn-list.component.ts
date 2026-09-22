import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GoodsReceiptNoteService } from '../../services/goods-receipt-note.service';
import {
  GoodsReceiptNote,
  GRNStatus,
  getGRNStatusLabel,
  getGRNStatusClass
} from '../../models/goods-receipt-note.model';

@Component({
  selector: 'app-grn-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './grn-list.component.html',
  styleUrls: ['./grn-list.component.css']
})
export class GRNListComponent implements OnInit {
  grns: GoodsReceiptNote[] = [];
  filteredGRNs: GoodsReceiptNote[] = [];
  searchTerm: string = '';
  statusFilter: GRNStatus | null = null;
  loading: boolean = false;
  error: string | null = null;

  GRNStatus = GRNStatus;

  constructor(
    private grnService: GoodsReceiptNoteService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadGRNs();
  }

  loadGRNs(): void {
    this.loading = true;
    this.error = null;

    this.grnService.getAll().subscribe({
      next: (response) => {
        if (response.success) {
          this.grns = response.data;
          this.filteredGRNs = this.grns;
          this.applyFilters();
        } else {
          this.error = response.message;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load goods receipt notes. Please try again.';
        this.loading = false;
        console.error('Error loading GRNs:', err);
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters(): void {
    let filtered = this.grns;

    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(grn =>
        grn.grnNumber.toLowerCase().includes(term) ||
        grn.purchaseOrderNumber.toLowerCase().includes(term) ||
        grn.receivedBy.toLowerCase().includes(term)
      );
    }

    if (this.statusFilter !== null) {
      filtered = filtered.filter(grn => grn.status === this.statusFilter);
    }

    this.filteredGRNs = filtered;
    this.cdr.detectChanges();
  }

  onStatusFilterChange(event: any): void {
    const value = event.target.value;
    this.statusFilter = value === '' ? null : parseInt(value);
    this.applyFilters();
  }

  getStatusLabel(status: GRNStatus): string {
    return getGRNStatusLabel(status);
  }

  getStatusClass(status: GRNStatus): string {
    return getGRNStatusClass(status);
  }

  completeGRN(id: string, grnNumber: string): void {
    if (!confirm(`Are you sure you want to complete GRN "${grnNumber}"? This will update stock levels.`)) {
      return;
    }

    this.grnService.complete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadGRNs();
        } else {
          alert(`Failed to complete GRN: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to complete GRN. Please try again.');
        console.error('Error completing GRN:', err);
      }
    });
  }

  cancelGRN(id: string, grnNumber: string): void {
    if (!confirm(`Are you sure you want to cancel GRN "${grnNumber}"?`)) {
      return;
    }

    this.grnService.cancel(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadGRNs();
        } else {
          alert(`Failed to cancel GRN: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to cancel GRN. Please try again.');
        console.error('Error canceling GRN:', err);
      }
    });
  }

  deleteGRN(id: string, grnNumber: string): void {
    if (!confirm(`Are you sure you want to delete GRN "${grnNumber}"?`)) {
      return;
    }

    this.grnService.delete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadGRNs();
        } else {
          alert(`Failed to delete GRN: ${response.message}`);
        }
      },
      error: (err) => {
        alert('Failed to delete GRN. Please try again.');
        console.error('Error deleting GRN:', err);
      }
    });
  }

  getTotalAccepted(grn: GoodsReceiptNote): number {
    return grn.items.reduce((sum, item) => sum + item.acceptedQuantity, 0);
  }

  getTotalRejected(grn: GoodsReceiptNote): number {
    return grn.items.reduce((sum, item) => sum + item.rejectedQuantity, 0);
  }
}
