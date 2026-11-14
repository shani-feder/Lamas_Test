"""
Python Service - AI Control Suggestions
שירות Python לניתוח מבנה בסיס הנתונים והצעת בקרות באופן אוטומטי
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import pyodbc
from typing import Optional
import os

app = FastAPI(title="Data Controls AI Service", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200", "http://localhost:5000"],
    allow_methods=["*"],
    allow_headers=["*"],
)

CONNECTION_STRING = os.getenv(
    "DB_CONNECTION",
    "Driver={ODBC Driver 17 for SQL Server};Server=localhost;Database=DataControlsDB;Trusted_Connection=yes;"
)

class ControlSuggestion(BaseModel):
    name: str
    description: str
    entity_name: str
    conditions: list
    severity: str  # high, medium, low
    category: str  # data_quality, business_rule, consistency

class AnalysisResult(BaseModel):
    suggestions: list[ControlSuggestion]
    analyzed_tables: int
    analyzed_fields: int

def get_db_connection():
    return pyodbc.connect(CONNECTION_STRING)

def get_metadata():
    """קריאת מטא-דאטה מבסיס הנתונים"""
    conn = get_db_connection()
    cursor = conn.cursor()
    
    # קריאת ישויות
    cursor.execute("""
        SELECT EntityId, TableName, BusinessName, PrimaryKeyColumn 
        FROM BusinessEntities WHERE IsActive = 1
    """)
    entities = [{"id": r[0], "table": r[1], "name": r[2], "pk": r[3]} for r in cursor.fetchall()]
    
    # קריאת שדות
    cursor.execute("""
        SELECT f.FieldId, f.EntityId, f.ColumnName, f.BusinessName, f.DataType, f.IsRequired, e.TableName
        FROM BusinessFields f
        JOIN BusinessEntities e ON f.EntityId = e.EntityId
    """)
    fields = [{"id": r[0], "entity_id": r[1], "column": r[2], "name": r[3], 
               "type": r[4], "required": r[5], "table": r[6]} for r in cursor.fetchall()]
    
    # קריאת קשרים
    cursor.execute("""
        SELECT r.RelationId, r.SourceEntityId, r.TargetEntityId, r.SourceColumn, r.TargetColumn,
               s.TableName as SourceTable, t.TableName as TargetTable
        FROM EntityRelations r
        JOIN BusinessEntities s ON r.SourceEntityId = s.EntityId
        JOIN BusinessEntities t ON r.TargetEntityId = t.EntityId
    """)
    relations = [{"id": r[0], "source_id": r[1], "target_id": r[2], 
                  "source_col": r[3], "target_col": r[4],
                  "source_table": r[5], "target_table": r[6]} for r in cursor.fetchall()]
    
    conn.close()
    return entities, fields, relations

def analyze_for_null_checks(fields, entities):
    """הצעת בקרות לשדות חובה ריקים"""
    suggestions = []
    for field in fields:
        if field["required"] or field["column"] in ["Email", "Phone", "IdNumber"]:
            entity = next((e for e in entities if e["id"] == field["entity_id"]), None)
            if entity:
                suggestions.append(ControlSuggestion(
                    name=f"{field['name']} ריק ב{entity['name']}",
                    description=f"איתור רשומות ב{entity['name']} שבהן השדה {field['name']} ריק",
                    entity_name=entity["name"],
                    conditions=[{"field": field["name"], "operator": "IS NULL"}],
                    severity="medium" if not field["required"] else "high",
                    category="data_quality"
                ))
    return suggestions

def analyze_for_date_validations(fields, entities):
    """הצעת בקרות לתאריכים לא תקינים"""
    suggestions = []
    date_fields = [f for f in fields if f["type"] == "date"]
    
    # קיבוץ לפי ישות
    entity_dates = {}
    for f in date_fields:
        if f["entity_id"] not in entity_dates:
            entity_dates[f["entity_id"]] = []
        entity_dates[f["entity_id"]].append(f)
    
    for entity_id, dates in entity_dates.items():
        entity = next((e for e in entities if e["id"] == entity_id), None)
        if not entity:
            continue
            
        # בדיקת תאריך התחלה/סיום
        start_date = next((d for d in dates if "Start" in d["column"] or "Hire" in d["column"]), None)
        end_date = next((d for d in dates if "End" in d["column"]), None)
        
        if start_date and end_date:
            suggestions.append(ControlSuggestion(
                name=f"תאריכים לא תקינים ב{entity['name']}",
                description=f"איתור רשומות שבהן {end_date['name']} מוקדם מ{start_date['name']}",
                entity_name=entity["name"],
                conditions=[
                    {"field": end_date["name"], "operator": "IS NOT NULL"},
                    {"field": end_date["name"], "operator": "<", "compare_field": start_date["name"]}
                ],
                severity="high",
                category="data_quality"
            ))
        
        # בדיקת תאריך לידה עתידי
        birth_date = next((d for d in dates if "Birth" in d["column"]), None)
        if birth_date:
            suggestions.append(ControlSuggestion(
                name=f"תאריך לידה עתידי ב{entity['name']}",
                description=f"איתור רשומות עם תאריך לידה עתידי",
                entity_name=entity["name"],
                conditions=[{"field": birth_date["name"], "operator": ">", "value": "TODAY"}],
                severity="high",
                category="data_quality"
            ))
    
    return suggestions

def analyze_for_numeric_validations(fields, entities):
    """הצעת בקרות לערכים מספריים חריגים"""
    suggestions = []
    numeric_fields = [f for f in fields if f["type"] == "number"]
    
    for field in numeric_fields:
        entity = next((e for e in entities if e["id"] == field["entity_id"]), None)
        if not entity:
            continue
            
        # בדיקת ערכים שליליים
        if field["column"] in ["Salary", "Budget", "HoursAllocated", "Price", "Amount"]:
            suggestions.append(ControlSuggestion(
                name=f"{field['name']} שלילי ב{entity['name']}",
                description=f"איתור רשומות עם {field['name']} שלילי",
                entity_name=entity["name"],
                conditions=[{"field": field["name"], "operator": "<", "value": "0"}],
                severity="high",
                category="business_rule"
            ))
            
        # בדיקת שכר חריג
        if field["column"] == "Salary":
            suggestions.append(ControlSuggestion(
                name=f"שכר חריג ב{entity['name']}",
                description=f"איתור רשומות עם שכר גבוה באופן חריג (מעל 50,000)",
                entity_name=entity["name"],
                conditions=[{"field": field["name"], "operator": ">", "value": "50000"}],
                severity="medium",
                category="business_rule"
            ))
    
    return suggestions

def analyze_for_referential_integrity(fields, entities, relations):
    """הצעת בקרות לעקביות בין טבלאות"""
    suggestions = []
    
    for relation in relations:
        source_entity = next((e for e in entities if e["id"] == relation["source_id"]), None)
        target_entity = next((e for e in entities if e["id"] == relation["target_id"]), None)
        
        if source_entity and target_entity:
            # בדיקת רשומות פעילות מקושרות לרשומות לא פעילות
            source_active = next((f for f in fields if f["entity_id"] == relation["source_id"] 
                                  and f["column"] == "IsActive"), None)
            target_active = next((f for f in fields if f["entity_id"] == relation["target_id"] 
                                  and f["column"] == "IsActive"), None)
            
            if source_active and target_active:
                suggestions.append(ControlSuggestion(
                    name=f"{source_entity['name']} פעילים ב{target_entity['name']} לא פעילה",
                    description=f"איתור {source_entity['name']} פעילים המשויכים ל{target_entity['name']} לא פעילה",
                    entity_name=source_entity["name"],
                    conditions=[
                        {"field": "פעיל", "operator": "=", "value": "1"},
                        {"field": f"{target_entity['name']}.פעילה", "operator": "=", "value": "0"}
                    ],
                    severity="high",
                    category="consistency"
                ))
    
    return suggestions

def analyze_for_age_validations(fields, entities):
    """הצעת בקרות לגיל"""
    suggestions = []
    
    for entity in entities:
        birth_field = next((f for f in fields if f["entity_id"] == entity["id"] 
                           and f["column"] == "BirthDate"), None)
        salary_field = next((f for f in fields if f["entity_id"] == entity["id"] 
                            and f["column"] == "Salary"), None)
        
        if birth_field:
            # קטינים
            suggestions.append(ControlSuggestion(
                name=f"קטינים ב{entity['name']}",
                description=f"איתור רשומות של אנשים מתחת לגיל 18",
                entity_name=entity["name"],
                conditions=[{"field": birth_field["name"], "operator": ">", "value": "18_YEARS_AGO"}],
                severity="medium",
                category="business_rule"
            ))
            
            # קטינים עם שכר גבוה
            if salary_field:
                suggestions.append(ControlSuggestion(
                    name=f"קטינים עם שכר גבוה ב{entity['name']}",
                    description=f"איתור קטינים (מתחת לגיל 18) עם שכר גבוה מ-15,000",
                    entity_name=entity["name"],
                    conditions=[
                        {"field": birth_field["name"], "operator": ">", "value": "18_YEARS_AGO"},
                        {"field": salary_field["name"], "operator": ">", "value": "15000"}
                    ],
                    severity="high",
                    category="business_rule"
                ))
    
    return suggestions

@app.get("/api/ai/analyze", response_model=AnalysisResult)
async def analyze_database():
    """ניתוח בסיס הנתונים והצעת בקרות"""
    try:
        entities, fields, relations = get_metadata()
        
        suggestions = []
        suggestions.extend(analyze_for_null_checks(fields, entities))
        suggestions.extend(analyze_for_date_validations(fields, entities))
        suggestions.extend(analyze_for_numeric_validations(fields, entities))
        suggestions.extend(analyze_for_referential_integrity(fields, entities, relations))
        suggestions.extend(analyze_for_age_validations(fields, entities))
        
        # הסרת כפילויות
        unique_suggestions = []
        seen_names = set()
        for s in suggestions:
            if s.name not in seen_names:
                seen_names.add(s.name)
                unique_suggestions.append(s)
        
        return AnalysisResult(
            suggestions=unique_suggestions,
            analyzed_tables=len(entities),
            analyzed_fields=len(fields)
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/ai/suggestions/{entity_id}")
async def get_suggestions_for_entity(entity_id: int):
    """הצעות בקרות לישות ספציפית"""
    try:
        entities, fields, relations = get_metadata()
        entity = next((e for e in entities if e["id"] == entity_id), None)
        
        if not entity:
            raise HTTPException(status_code=404, detail="Entity not found")
        
        entity_fields = [f for f in fields if f["entity_id"] == entity_id]
        entity_relations = [r for r in relations if r["source_id"] == entity_id or r["target_id"] == entity_id]
        
        suggestions = []
        suggestions.extend(analyze_for_null_checks(entity_fields, [entity]))
        suggestions.extend(analyze_for_date_validations(entity_fields, [entity]))
        suggestions.extend(analyze_for_numeric_validations(entity_fields, [entity]))
        suggestions.extend(analyze_for_referential_integrity(entity_fields, [entity], entity_relations))
        suggestions.extend(analyze_for_age_validations(entity_fields, [entity]))
        
        return {"entity": entity["name"], "suggestions": suggestions}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "AI Control Suggestions"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5001)
