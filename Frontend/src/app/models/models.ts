export interface BusinessEntity {
  entityId: number;
  tableName: string;
  businessName: string;
  businessDescription?: string;
  primaryKeyColumn?: string;
  isActive: boolean;
  fields?: BusinessField[];
}

export interface BusinessField {
  fieldId: number;
  entityId: number;
  columnName: string;
  businessName: string;
  dataType?: string;
  isRequired: boolean;
  isSearchable: boolean;
  displayOrder: number;
  entityBusinessName?: string;
  tableName?: string;
}

export interface EntityRelation {
  relationId: number;
  sourceEntityId: number;
  targetEntityId: number;
  sourceColumn?: string;
  targetColumn?: string;
  relationName?: string;
  relationType?: string;
  sourceTableName?: string;
  targetTableName?: string;
  sourceEntityName?: string;
  targetEntityName?: string;
}

export interface Control {
  controlId: number;
  controlName: string;
  controlDescription?: string;
  primaryEntityId: number;
  isActive: boolean;
  createdDate: Date;
  createdBy?: string;
  primaryEntityName?: string;
  conditions: ControlCondition[];
  joins: ControlJoin[];
}

export interface ControlCondition {
  conditionId?: number;
  controlId?: number;
  fieldId: number;
  operator: string;
  value?: string;
  valueType: string;
  logicalOperator: string;
  groupNumber: number;
  orderIndex: number;
  fieldBusinessName?: string;
  columnName?: string;
  tableName?: string;
  dataType?: string;
}

export interface ControlJoin {
  joinId?: number;
  controlId?: number;
  relationId: number;
  joinType: string;
}

export interface ControlSummary {
  controlId: number;
  controlName: string;
  controlDescription?: string;
  primaryEntityName?: string;
  isActive: boolean;
  createdDate: Date;
  lastExecutionFailedCount?: number;
  lastExecutionDate?: Date;
}

export interface ControlExecutionResult {
  controlId: number;
  controlName: string;
  totalRecordsChecked: number;
  failedRecordsCount: number;
  executionTimeMs: number;
  status: string;
  errorMessage?: string;
  violations: ViolationRecord[];
  columns: ColumnInfo[];
}

export interface ViolationRecord {
  data: { [key: string]: any };
  reasons: string[];
}

export interface ColumnInfo {
  columnName: string;
  businessName: string;
  dataType: string;
}

export interface OperatorInfo {
  value: string;
  displayName: string;
  requiresValue: boolean;
  applicableTypes: string[];
}
