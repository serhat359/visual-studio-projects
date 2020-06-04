using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CasualConsole
{
    public class ExcelHelper
    {
        string sheetName;
        IEnumerable<object> data;

        public ExcelHelper(string sheetName)
        {
            this.sheetName = sheetName;
        }

        public static byte[] WriteToExcel(string sheetName, IEnumerable<object> data)
        {
            var arr = new ExcelHelper(sheetName)
                .SetData(data)
                .GetAsByteArray();

            return arr;
        }

        public static void WriteToExcelStream(string sheetName, IEnumerable<object> data, Stream stream)
        {
            new ExcelHelper(sheetName)
                .SetData(data)
                .WriteToStream(stream);
        }

        public ExcelHelper SetData(IEnumerable<object> data)
        {
            this.data = data;

            return this;
        }

        public byte[] GetAsByteArray()
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                WriteDataToPackage(excelPackage);

                return excelPackage.GetAsByteArray();
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                WriteDataToPackage(excelPackage);

                excelPackage.SaveAs(stream);
            }
        }

        private void WriteDataToPackage(ExcelPackage excelPackage)
        {
            var sheet = excelPackage.Workbook.Worksheets.Add(this.sheetName);
            var cells = sheet.Cells;

            var properties = GetProperties();

            #region Header
            var row = 1;
            for (int i = 0; i < properties.Length; i++)
            {
                cells[row, i + 1].Value = properties[i].Name;
            }
            #endregion

            #region Data
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
        }

        private PropertyInfo[] GetProperties()
        {
            var type = data.GetType();

            var genericType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

            var properties = genericType.GetProperties();

            properties = properties.Where(x => x.GetCustomAttribute<ExcelIgnoreAttribute>() == null).ToArray();

            return properties;
        }
    }

}