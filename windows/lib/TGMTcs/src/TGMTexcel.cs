//To install EPPlus to handle excel file, go to menu in Visual Studio: Tools -> NuGet Package Manager -> Package Manager Console
//Type following in console:
//PM> Install-Package EPPlus


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using OfficeOpenXml.Style;

namespace TGMTcs
{
    public class TGMTexcel
    {
        ExcelPackage m_excel;
        ExcelWorkbook m_workbook;

        public TGMTexcel(string fileName)
        {
            m_excel = new ExcelPackage(new FileInfo(fileName));
            m_workbook = m_excel.Workbook;            
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Save()
        {
            m_excel.Save();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SaveAs(string fileName)
        {
            m_excel.SaveAs(new FileInfo(fileName));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetCellValue(int worksheetID, int row, int col,  string value)
        {
            m_workbook.Worksheets[worksheetID].Cells[row, col].Value = value;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetCellBGColor(int worksheetID, int row, int col, Color color)
        {
            m_workbook.Worksheets[worksheetID].Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            m_workbook.Worksheets[worksheetID].Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SetCellTextColor(int worksheetID, int row, int col, Color color)
        {
            m_workbook.Worksheets[worksheetID].Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            m_workbook.Worksheets[worksheetID].Cells[row, col].Style.Font.Color.SetColor(color);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddRow(int worksheetID, int row, string[] values)
        {
            for(int i=0; i<values.Length;i++)
            {
                m_workbook.Worksheets[worksheetID].Cells[row, i+ 1].Value = values[i];
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddSheet(string sheetName)
        {
            if(m_workbook.Worksheets[sheetName] == null)
                m_workbook.Worksheets.Add(sheetName);
        }
    }


}
