-- ===========================================
-- הכנסת נתוני מטא-דאטה
-- מיפוי שמות עסקיים לשמות טכניים
-- ===========================================

USE DataControlsDB;
GO

-- ===========================================
-- הגדרת ישויות עסקיות
-- ===========================================

-- ניקוי נתונים קיימים
DELETE FROM ControlConditions;
DELETE FROM ControlJoins;
DELETE FROM ControlExecutions;
DELETE FROM Controls;
DELETE FROM EntityRelations;
DELETE FROM BusinessFields;
DELETE FROM BusinessEntities;
GO

-- הכנסת ישויות
INSERT INTO BusinessEntities (TableName, BusinessName, BusinessDescription, PrimaryKeyColumn)
VALUES 
    ('Employees', N'עובדים', N'טבלת עובדי הארגון', 'EmployeeId'),
    ('Departments', N'מחלקות', N'טבלת מחלקות הארגון', 'DepartmentId'),
    ('Projects', N'פרויקטים', N'טבלת פרויקטים', 'ProjectId'),
    ('EmployeeProjects', N'שיוך עובדים לפרויקטים', N'טבלת קשר בין עובדים לפרויקטים', 'EmployeeProjectId');
GO

-- ===========================================
-- הגדרת שדות עסקיים
-- ===========================================

-- שדות טבלת עובדים
INSERT INTO BusinessFields (EntityId, ColumnName, BusinessName, DataType, IsRequired, DisplayOrder)
SELECT EntityId, 'EmployeeId', N'מזהה עובד', 'number', 1, 1 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'FirstName', N'שם פרטי', 'text', 1, 2 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'LastName', N'שם משפחה', 'text', 1, 3 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'IdNumber', N'תעודת זהות', 'text', 0, 4 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'BirthDate', N'תאריך לידה', 'date', 0, 5 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'HireDate', N'תאריך תחילת העסקה', 'date', 0, 6 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'EndDate', N'תאריך סיום העסקה', 'date', 0, 7 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'DepartmentId', N'מחלקה', 'number', 0, 8 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'Salary', N'שכר', 'number', 0, 9 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'Email', N'דואר אלקטרוני', 'text', 0, 10 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'Phone', N'טלפון', 'text', 0, 11 FROM BusinessEntities WHERE TableName = 'Employees'
UNION ALL
SELECT EntityId, 'IsActive', N'פעיל', 'boolean', 0, 12 FROM BusinessEntities WHERE TableName = 'Employees';
GO

-- שדות טבלת מחלקות
INSERT INTO BusinessFields (EntityId, ColumnName, BusinessName, DataType, IsRequired, DisplayOrder)
SELECT EntityId, 'DepartmentId', N'מזהה מחלקה', 'number', 1, 1 FROM BusinessEntities WHERE TableName = 'Departments'
UNION ALL
SELECT EntityId, 'DepartmentName', N'שם מחלקה', 'text', 1, 2 FROM BusinessEntities WHERE TableName = 'Departments'
UNION ALL
SELECT EntityId, 'ManagerName', N'שם מנהל', 'text', 0, 3 FROM BusinessEntities WHERE TableName = 'Departments'
UNION ALL
SELECT EntityId, 'Budget', N'תקציב', 'number', 0, 4 FROM BusinessEntities WHERE TableName = 'Departments'
UNION ALL
SELECT EntityId, 'IsActive', N'פעילה', 'boolean', 0, 5 FROM BusinessEntities WHERE TableName = 'Departments';
GO

-- שדות טבלת פרויקטים
INSERT INTO BusinessFields (EntityId, ColumnName, BusinessName, DataType, IsRequired, DisplayOrder)
SELECT EntityId, 'ProjectId', N'מזהה פרויקט', 'number', 1, 1 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'ProjectName', N'שם פרויקט', 'text', 1, 2 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'DepartmentId', N'מחלקה', 'number', 0, 3 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'StartDate', N'תאריך התחלה', 'date', 0, 4 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'EndDate', N'תאריך סיום', 'date', 0, 5 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'Budget', N'תקציב', 'number', 0, 6 FROM BusinessEntities WHERE TableName = 'Projects'
UNION ALL
SELECT EntityId, 'Status', N'סטטוס', 'text', 0, 7 FROM BusinessEntities WHERE TableName = 'Projects';
GO

-- שדות טבלת שיוך עובדים לפרויקטים
INSERT INTO BusinessFields (EntityId, ColumnName, BusinessName, DataType, IsRequired, DisplayOrder)
SELECT EntityId, 'EmployeeProjectId', N'מזהה שיוך', 'number', 1, 1 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'EmployeeId', N'עובד', 'number', 1, 2 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'ProjectId', N'פרויקט', 'number', 1, 3 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'Role', N'תפקיד', 'text', 0, 4 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'HoursAllocated', N'שעות מוקצות', 'number', 0, 5 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'StartDate', N'תאריך התחלה', 'date', 0, 6 FROM BusinessEntities WHERE TableName = 'EmployeeProjects'
UNION ALL
SELECT EntityId, 'EndDate', N'תאריך סיום', 'date', 0, 7 FROM BusinessEntities WHERE TableName = 'EmployeeProjects';
GO

-- ===========================================
-- הגדרת קשרים בין ישויות
-- ===========================================

INSERT INTO EntityRelations (SourceEntityId, TargetEntityId, SourceColumn, TargetColumn, RelationName, RelationType)
SELECT 
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Employees'),
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Departments'),
    'DepartmentId', 'DepartmentId', N'עובד שייך למחלקה', 'ManyToOne'
UNION ALL
SELECT 
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Projects'),
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Departments'),
    'DepartmentId', 'DepartmentId', N'פרויקט שייך למחלקה', 'ManyToOne'
UNION ALL
SELECT 
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'EmployeeProjects'),
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Employees'),
    'EmployeeId', 'EmployeeId', N'שיוך לעובד', 'ManyToOne'
UNION ALL
SELECT 
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'EmployeeProjects'),
    (SELECT EntityId FROM BusinessEntities WHERE TableName = 'Projects'),
    'ProjectId', 'ProjectId', N'שיוך לפרויקט', 'ManyToOne';
GO

PRINT 'Metadata inserted successfully!';
GO
