# מערכת בקרות נתונים דינאמית

מערכת להגדרה והרצה של בקרות נתונים באופן דינאמי, ללא צורך בכתיבת קוד.

## טכנולוגיות
- **Backend:** .NET 8 Web API, Python FastAPI
- **Frontend:** Angular 17
- **Database:** SQL Server

## מבנה הפרויקט

```
├── Database/                    # סקריפטים ליצירת בסיס הנתונים
├── Backend/
│   ├── DataControlsAPI/         # API ב-C# .NET 8
│   └── PythonService/           # שירות Python להצעות AI
├── Frontend/                    # ממשק משתמש ב-Angular 17
└── Docs/                        # תיעוד
```

## התקנה והפעלה

### 1. בסיס הנתונים (SQL Server)
הרץ את הסקריפטים בסדר הבא:
```sql
01_CreateDatabase.sql    -- יצירת הטבלאות
02_InsertMetadata.sql    -- הכנסת מטא-דאטה
03_InsertSampleData.sql  -- נתוני דוגמה
04_InsertSampleControls.sql -- בקרות דוגמה
```

### 2. Backend (.NET)
```bash
cd Backend/DataControlsAPI
dotnet restore
dotnet run
```
ה-API יעלה על `http://localhost:5000`

### 3. Backend (Python - אופציונלי, להצעות AI)
```bash
cd Backend/PythonService
pip install -r requirements.txt
python main.py
```
השירות יעלה על `http://localhost:5001`

### 4. Frontend
```bash
cd Frontend
npm install
npm start
```
האפליקציה תעלה על `http://localhost:4200`

## API Endpoints

### Metadata
- `GET /api/metadata/entities` - רשימת ישויות
- `GET /api/metadata/entities/{id}/fields` - שדות של ישות
- `GET /api/metadata/relations` - קשרים בין ישויות

### Controls
- `GET /api/controls` - רשימת בקרות
- `GET /api/controls/{id}` - פרטי בקרה
- `POST /api/controls` - יצירת בקרה
- `PUT /api/controls/{id}` - עדכון בקרה
- `DELETE /api/controls/{id}` - מחיקת בקרה
- `POST /api/controls/{id}/execute` - הרצת בקרה
- `POST /api/controls/execute-all` - הרצת כל הבקרות
- `GET /api/controls/{id}/preview-sql` - תצוגה מקדימה של SQL

### AI (Python Service)
- `GET /api/ai/analyze` - ניתוח והצעת בקרות
- `GET /api/ai/suggestions/{entity_id}` - הצעות לישות ספציפית

## בקרות דוגמה

1. **עובדים קטינים עם שכר גבוה** - איתור עובדים מתחת לגיל 18 עם שכר מעל 15,000
2. **תאריכי העסקה לא תקינים** - תאריך סיום לפני תאריך התחלה
3. **עובדים פעילים במחלקה לא פעילה**
4. **עובדים ללא אימייל**
5. **עובדים ללא שיוך למחלקה**
6. **שכר חריג** - שכר מעל 50,000

## תכונות המערכת

### הגדרת בקרות
- בחירת ישות ראשית בשמות עסקיים
- הוספת קשרים (JOINs) לישויות נוספות
- הגדרת תנאים מרובים עם AND/OR
- תצוגה מקדימה של SQL

### הרצת בקרות
- הרצה בודדת או הרצת כל הבקרות
- שמירת היסטוריית הרצות
- מדידת זמני ביצוע

### תוצאות
- הצגת רשומות חריגות בטבלה
- סיבת כישלון לכל רשומה
- סינון וחיפוש בתוצאות
- מיון לפי עמודות
- ייצוא ל-CSV

### הצעות AI (משימה מתקדמת)
- ניתוח אוטומטי של מבנה הנתונים
- הצעת בקרות לפי קטגוריות:
  - איכות נתונים (שדות ריקים, תאריכים לא תקינים)
  - כללים עסקיים (שכר שלילי, גיל לא תקין)
  - עקביות (רשומות פעילות מקושרות ללא פעילות)

## שימוש בכלי AI

במהלך הפיתוח נעשה שימוש ב-**Amazon Q Developer** למטרות:
- יצירת מבנה הפרויקט
- כתיבת קוד Backend ו-Frontend
- יצירת סקריפטי SQL
- כתיבת תיעוד

## הנחות עבודה

1. המשתמשים מכירים את המונחים העסקיים
2. מבנה הטבלאות יציב יחסית
3. SQL Server מותקן ומוגדר
4. Node.js ו-.NET SDK מותקנים

## מגבלות

1. אין תמיכה ב-Subqueries מורכבים
2. אין תמיכה ב-Aggregations (GROUP BY)
3. מוגבל לקשרים שהוגדרו במטא-דאטה
4. אין מנגנון הרשאות

## הרחבות אפשריות

1. תזמון בקרות אוטומטי
2. התראות במייל
3. דשבורד סיכום
4. השוואת תוצאות לאורך זמן
5. ניהול משתמשים והרשאות
6. תמיכה ב-Aggregations
