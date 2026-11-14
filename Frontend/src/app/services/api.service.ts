import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  BusinessEntity, BusinessField, EntityRelation, 
  Control, ControlSummary, ControlExecutionResult, OperatorInfo 
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  // Metadata
  getEntities(): Observable<BusinessEntity[]> {
    return this.http.get<BusinessEntity[]>(`${this.baseUrl}/metadata/entities`);
  }

  getEntity(id: number): Observable<BusinessEntity> {
    return this.http.get<BusinessEntity>(`${this.baseUrl}/metadata/entities/${id}`);
  }

  getFieldsByEntity(entityId: number): Observable<BusinessField[]> {
    return this.http.get<BusinessField[]>(`${this.baseUrl}/metadata/entities/${entityId}/fields`);
  }

  getAllFields(): Observable<BusinessField[]> {
    return this.http.get<BusinessField[]>(`${this.baseUrl}/metadata/fields`);
  }

  getRelations(): Observable<EntityRelation[]> {
    return this.http.get<EntityRelation[]>(`${this.baseUrl}/metadata/relations`);
  }

  getRelationsByEntity(entityId: number): Observable<EntityRelation[]> {
    return this.http.get<EntityRelation[]>(`${this.baseUrl}/metadata/entities/${entityId}/relations`);
  }

  // Controls
  getControls(): Observable<ControlSummary[]> {
    return this.http.get<ControlSummary[]>(`${this.baseUrl}/controls`);
  }

  getControl(id: number): Observable<Control> {
    return this.http.get<Control>(`${this.baseUrl}/controls/${id}`);
  }

  createControl(control: Partial<Control>): Observable<{ controlId: number }> {
    return this.http.post<{ controlId: number }>(`${this.baseUrl}/controls`, control);
  }

  updateControl(id: number, control: Partial<Control>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/controls/${id}`, control);
  }

  deleteControl(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/controls/${id}`);
  }

  getOperators(): Observable<OperatorInfo[]> {
    return this.http.get<OperatorInfo[]>(`${this.baseUrl}/controls/operators`);
  }

  executeControl(id: number): Observable<ControlExecutionResult> {
    return this.http.post<ControlExecutionResult>(`${this.baseUrl}/controls/${id}/execute`, {});
  }

  executeAllControls(): Observable<ControlExecutionResult[]> {
    return this.http.post<ControlExecutionResult[]>(`${this.baseUrl}/controls/execute-all`, {});
  }

  previewSql(id: number): Observable<{ sql: string }> {
    return this.http.get<{ sql: string }>(`${this.baseUrl}/controls/${id}/preview-sql`);
  }
}
