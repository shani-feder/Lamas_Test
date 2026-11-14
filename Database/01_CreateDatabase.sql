-- ===========================================
-- מערכת בקרות נתונים דינאמית
-- סקריפט יצירת בסיס הנתונים
-- ===========================================

-- יצירת בסיס הנתונים
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DataControlsDB')
BEGIN
    CREATE DATABASE DataControlsDB;
END
GO

USE DataControlsDB;
GO

-- ===========================================
-- חלק א': טבלאות נתונים עסקיים
-- ===========================================

-- טבלת מחלקות
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments (
        DepartmentId INT PRIMARY KEY IDENTITY(1,1),
        DepartmentName NVARCHAR(100) NOT NULL,
        ManagerName NVARCHAR(100),
        Budget DECIMAL(18,2),
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- טבלת עובדים
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        EmployeeId INT PRIMARY KEY IDENTITY(1,1),
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        IdNumber NVARCHAR(20),
        BirthDate DATE,
        HireDate DATE,
        EndDate DATE,
        DepartmentId INT FOREIGN KEY REFERENCES Departments(DepartmentId),
        Salary DECIMAL(18,2),
        Email NVARCHAR(100),
        Phone NVARCHAR(20),
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- טבלת פרויקטים
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Projects')
BEGIN
    CREATE TABLE Projects (
        ProjectId INT PRIMARY KEY IDENTITY(1,1),
        ProjectName NVARCHAR(100) NOT NULL,
        DepartmentId INT FOREIGN KEY REFERENCES Departments(DepartmentId),
        StartDate DATE,
        EndDate DATE,
        Budget DECIMAL(18,2),
        Status NVARCHAR(20), -- 'Active', 'Completed', 'Cancelled'
        CreatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- טבלת שיוך עובדים לפרויקטים
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeProjects')
BEGIN
    CREATE TABLE EmployeeProjects (
        EmployeeProjectId INT PRIMARY KEY IDENTITY(1,1),
        EmployeeId INT FOREIGN KEY REFERENCES Employees(EmployeeId),
        ProjectId INT FOREIGN KEY REFERENCES Projects(ProjectId),
        Role NVARCHAR(50),
        HoursAllocated INT,
        StartDate DATE,
        EndDate DATE
    );
END
GO

-- ===========================================
-- חלק ב': טבלאות מטא-דאטה (תצורת המערכת)
-- ===========================================

-- טבלת ישויות עסקיות (מיפוי טבלאות לשמות עסקיים)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessEntities')
BEGIN
    CREATE TABLE BusinessEntities (
        EntityId INT PRIMARY KEY IDENTITY(1,1),
        TableName NVARCHAR(100) NOT NULL,          -- שם הטבלה הטכני
        BusinessName NVARCHAR(100) NOT NULL,       -- שם עסקי בעברית
        BusinessDescription NVARCHAR(500),         -- תיאור הישות
        PrimaryKeyColumn NVARCHAR(100),            -- עמודת מפתח ראשי
        IsActive BIT DEFAULT 1
    );
END
GO

-- טבלת שדות עסקיים (מיפוי עמודות לשמות עסקיים)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BusinessFields')
BEGIN
    CREATE TABLE BusinessFields (
        FieldId INT PRIMARY KEY IDENTITY(1,1),
        EntityId INT FOREIGN KEY REFERENCES BusinessEntities(EntityId),
        ColumnName NVARCHAR(100) NOT NULL,         -- שם העמודה הטכני
        BusinessName NVARCHAR(100) NOT NULL,       -- שם עסקי בעברית
        DataType NVARCHAR(50),                     -- סוג הנתון (text, number, date, boolean)
        IsRequired BIT DEFAULT 0,                  -- האם שדה חובה
        IsSearchable BIT DEFAULT 1,                -- האם ניתן לחפש לפיו
        DisplayOrder INT DEFAULT 0
    );
END
GO

-- טבלת קשרים בין ישויות
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EntityRelations')
BEGIN
    CREATE TABLE EntityRelations (
        RelationId INT PRIMARY KEY IDENTITY(1,1),
        SourceEntityId INT FOREIGN KEY REFERENCES BusinessEntities(EntityId),
        TargetEntityId INT FOREIGN KEY REFERENCES BusinessEntities(EntityId),
        SourceColumn NVARCHAR(100),                -- עמודת המפתח הזר בטבלת המקור
        TargetColumn NVARCHAR(100),                -- עמודת המפתח בטבלת היעד
        RelationName NVARCHAR(100),                -- שם הקשר בעברית
        RelationType NVARCHAR(20)                  -- 'OneToMany', 'ManyToOne', 'ManyToMany'
    );
END
GO

-- ===========================================
-- חלק ג': טבלאות הגדרת בקרות
-- ===========================================

-- טבלת בקרות
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Controls')
BEGIN
    CREATE TABLE Controls (
        ControlId INT PRIMARY KEY IDENTITY(1,1),
        ControlName NVARCHAR(200) NOT NULL,        -- שם הבקרה
        ControlDescription NVARCHAR(1000),         -- תיאור הבקרה
        PrimaryEntityId INT FOREIGN KEY REFERENCES BusinessEntities(EntityId), -- הישות הראשית
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100),
        LastModifiedDate DATETIME,
        LastModifiedBy NVARCHAR(100)
    );
END
GO

-- טבלת תנאי בקרה
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ControlConditions')
BEGIN
    CREATE TABLE ControlConditions (
        ConditionId INT PRIMARY KEY IDENTITY(1,1),
        ControlId INT FOREIGN KEY REFERENCES Controls(ControlId),
        FieldId INT FOREIGN KEY REFERENCES BusinessFields(FieldId),
        Operator NVARCHAR(20) NOT NULL,            -- '=', '!=', '>', '<', '>=', '<=', 'LIKE', 'IS NULL', 'IS NOT NULL', 'IN'
        Value NVARCHAR(500),                       -- הערך להשוואה
        ValueType NVARCHAR(20),                    -- 'static', 'field', 'parameter'
        LogicalOperator NVARCHAR(10) DEFAULT 'AND', -- 'AND', 'OR'
        GroupNumber INT DEFAULT 0,                 -- לקיבוץ תנאים בסוגריים
        OrderIndex INT DEFAULT 0                   -- סדר התנאים
    );
END
GO

-- טבלת JOIN-ים לבקרה
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ControlJoins')
BEGIN
    CREATE TABLE ControlJoins (
        JoinId INT PRIMARY KEY IDENTITY(1,1),
        ControlId INT FOREIGN KEY REFERENCES Controls(ControlId),
        RelationId INT FOREIGN KEY REFERENCES EntityRelations(RelationId),
        JoinType NVARCHAR(20) DEFAULT 'INNER'      -- 'INNER', 'LEFT', 'RIGHT'
    );
END
GO

-- טבלת היסטוריית הרצות
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ControlExecutions')
BEGIN
    CREATE TABLE ControlExecutions (
        ExecutionId INT PRIMARY KEY IDENTITY(1,1),
        ControlId INT FOREIGN KEY REFERENCES Controls(ControlId),
        ExecutionDate DATETIME DEFAULT GETDATE(),
        ExecutedBy NVARCHAR(100),
        TotalRecordsChecked INT,
        FailedRecordsCount INT,
        ExecutionTimeMs INT,
        Status NVARCHAR(20),                       -- 'Success', 'Failed', 'Error'
        ErrorMessage NVARCHAR(MAX),
        GeneratedSQL NVARCHAR(MAX)                 -- השאילתה שנוצרה (לצורכי דיבוג)
    );
END
GO

PRINT 'Database schema created successfully!';
GO
