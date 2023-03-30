using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MessagePack;
using MongoDB.Bson;
using OfficeOpenXml;
using Realms;
using UnityEditor;
using UnityEngine;

public static class ExcelExporter
{
    private const string excelDir = "./Excels";

    private static Realm m_realm_export;
    
    [MenuItem("Tools/导出Excels数据")] 
    
    public static void Export()
    {
        Debug.Log(Application.persistentDataPath);
        var config  = new RealmConfiguration(Application.persistentDataPath + "/db.realm")
        {
            ShouldDeleteIfMigrationNeeded = true
        };
        
        m_realm_export = Realm.GetInstance(config);

        m_realm_export.Write(() => m_realm_export.RemoveAll());

        foreach (string path in Directory.GetFiles(excelDir, "*.xlsx"))
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    foreach (ExcelWorksheet sheet in package.Workbook.Worksheets)
                    {
                        UnityEngine.Debug.Log(sheet.Name);
                        ExportClassJSON(sheet);
                    }
                }
            }
        }
    }

    static List<ClassField> ExportClassField(ExcelWorksheet sheet) {
        List<ClassField> fields = new List<ClassField>();
        Type classType = Type.GetType($"Model.{sheet.Name}, Assembly-CSharp");
        const int headRow = 1;

        for (int col = 1; col <= sheet.Dimension.End.Column; ++col) {
            string[] fieldHead = sheet.Cells[headRow, col].Text.Split('$');
            string fieldName = fieldHead.Length == 2 ? fieldHead[0] : fieldHead[1];
            string fieldType = fieldHead.Length == 2 ? fieldHead[1] : fieldHead[2];
            PropertyInfo fieldInfo = classType?.GetProperty(fieldName);
            if (fieldInfo != null) {
                ClassField previous = fields.Find((ClassField field) => (field.FieldName == fieldName && field.FieldType == fieldType));
                if (previous != null) {
                    previous.FieldColumn.Add(col);
                } else {
                    fields.Add(new ClassField(fieldName, fieldType, fieldInfo, col));
                }
            }
        }
        return fields;
    }

    static void ExportClassJSON(ExcelWorksheet sheet)
    {
        Type classType = Type.GetType($"Model.{sheet.Name}, Assembly-CSharp");

        if (classType == null)
        {
            UnityEngine.Debug.LogWarning($"Model.{sheet.Name} Not Exist!");
            return;
        }
        
        List<ClassField> fields = ExportClassField(sheet);

        m_realm_export.Write(() =>
        {
            for (int row = 2; row <= sheet.Dimension.End.Row; ++row)
            {
                var obj = Activator.CreateInstance(classType);
                
                Debug.Log(fields.Count);
                foreach (ClassField field in fields)
                {
                    var value = GetRowFieldValue(sheet, field, row);
                    Debug.Log($"{field.FieldName} {value is IList} {value.ToJson()}");
                    if (field.FieldInfo.CanWrite)
                    {
                        field.FieldInfo.SetValue(obj, GetRowFieldValue(sheet, field, row));
                    } else if (field.FieldInfo.PropertyType.Name.StartsWith("IList") && value is IList list)
                    {
                        foreach (var item in list)
                        {
                            var objList = field.FieldInfo.GetMethod.Invoke(obj, null);
                            objList.GetType().GetMethod("Add")?.Invoke(objList, new[] {item});
                        }
                        
                    }
                }

                m_realm_export.Add(obj as IRealmObject);
            }
        });
        // string value = MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(list.GetType(), list));
    }

    static object GetRowFieldValue(ExcelWorksheet sheet, ClassField field, int row) {
        if (field.FieldColumn.Count == 1) {
            return Convert(field.FieldInfo, sheet.Cells[row, field.FieldColumn[0]].Text.Trim());
        } else {
            List<string> values = new List<string>();
            foreach(int col in field.FieldColumn) {
                string value = sheet.Cells[row, col].Text.Trim();
                if (field.FieldInfo.PropertyType.Name == "String[]") {
                    values.Add($"\"{value.Replace("\"", "\\\"")}\"");
                } else if (value.Length > 0) {
                    values.Add(value);
                }
            }
            return Convert(field.FieldInfo, string.Join(",", values));
        }
    }

    static object Convert(PropertyInfo fieldInfo, string value) {
        string jsonValue;


        
        if (fieldInfo.PropertyType.Name.StartsWith("IList"))
        {
            jsonValue = $"[{value}]";
        }
        else
        {
            switch (fieldInfo.PropertyType.Name) {
                case "String":
                    jsonValue = $"\"{value.Replace("\"", "\\\"")}\"";
                    break;
                default:
                    jsonValue = value;
                    break;
            }
        }
        //Debug.Log(fieldInfo.PropertyType.Name);
        // Debug.Log(jsonValue);
        return MessagePackSerializer.Deserialize(fieldInfo.PropertyType, MessagePackSerializer.ConvertFromJson(jsonValue));
    }

    class ClassField {
        public string FieldName;
        public string FieldType;
        public PropertyInfo FieldInfo;
        public List<int> FieldColumn = new List<int>();

        public ClassField(string name, string type, PropertyInfo info, int column) {
            FieldName = name;
            FieldType = type;
            FieldInfo = info;
            FieldColumn.Add(column);
        }
    }
}