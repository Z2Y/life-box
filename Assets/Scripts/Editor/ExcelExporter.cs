using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MessagePack;
using OfficeOpenXml;
using UnityEditor;

public static class ExcelExporter
{
    private const string excelDir = "./Excels";
    private const string outputDir = "./Assets/Resources/Models/";

    [MenuItem("Tools/导出Excels数据")]
    public static void Export()
    {
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
            FieldInfo fieldInfo = classType.GetField(fieldName);
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

    static void ExportClassJSON(ExcelWorksheet sheet) {
        Type classType = Type.GetType($"Model.{sheet.Name}, Assembly-CSharp");

        if (classType == null) {
            UnityEngine.Debug.LogWarning($"Model.{sheet.Name} Not Exist!");
            return;
        }

        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(classType));
        List<ClassField> fields = ExportClassField(sheet);

        for (int row = 2; row <= sheet.Dimension.End.Row; ++row) {
            var obj = Activator.CreateInstance(classType);
            foreach(ClassField field in fields) {
                field.FieldInfo.SetValue(obj, GetRowFieldValue(sheet, field, row));
            }
            list.Add(obj);
        }

        string outputPath = $"{outputDir}{sheet.Name}.bytes";
        if (Directory.Exists(outputPath)) {
            Directory.Delete(outputPath);
        }
        using(FileStream fs = new FileStream(outputPath, FileMode.OpenOrCreate)) {
            MessagePackSerializer.Serialize(list.GetType(), fs, list);
        }
        // string value = MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(list.GetType(), list));
    }

    static object GetRowFieldValue(ExcelWorksheet sheet, ClassField field, int row) {
        if (field.FieldColumn.Count == 1) {
            return Convert(field.FieldInfo, sheet.Cells[row, field.FieldColumn[0]].Text.Trim());
        } else {
            List<string> values = new List<string>();
            foreach(int col in field.FieldColumn) {
                string value = sheet.Cells[row, col].Text.Trim();
                if (field.FieldInfo.FieldType.Name == "String[]") {
                    values.Add($"\"{value.Replace("\"", "\\\"")}\"");
                } else if (value.Length > 0) {
                    values.Add(value);
                }
            }
            return Convert(field.FieldInfo, String.Join(",", values));
        }
    }

    static object Convert(FieldInfo fieldInfo, string value) {
        string jsonValue;
        switch (fieldInfo.FieldType.Name) {
            case "Int64[]":
            case "Int32[]":
            case "int[]":
            case "Single[]":
            case "String[]":
                jsonValue = $"[{value}]";
                break;
            case "String":
                jsonValue = $"\"{value.Replace("\"", "\\\"")}\"";
                break;
            default:
                jsonValue = value;
                break;
        }
        UnityEngine.Debug.Log(jsonValue);
        return MessagePackSerializer.Deserialize(fieldInfo.FieldType, MessagePackSerializer.ConvertFromJson(jsonValue));
    }

    class ClassField {
        public string FieldName;
        public string FieldType;
        public FieldInfo FieldInfo;
        public List<int> FieldColumn = new List<int>();

        public ClassField(string name, string type, FieldInfo info, int column) {
            FieldName = name;
            FieldType = type;
            FieldInfo = info;
            FieldColumn.Add(column);
        }
    }
}