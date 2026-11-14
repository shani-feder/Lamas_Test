-- ===========================================
-- הכנסת בקרות דוגמה
-- ===========================================

USE DataControlsDB;
GO

-- ניקוי בקרות קיימות
DELETE FROM ControlConditions;
DELETE FROM ControlJoins;
DELETE FROM Controls;
GO

-- ===========================================
-- בקרה 1: עובדים צעירים מדי עם שכר גבוה
-- ===========================================
DECLARE @ControlId1 INT;
DECLARE @EmployeeEntityId INT = (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Employees');

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'עובדים קטינים עם שכר גבוה',
    N'איתור עובדים שגילם נמוך מ-18 ושכרם גבוה מ-15,000 ש"ח',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId1 = SCOPE_IDENTITY();

-- תנאי 1: גיל < 18 (תאריך לידה אחרי היום פחות 18 שנה)
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId1, FieldId, '>', 'DATEADD(YEAR, -18, GETDATE())', 'expression', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'BirthDate' AND EntityId = @EmployeeEntityId;

-- תנאי 2: שכר > 15000
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId1, FieldId, '>', '15000', 'static', 'AND', 0, 2
FROM BusinessFields WHERE ColumnName = 'Salary' AND EntityId = @EmployeeEntityId;

-- ===========================================
-- בקרה 2: תאריך סיום לפני תאריך התחלה
-- ===========================================
DECLARE @ControlId2 INT;

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'תאריכי העסקה לא תקינים',
    N'איתור עובדים שתאריך סיום העסקתם מוקדם מתאריך תחילת העסקתם',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId2 = SCOPE_IDENTITY();

-- תנאי: EndDate < HireDate AND EndDate IS NOT NULL
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId2, FieldId, 'IS NOT NULL', NULL, 'static', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'EndDate' AND EntityId = @EmployeeEntityId;

INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId2, f1.FieldId, '<', 'HireDate', 'field', 'AND', 0, 2
FROM BusinessFields f1
WHERE f1.ColumnName = 'EndDate' AND f1.EntityId = @EmployeeEntityId;

-- ===========================================
-- בקרה 3: עובדים פעילים במחלקה לא פעילה
-- ===========================================
DECLARE @ControlId3 INT;
DECLARE @DeptEntityId INT = (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Departments');

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'עובדים פעילים במחלקה לא פעילה',
    N'איתור עובדים פעילים המשויכים למחלקה שאינה פעילה',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId3 = SCOPE_IDENTITY();

-- הוספת JOIN למחלקות
INSERT INTO ControlJoins (ControlId, RelationId, JoinType)
SELECT @ControlId3, RelationId, 'INNER'
FROM EntityRelations 
WHERE SourceEntityId = @EmployeeEntityId AND TargetEntityId = @DeptEntityId;

-- תנאי 1: עובד פעיל
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId3, FieldId, '=', '1', 'static', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'IsActive' AND EntityId = @EmployeeEntityId;

-- תנאי 2: מחלקה לא פעילה
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId3, FieldId, '=', '0', 'static', 'AND', 0, 2
FROM BusinessFields WHERE ColumnName = 'IsActive' AND EntityId = @DeptEntityId;

-- ===========================================
-- בקרה 4: עובדים ללא אימייל
-- ===========================================
DECLARE @ControlId4 INT;

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'עובדים ללא כתובת אימייל',
    N'איתור עובדים פעילים שחסרה להם כתובת דואר אלקטרוני',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId4 = SCOPE_IDENTITY();

-- תנאי 1: עובד פעיל
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId4, FieldId, '=', '1', 'static', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'IsActive' AND EntityId = @EmployeeEntityId;

-- תנאי 2: אימייל ריק
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId4, FieldId, 'IS NULL', NULL, 'static', 'AND', 0, 2
FROM BusinessFields WHERE ColumnName = 'Email' AND EntityId = @EmployeeEntityId;

-- ===========================================
-- בקרה 5: עובדים ללא מחלקה
-- ===========================================
DECLARE @ControlId5 INT;

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'עובדים ללא שיוך למחלקה',
    N'איתור עובדים פעילים שאינם משויכים לאף מחלקה',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId5 = SCOPE_IDENTITY();

-- תנאי 1: עובד פעיל
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId5, FieldId, '=', '1', 'static', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'IsActive' AND EntityId = @EmployeeEntityId;

-- תנאי 2: מחלקה ריקה
INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId5, FieldId, 'IS NULL', NULL, 'static', 'AND', 0, 2
FROM BusinessFields WHERE ColumnName = 'DepartmentId' AND EntityId = @EmployeeEntityId;

-- ===========================================
-- בקרה 6: שכר חריג
-- ===========================================
DECLARE @ControlId6 INT;

INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
VALUES (
    N'שכר חריג',
    N'איתור עובדים עם שכר גבוה מ-50,000 ש"ח',
    @EmployeeEntityId,
    N'מנהל מערכת'
);
SET @ControlId6 = SCOPE_IDENTITY();

INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
SELECT @ControlId6, FieldId, '>', '50000', 'static', 'AND', 0, 1
FROM BusinessFields WHERE ColumnName = 'Salary' AND EntityId = @EmployeeEntityId;

PRINT 'Sample controls inserted successfully!';
GO
