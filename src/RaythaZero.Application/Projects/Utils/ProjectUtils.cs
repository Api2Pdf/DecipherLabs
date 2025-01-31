using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using RaythaZero.Application.Common.Utils;
using UglyToad.PdfPig;
using Topic = RaythaZero.Domain.Entities.Topic;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Utils;

public static class ProjectUtils
{
    private static List<Topic> _topics;
    private static Dictionary<string, BlsOccupationData> _blsData;

    public static Dictionary<string, BlsOccupationData> GetBlsData(string companyState)
    {
        if (_blsData == null)
        {
            var blsDataAsJson = StringExtensions.ReadJsonFile("RaythaZero.Application.Projects.Utils.bls.json");
            var blsArray = JsonSerializer.Deserialize<List<BlsOccupationData>>(blsDataAsJson);
            _blsData = blsArray.Where(x => x.state_name.Equals(companyState, StringComparison.OrdinalIgnoreCase))
                              .ToDictionary(x => x.occupation_code, x => x);
        }
        return _blsData ?? new Dictionary<string, BlsOccupationData>();
    }

    public static IEnumerable<Topic> GetTopics()
    {
        var topicsAsJson = StringExtensions.ReadJsonFile("RaythaZero.Application.Projects.Utils.topics.json");
        var topics = JsonSerializer.Deserialize<IEnumerable<Topic>>(topicsAsJson);
        
        return topics ?? Enumerable.Empty<Topic>();
    }
    
    public static string GetTextFromDocument(byte[] bytes, string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();

        return extension switch
        {
            ".pdf" => ReadPdfText(bytes),
            ".docx" => ReadWordText(bytes),
            ".xlsx" => ReadExcelData(bytes),
            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        }; 
    }
    
    static string ReadPdfText(byte[] bytes)
    {
        using (var pdfDocument = PdfDocument.Open(bytes))
        {
            string text = "";

            foreach (var page in pdfDocument.GetPages())
            {
                text += page.Text + Environment.NewLine;
            }
            return text;
        }
    }
    
    static string ReadWordText(byte[] bytes)
    {
        using (var memoryStream = new MemoryStream(bytes))
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memoryStream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                return body.InnerText; // Extracts all text as a single string
            }
        }
    }
    
    static string ReadExcelData(byte[] bytes)
    {
        var rowsData = new List<string[]>(); // List to hold all rows as arrays of strings

        using (var memoryStream = new MemoryStream(bytes))
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, false))
            {
                var workbookPart = spreadsheetDocument.WorkbookPart;
                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

                // Get the first sheet
                var sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                foreach (var row in sheetData.Elements<Row>())
                {
                    var rowData = new List<string>();

                    foreach (var cell in row.Elements<Cell>())
                    {
                        string cellValue = GetCellValue(cell, sharedStringTable);
                        rowData.Add(cellValue);
                    }

                    rowsData.Add(rowData.ToArray());
                }
            }
        }

        return string.Join("\n", rowsData);
    }

    // Helper method to get the cell value
    static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell == null || cell.CellValue == null) return string.Empty;

        string value = cell.CellValue.Text;

        // If the cell data type is SharedString, get the value from the shared string table
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            if (int.TryParse(value, out int sharedStringIndex))
            {
                return sharedStringTable?.ElementAt(sharedStringIndex)?.InnerText ?? string.Empty;
            }
        }

        return value;
    }
}