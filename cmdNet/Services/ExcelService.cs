
using cmdNet.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdNet.Services;

public class ExcelService
{
    public List<UserExcelModel> Read(
        string filePath)
    {
       

        var result =
            new List<UserExcelModel>();

        using var package =
            new ExcelPackage(
                new FileInfo(filePath));

        var sheet =
            package.Workbook.Worksheets[0];

        for (int row = 1;
             row <= sheet.Dimension.End.Row;
             row++)
        {
            result.Add(new UserExcelModel
            {
                FirstName =
                    sheet.Cells[row, 1].Text.Trim(),

                LastName =
                    sheet.Cells[row, 2].Text.Trim(),

                NationalCode =
                    sheet.Cells[row, 3].Text.Trim(),

                Mobile =
                    sheet.Cells[row, 4].Text.Trim()
            });
        }

        return result;
    }


    public void Export(
    string fileName,
    List<UserResultModel> data)
    {
        
        using var package =
            new ExcelPackage();

        var ws =
            package.Workbook.Worksheets.Add(
                "Report");

        ws.Cells[1, 1].Value = "FirstName";
        ws.Cells[1, 2].Value = "LastName";
        ws.Cells[1, 3].Value = "UserName";
        ws.Cells[1, 4].Value = "Password";
        ws.Cells[1, 5].Value = "Status";
        ws.Cells[1, 6].Value = "SimilarUserName";
        ws.Cells[1, 7].Value = "SimilarDisplayName";
        ws.Cells[1, 8].Value = "Action";

        int row = 2;

        foreach (var item in data)
        {
            ws.Cells[row, 1].Value =
                item.FirstName;

            ws.Cells[row, 2].Value =
                item.LastName;

            ws.Cells[row, 3].Value =
                item.UserName;

            ws.Cells[row, 4].Value =
                item.Password;

            ws.Cells[row, 5].Value =
                item.Status;
            ws.Cells[row, 6].Value = item.SimilarUserName;
            ws.Cells[row, 7].Value = item.SimilarDisplayName;
            ws.Cells[row, 8].Value = item.Action;

            row++;
        }

        File.WriteAllBytes(
            fileName,
            package.GetAsByteArray());
    }
}