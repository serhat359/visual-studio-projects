using System;
using System.Collections.Generic;
using System.Reflection;

namespace OfficeOpenXml
{
    public static class ExcelExtensions
    {
        public static ExcelPackage AddValues<T>(this ExcelPackage excelPackage, string sheetName, IEnumerable<T> data)
        {
            var sheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            var cells = sheet.Cells;

            var row = 1;
            foreach (var item in data)
            {
                cells[row, 1].Value = item;
                row++;
            }

            return excelPackage;
        }

        public static ExcelPackage AddSheet<T>(this ExcelPackage excelPackage, string sheetName, IEnumerable<T> data) where T : class
        {
            var sheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            var cells = sheet.Cells;

            var properties = GetProperties(typeof(T));

            #region Write Header
            var row = 1;
            for (int i = 0; i < properties.Length; i++)
            {
                cells[row, i + 1].Value = properties[i].Name;
            }
            #endregion

            #region Write Data
            row = 2;
            foreach (var item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    cells[row, col + 1].Value = properties[col].GetValue(item);
                }

                row++;
            }
            #endregion

            return excelPackage;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            var properties = type.GetProperties();

            return properties;
        }
    }
}
