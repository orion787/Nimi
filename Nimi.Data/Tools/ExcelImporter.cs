using System.Reflection;
using ExcelDataReader;
using Nimi.Core.Entities;
using Nimi.Data.Repositories;

namespace Nimi.Data.Tools
{
    public static class ExcelImporter
    {
        public static int Import<T>(
            string filePath,
            INimiRepository<T> context,
            Dictionary<string, string>? columnMapping = null,
            int sheetIndex = 0
        ) where T : EntityBase, new()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream); //HERE !!!!

            var result = reader.AsDataSet();
            var table = result.Tables[sheetIndex];

            int successCount = 0;

            for (int row = 1; row < table.Rows.Count; row++) // 0 — заголовки
            {
                try
                {
                    var obj = new T();
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        string? header = table.Rows[0][col].ToString()?.Trim();
                        string propName = columnMapping != null && columnMapping.ContainsKey(header)
                            ? columnMapping[header]
                            : header ?? "";

                        PropertyInfo? prop = typeof(T).GetProperty(propName);
                        if (prop != null && prop.CanWrite)
                        {
                            var value = table.Rows[row][col];
                            if (value != DBNull.Value)
                            {
                                object converted = Convert.ChangeType(value, prop.PropertyType);
                                prop.SetValue(obj, converted);
                            }
                        }
                    }

                    context.Add(obj);
                    successCount++;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Ошибка в строке {row + 1}: {ex.Message}");
                }
            }

            Console.WriteLine($"✅ Импорт завершён. Импортировано {successCount} объектов типа {typeof(T).Name}");
            return successCount;
        }
    }
}
