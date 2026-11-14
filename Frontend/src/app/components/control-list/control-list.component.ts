import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ControlSummary } from '../../models/models';

@Component({
  selector: 'app-control-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container">
      <div class="header">
        <h1>רשימת בקרות</h1>
        <button class="btn-primary" routerLink="/controls/new">+ בקרה חדשה</button>
      </div>

      <div class="actions">
        <button class="btn-secondary" (click)="executeAll()" [disabled]="executing">
          {{ executing ? 'מריץ...' : 'הרץ את כל הבקרות' }}
        </button>
      </div>

      <table class="controls-table">
        <thead>
          <tr>
            <th>שם הבקרה</th>
            <th>ישות ראשית</th>
            <th>סטטוס</th>
            <th>הרצה אחרונה</th>
            <th>חריגות</th>
            <th>פעולות</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let control of controls">
            <td>{{ control.controlName }}</td>
            <td>{{ control.primaryEntityName }}</td>
            <td>
              <span [class]="control.isActive ? 'status-active' : 'status-inactive'">
                {{ control.isActive ? 'פעיל' : 'לא פעיל' }}
              </span>
            </td>
            <td>{{ control.lastExecutionDate | date:'dd/MM/yyyy HH:mm' }}</td>
            <td [class]="control.lastExecutionFailedCount ? 'violations' : ''">
              {{ control.lastExecutionFailedCount ?? '-' }}
            </td>
            <td class="actions-cell">
              <button class="btn-icon" (click)="execute(control.controlId)" title="הרץ">▶</button>
              <button class="btn-icon" routerLink="/controls/{{control.controlId}}" title="ערוך">✏️</button>
              <button class="btn-icon" routerLink="/controls/{{control.controlId}}/results" title="תוצאות">📊</button>
              <button class="btn-icon danger" (click)="delete(control.controlId)" title="מחק">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .actions { margin-bottom: 20px; }
    .controls-table { width: 100%; border-collapse: collapse; }
    .controls-table th, .controls-table td { padding: 12px; text-align: right; border-bottom: 1px solid #ddd; }
    .controls-table th { background: #f5f5f5; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-secondary { background: #fff; border: 1px solid #1976d2; color: #1976d2; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-icon { background: none; border: none; cursor: pointer; padding: 5px; font-size: 16px; }
    .btn-icon.danger:hover { color: red; }
    .status-active { color: green; }
    .status-inactive { color: gray; }
    .violations { color: red; font-weight: bold; }
    .actions-cell { white-space: nowrap; }
  `]
})
export class ControlListComponent implements OnInit {
  controls: ControlSummary[] = [];
  executing = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadControls();
  }

  loadControls() {
    this.api.getControls().subscribe(data => this.controls = data);
  }

  execute(id: number) {
    this.api.executeControl(id).subscribe(() => this.loadControls());
  }

  executeAll() {
    this.executing = true;
    this.api.executeAllControls().subscribe({
      next: () => { this.loadControls(); this.executing = false; },
      error: () => this.executing = false
    });
  }

  delete(id: number) {
    if (confirm('האם למחוק את הבקרה?')) {
      this.api.deleteControl(id).subscribe(() => this.loadControls());
    }
  }
}
