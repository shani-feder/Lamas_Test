import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ControlExecutionResult, Control } from '../../models/models';

@Component({
  selector: 'app-control-results',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="container">
      <div class="header">
        <h1>תוצאות בקרה: {{ result?.controlName }}</h1>
        <button class="btn-secondary" routerLink="/controls">חזרה לרשימה</button>
      </div>

      <div class="control-info" *ngIf="control">
        <p><strong>תיאור:</strong> {{ control.controlDescription }}</p>
        <p><strong>תנאי הבקרה:</strong></p>
        <ul class="conditions-list">
          <li *ngFor="let cond of control.conditions; let i = index">
            <span *ngIf="i > 0" class="logical-op">{{ cond.logicalOperator === 'AND' ? 'וגם' : 'או' }}</span>
            {{ cond.fieldBusinessName }} {{ getOperatorDisplay(cond.operator) }} 
            <span *ngIf="cond.value">{{ cond.value }}</span>
          </li>
        </ul>
      </div>

      <div class="summary" *ngIf="result">
        <div class="stat">
          <span class="label">סה"כ רשומות</span>
          <span class="value">{{ result.totalRecordsChecked }}</span>
        </div>
        <div class="stat violations">
          <span class="label">חריגות</span>
          <span class="value">{{ result.failedRecordsCount }}</span>
        </div>
        <div class="stat">
          <span class="label">זמן ריצה</span>
          <span class="value">{{ result.executionTimeMs }} ms</span>
        </div>
        <div class="stat" [class.success]="result.status === 'Success'" [class.error]="result.status === 'Error'">
          <span class="label">סטטוס</span>
          <span class="value">{{ result.status === 'Success' ? 'הצלחה' : 'שגיאה' }}</span>
        </div>
      </div>

      <div class="error-message" *ngIf="result?.errorMessage">
        {{ result.errorMessage }}
      </div>

      <!-- סינון וחיפוש -->
      <div class="filters" *ngIf="result?.violations?.length">
        <div class="search-box">
          <input type="text" [(ngModel)]="searchTerm" (ngModelChange)="applyFilters()" 
                 placeholder="חיפוש בתוצאות..." />
        </div>
        <div class="filter-options">
          <label>סינון לפי עמודה:</label>
          <select [(ngModel)]="filterColumn" (ngModelChange)="applyFilters()">
            <option value="">הכל</option>
            <option *ngFor="let col of result.columns" [value]="col.columnName">{{ col.businessName }}</option>
          </select>
          <input type="text" [(ngModel)]="filterValue" (ngModelChange)="applyFilters()" 
                 placeholder="ערך לסינון" *ngIf="filterColumn" />
        </div>
        <div class="results-count">
          מציג {{ filteredViolations.length }} מתוך {{ result.violations.length }} רשומות
        </div>
      </div>

      <div class="results-table" *ngIf="filteredViolations?.length">
        <h3>רשומות חריגות</h3>
        <table>
          <thead>
            <tr>
              <th class="reason-col">סיבת כישלון</th>
              <th *ngFor="let col of result!.columns" (click)="sortBy(col.columnName)" class="sortable">
                {{ col.businessName }}
                <span *ngIf="sortColumn === col.columnName">{{ sortDirection === 'asc' ? '▲' : '▼' }}</span>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let violation of filteredViolations">
              <td class="reason-cell">
                <div class="violation-reasons">
                  <span *ngFor="let reason of getViolationReasons(violation)" class="reason-tag">
                    {{ reason }}
                  </span>
                </div>
              </td>
              <td *ngFor="let col of result!.columns" [class.highlight]="isViolationField(col.columnName, violation)">
                {{ formatValue(violation.data[col.columnName], col.dataType) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="no-violations" *ngIf="result && !result.violations?.length && result.status === 'Success'">
        ✅ לא נמצאו חריגות - כל הרשומות עומדות בתנאי הבקרה
      </div>

      <div class="actions">
        <button class="btn-primary" (click)="execute()" [disabled]="loading">
          {{ loading ? 'מריץ...' : 'הרץ שוב' }}
        </button>
        <button class="btn-secondary" (click)="exportResults()" *ngIf="result?.violations?.length">
          ייצוא לקובץ
        </button>
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; max-width: 1200px; margin: 0 auto; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .control-info { background: #e3f2fd; padding: 15px; border-radius: 8px; margin-bottom: 20px; }
    .conditions-list { margin: 10px 0; padding-right: 20px; }
    .conditions-list li { margin: 5px 0; }
    .logical-op { color: #1976d2; font-weight: bold; margin-left: 5px; }
    .summary { display: flex; gap: 20px; margin-bottom: 20px; flex-wrap: wrap; }
    .stat { background: #f5f5f5; padding: 15px 25px; border-radius: 8px; text-align: center; min-width: 120px; }
    .stat .label { display: block; font-size: 12px; color: #666; }
    .stat .value { display: block; font-size: 24px; font-weight: bold; }
    .stat.violations .value { color: #d32f2f; }
    .stat.success .value { color: #388e3c; }
    .stat.error .value { color: #d32f2f; }
    .error-message { background: #ffebee; color: #c62828; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
    
    .filters { background: #fff; padding: 15px; border: 1px solid #ddd; border-radius: 8px; margin-bottom: 20px; }
    .search-box input { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; margin-bottom: 10px; }
    .filter-options { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
    .filter-options select, .filter-options input { padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
    .results-count { margin-top: 10px; color: #666; font-size: 14px; }
    
    .results-table { margin-bottom: 20px; overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; min-width: 800px; }
    th, td { padding: 12px; text-align: right; border-bottom: 1px solid #ddd; }
    th { background: #f5f5f5; position: sticky; top: 0; }
    th.sortable { cursor: pointer; }
    th.sortable:hover { background: #e0e0e0; }
    .reason-col { min-width: 200px; }
    .reason-cell { vertical-align: top; }
    .violation-reasons { display: flex; flex-wrap: wrap; gap: 5px; }
    .reason-tag { background: #ffebee; color: #c62828; padding: 3px 8px; border-radius: 4px; font-size: 12px; }
    .highlight { background: #fff3e0; }
    
    .no-violations { background: #e8f5e9; color: #2e7d32; padding: 30px; border-radius: 8px; text-align: center; margin-bottom: 20px; font-size: 18px; }
    .actions { display: flex; gap: 10px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-primary:disabled { background: #ccc; }
    .btn-secondary { background: #fff; border: 1px solid #1976d2; color: #1976d2; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
  `]
})
export class ControlResultsComponent implements OnInit {
  result: ControlExecutionResult | null = null;
  control: Control | null = null;
  loading = false;
  controlId = 0;

  // סינון וחיפוש
  searchTerm = '';
  filterColumn = '';
  filterValue = '';
  filteredViolations: any[] = [];

  // מיון
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  private operatorDisplayMap: { [key: string]: string } = {
    '=': 'שווה ל',
    '!=': 'שונה מ',
    '>': 'גדול מ',
    '<': 'קטן מ',
    '>=': 'גדול או שווה ל',
    '<=': 'קטן או שווה ל',
    'LIKE': 'מכיל',
    'IS NULL': 'ריק',
    'IS NOT NULL': 'לא ריק',
    'IN': 'אחד מ'
  };

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.controlId = +this.route.snapshot.paramMap.get('id')!;
    this.loadControl();
    this.execute();
  }

  loadControl() {
    this.api.getControl(this.controlId).subscribe(c => this.control = c);
  }

  execute() {
    this.loading = true;
    this.api.executeControl(this.controlId).subscribe({
      next: r => {
        this.result = r;
        this.filteredViolations = r.violations || [];
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  getOperatorDisplay(operator: string): string {
    return this.operatorDisplayMap[operator] || operator;
  }

  getViolationReasons(violation: any): string[] {
    if (!this.control) return [];
    
    const reasons: string[] = [];
    for (const cond of this.control.conditions) {
      const value = violation.data[cond.columnName!];
      const condValue = cond.value;
      
      let failed = false;
      switch (cond.operator) {
        case '=': failed = value != condValue; break;
        case '!=': failed = value == condValue; break;
        case '>': failed = !(value > Number(condValue)); break;
        case '<': failed = !(value < Number(condValue)); break;
        case '>=': failed = !(value >= Number(condValue)); break;
        case '<=': failed = !(value <= Number(condValue)); break;
        case 'IS NULL': failed = value !== null && value !== undefined; break;
        case 'IS NOT NULL': failed = value === null || value === undefined; break;
        case 'LIKE': failed = !String(value).includes(condValue || ''); break;
      }
      
      // הבקרה מחזירה רשומות שעומדות בתנאים - אלו החריגות
      if (!failed) {
        reasons.push(`${cond.fieldBusinessName} ${this.getOperatorDisplay(cond.operator)} ${condValue || ''}`);
      }
    }
    
    return reasons.length ? reasons : ['עומד בתנאי הבקרה'];
  }

  isViolationField(columnName: string, violation: any): boolean {
    if (!this.control) return false;
    return this.control.conditions.some(c => c.columnName === columnName);
  }

  applyFilters() {
    if (!this.result?.violations) return;
    
    let filtered = [...this.result.violations];
    
    // חיפוש טקסט חופשי
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(v => 
        Object.values(v.data).some(val => 
          String(val).toLowerCase().includes(term)
        )
      );
    }
    
    // סינון לפי עמודה
    if (this.filterColumn && this.filterValue) {
      filtered = filtered.filter(v => 
        String(v.data[this.filterColumn]).toLowerCase().includes(this.filterValue.toLowerCase())
      );
    }
    
    // מיון
    if (this.sortColumn) {
      filtered.sort((a, b) => {
        const aVal = a.data[this.sortColumn];
        const bVal = b.data[this.sortColumn];
        const comparison = aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }
    
    this.filteredViolations = filtered;
  }

  sortBy(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.applyFilters();
  }

  formatValue(value: any, dataType: string): string {
    if (value === null || value === undefined) return '-';
    if (dataType === 'date') {
      const date = new Date(value);
      return date.toLocaleDateString('he-IL');
    }
    if (dataType === 'boolean') return value ? 'כן' : 'לא';
    if (dataType === 'number') return Number(value).toLocaleString('he-IL');
    return String(value);
  }

  exportResults() {
    if (!this.result?.violations?.length) return;
    
    const headers = this.result.columns.map(c => c.businessName).join(',');
    const rows = this.filteredViolations.map(v => 
      this.result!.columns.map(c => `"${v.data[c.columnName] ?? ''}"`).join(',')
    );
    
    const csv = '\uFEFF' + [headers, ...rows].join('\n'); // BOM for Hebrew
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${this.result.controlName}_results.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }
}
