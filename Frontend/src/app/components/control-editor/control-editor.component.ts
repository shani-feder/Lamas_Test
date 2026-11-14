import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { BusinessEntity, BusinessField, EntityRelation, Control, ControlCondition, ControlJoin, OperatorInfo } from '../../models/models';

@Component({
  selector: 'app-control-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="container">
      <h1>{{ isNew ? 'בקרה חדשה' : 'עריכת בקרה' }}</h1>

      <div class="form-section">
        <h3>פרטי הבקרה</h3>
        <div class="form-group">
          <label>שם הבקרה</label>
          <input type="text" [(ngModel)]="control.controlName" />
        </div>
        <div class="form-group">
          <label>תיאור</label>
          <textarea [(ngModel)]="control.controlDescription"></textarea>
        </div>
        <div class="form-group">
          <label>ישות ראשית</label>
          <select [(ngModel)]="control.primaryEntityId" (change)="onEntityChange()">
            <option *ngFor="let e of entities" [value]="e.entityId">{{ e.businessName }}</option>
          </select>
        </div>
      </div>

      <div class="form-section">
        <h3>קשרים (JOINs)</h3>
        <div *ngFor="let join of control.joins; let i = index" class="join-row">
          <select [(ngModel)]="join.relationId">
            <option *ngFor="let r of availableRelations" [value]="r.relationId">
              {{ r.relationName }} ({{ r.sourceEntityName }} → {{ r.targetEntityName }})
            </option>
          </select>
          <select [(ngModel)]="join.joinType">
            <option value="INNER">INNER</option>
            <option value="LEFT">LEFT</option>
            <option value="RIGHT">RIGHT</option>
          </select>
          <button class="btn-icon danger" (click)="removeJoin(i)">🗑️</button>
        </div>
        <button class="btn-secondary" (click)="addJoin()">+ הוסף קשר</button>
      </div>

      <div class="form-section">
        <h3>תנאים</h3>
        <div *ngFor="let cond of control.conditions; let i = index" class="condition-row">
          <select [(ngModel)]="cond.logicalOperator" *ngIf="i > 0">
            <option value="AND">וגם</option>
            <option value="OR">או</option>
          </select>
          <select [(ngModel)]="cond.fieldId">
            <option *ngFor="let f of allFields" [value]="f.fieldId">
              {{ f.entityBusinessName }}.{{ f.businessName }}
            </option>
          </select>
          <select [(ngModel)]="cond.operator">
            <option *ngFor="let op of operators" [value]="op.value">{{ op.displayName }}</option>
          </select>
          <input type="text" [(ngModel)]="cond.value" *ngIf="requiresValue(cond.operator)" placeholder="ערך" />
          <select [(ngModel)]="cond.valueType">
            <option value="static">ערך קבוע</option>
            <option value="field">שדה</option>
            <option value="expression">ביטוי</option>
          </select>
          <button class="btn-icon danger" (click)="removeCondition(i)">🗑️</button>
        </div>
        <button class="btn-secondary" (click)="addCondition()">+ הוסף תנאי</button>
      </div>

      <div class="form-section" *ngIf="!isNew">
        <h3>תצוגה מקדימה של SQL</h3>
        <button class="btn-secondary" (click)="previewSql()">הצג SQL</button>
        <pre *ngIf="sqlPreview">{{ sqlPreview }}</pre>
      </div>

      <div class="actions">
        <button class="btn-primary" (click)="save()">שמור</button>
        <button class="btn-secondary" routerLink="/controls">ביטול</button>
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; max-width: 900px; }
    .form-section { background: #f9f9f9; padding: 20px; margin-bottom: 20px; border-radius: 8px; }
    .form-group { margin-bottom: 15px; }
    .form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
    .form-group input, .form-group textarea, .form-group select { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
    .condition-row, .join-row { display: flex; gap: 10px; margin-bottom: 10px; align-items: center; flex-wrap: wrap; }
    .condition-row select, .condition-row input, .join-row select { flex: 1; min-width: 100px; padding: 8px; }
    .btn-primary { background: #1976d2; color: white; border: none; padding: 10px 20px; cursor: pointer; border-radius: 4px; }
    .btn-secondary { background: #fff; border: 1px solid #1976d2; color: #1976d2; padding: 10px 20px; cursor: pointer; border-radius: 4px; margin-right: 10px; }
    .btn-icon { background: none; border: none; cursor: pointer; padding: 5px; font-size: 16px; }
    .btn-icon.danger:hover { color: red; }
    .actions { margin-top: 20px; }
    pre { background: #263238; color: #aed581; padding: 15px; border-radius: 4px; overflow-x: auto; direction: ltr; text-align: left; }
  `]
})
export class ControlEditorComponent implements OnInit {
  isNew = true;
  control: Control = {
    controlId: 0,
    controlName: '',
    controlDescription: '',
    primaryEntityId: 0,
    isActive: true,
    createdDate: new Date(),
    conditions: [],
    joins: []
  };

  entities: BusinessEntity[] = [];
  allFields: BusinessField[] = [];
  availableRelations: EntityRelation[] = [];
  operators: OperatorInfo[] = [];
  sqlPreview = '';

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.api.getEntities().subscribe(e => this.entities = e);
    this.api.getAllFields().subscribe(f => this.allFields = f);
    this.api.getRelations().subscribe(r => this.availableRelations = r);
    this.api.getOperators().subscribe(o => this.operators = o);

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.api.getControl(+id).subscribe(c => this.control = c);
    }
  }

  onEntityChange() {
    this.api.getRelationsByEntity(this.control.primaryEntityId).subscribe(r => this.availableRelations = r);
  }

  addCondition() {
    this.control.conditions.push({
      fieldId: this.allFields[0]?.fieldId || 0,
      operator: '=',
      value: '',
      valueType: 'static',
      logicalOperator: 'AND',
      groupNumber: 0,
      orderIndex: this.control.conditions.length
    });
  }

  removeCondition(index: number) {
    this.control.conditions.splice(index, 1);
  }

  addJoin() {
    this.control.joins.push({
      relationId: this.availableRelations[0]?.relationId || 0,
      joinType: 'INNER'
    });
  }

  removeJoin(index: number) {
    this.control.joins.splice(index, 1);
  }

  requiresValue(operator: string): boolean {
    const op = this.operators.find(o => o.value === operator);
    return op?.requiresValue ?? true;
  }

  previewSql() {
    this.api.previewSql(this.control.controlId).subscribe(r => this.sqlPreview = r.sql);
  }

  save() {
    const dto = {
      controlName: this.control.controlName,
      controlDescription: this.control.controlDescription,
      primaryEntityId: this.control.primaryEntityId,
      conditions: this.control.conditions,
      joins: this.control.joins
    };

    const request = this.isNew
      ? this.api.createControl(dto)
      : this.api.updateControl(this.control.controlId, dto);

    request.subscribe(() => this.router.navigate(['/controls']));
  }
}
