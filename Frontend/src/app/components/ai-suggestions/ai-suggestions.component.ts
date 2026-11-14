import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface ControlSuggestion {
  name: string;
  description: string;
  entity_name: string;
  conditions: any[];
  severity: string;
  category: string;
}

@Component({
  selector: 'app-ai-suggestions',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container">
      <div class="header">
        <h1>🤖 הצעות בקרות מבוססות AI</h1>
        <button class="btn-secondary" routerLink="/controls">חזרה לרשימה</button>
      </div>

      <div class="info-box">
        <p>המערכת מנתחת את מבנה בסיס הנתונים ומציעה בקרות איכות נתונים רלוונטיות.</p>
      </div>

      <div class="actions">
        <button class="btn-primary" (click)="analyze()" [disabled]="loading">
          {{ loading ? 'מנתח...' : 'נתח והצע בקרות' }}
        </button>
      </div>

      <div class="stats" *ngIf="analyzedTables">
        <span>נותחו {{ analyzedTables }} טבלאות ו-{{ analyzedFields }} שדות</span>
      </div>

      <div class="suggestions-grid" *ngIf="suggestions.length">
        <div *ngFor="let suggestion of suggestions" class="suggestion-card" [class]="suggestion.severity">
          <div class="card-header">
            <span class="severity-badge" [class]="suggestion.severity">
              {{ getSeverityLabel(suggestion.severity) }}
            </span>
            <span class="category-badge">{{ getCategoryLabel(suggestion.category) }}</span>
          </div>
          <h3>{{ suggestion.name }}</h3>
          <p class="description">{{ suggestion.description }}</p>
          <p class="entity"><strong>ישות:</strong> {{ suggestion.entity_name }}</p>
          <div class="conditions">
            <strong>תנאים:</strong>
            <ul>
              <li *ngFor="let cond of suggestion.conditions">
                {{ cond.field }} {{ cond.operator }} {{ cond.value || cond.compare_field || '' }}
              </li>
            </ul>
          </div>
          <button class="btn-create" (click)="createControl(suggestion)">צור בקרה</button>
        </div>
      </div>

      <div class="no-suggestions" *ngIf="!loading && analyzed && !suggestions.length">
        לא נמצאו הצעות לבקרות
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; max-width: 1200px; margin: 0 auto; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .info-box { background: #e3f2fd; padding: 15px; border-radius: 8px; margin-bottom: 20px; }
    .actions { margin-bottom: 20px; }
    .stats { color: #666; margin-bottom: 20px; }
    
    .suggestions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 20px; }
    .suggestion-card { background: #fff; border: 1px solid #ddd; border-radius: 8px; padding: 20px; }
    .suggestion-card.high { border-right: 4px solid #d32f2f; }
    .suggestion-card.medium { border-right: 4px solid #ff9800; }
    .suggestion-card.low { border-right: 4px solid #4caf50; }
    
    .card-header { display: flex; gap: 10px; margin-bottom: 10px; }
    .severity-badge { padding: 3px 10px; border-radius: 4px; font-size: 12px; }
    .severity-badge.high { background: #ffebee; color: #c62828; }
    .severity-badge.medium { background: #fff3e0; color: #e65100; }
    .severity-badge.low { background: #e8f5e9; color: #2e7d32; }
    .category-badge { background: #e3f2fd; color: #1565c0; padding: 3px 10px; border-radius: 4px; font-size: 12px; }
    
    .suggestion-card h3 { margin: 0 0 10px 0; color: #333; }
    .description { color: #666; margin-bottom: 10px; }
    .entity { margin-bottom: 10px; }
    .conditions { margin-bottom: 15px; }
    .conditions ul { margin: 5px 0; padding-right: 20px; }
    
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-primary:disabled { background: #ccc; }
    .btn-secondary { background: #fff; border: 1px solid #1976d2; color: #1976d2; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-create { background: #4caf50; color: white; border: none; padding: 8px 16px; cursor: pointer; border-radius: 4px; width: 100%; }
    .btn-create:hover { background: #388e3c; }
    
    .no-suggestions { text-align: center; padding: 40px; color: #666; }
  `]
})
export class AiSuggestionsComponent implements OnInit {
  suggestions: ControlSuggestion[] = [];
  loading = false;
  analyzed = false;
  analyzedTables = 0;
  analyzedFields = 0;

  constructor(private http: HttpClient) {}

  ngOnInit() {}

  analyze() {
    this.loading = true;
    this.http.get<any>('http://localhost:5001/api/ai/analyze').subscribe({
      next: (result) => {
        this.suggestions = result.suggestions;
        this.analyzedTables = result.analyzed_tables;
        this.analyzedFields = result.analyzed_fields;
        this.analyzed = true;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error analyzing:', err);
        this.loading = false;
        alert('שגיאה בניתוח. ודא ששירות Python פעיל על פורט 5001');
      }
    });
  }

  getSeverityLabel(severity: string): string {
    const labels: { [key: string]: string } = {
      'high': 'חומרה גבוהה',
      'medium': 'חומרה בינונית',
      'low': 'חומרה נמוכה'
    };
    return labels[severity] || severity;
  }

  getCategoryLabel(category: string): string {
    const labels: { [key: string]: string } = {
      'data_quality': 'איכות נתונים',
      'business_rule': 'כלל עסקי',
      'consistency': 'עקביות'
    };
    return labels[category] || category;
  }

  createControl(suggestion: ControlSuggestion) {
    // Navigate to control editor with pre-filled data
    // For now, just show alert
    alert(`יצירת בקרה: ${suggestion.name}\n\nבגרסה הבאה - מעבר אוטומטי לעורך הבקרות עם הנתונים המוכנים`);
  }
}
