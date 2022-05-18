using ManageDisco.Model;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco.Helper
{
    public class FileHelper
    {
        PdfModel header;
        List<PdfModel> rows;
        int startY = 70;
        int startX = 70;

        int headerFromRowsInterline = 50;
        int rowsInterline = 30;

        public MemoryStream GeneratePdf(string fileName)
        {
            if (header == null)
                throw new NullReferenceException("Header is not initialized.");
            if (rows == null)
                throw new NullReferenceException("Rows are not initialized.");

            MemoryStream memoryStream = new MemoryStream();

            PdfDocument pdfDocument = new PdfDocument(memoryStream, false);
            PdfPage page = pdfDocument.AddPage();
            XGraphics writer = XGraphics.FromPdfPage(page);
            WriteHeaderAndRows(ref writer);

            pdfDocument.Save(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        private void WriteHeaderAndRows(ref XGraphics writer)
        {
            writer.DrawString(header.Value, new XFont(header.FontFamily, header.FontSize), header.Color, new XRect(0, startY, writer.PageSize.Width, writer.PageSize.Height), XStringFormats.TopCenter);
            startY += headerFromRowsInterline;

            WriteRows(ref writer);
        }

        private void WriteRows(ref XGraphics writer)
        {            
            foreach(PdfModel row in rows)
            {
                writer.DrawString(row.Value, new XFont(row.FontFamily, row.FontSize), row.Color, new XRect(startX, startY, writer.PageSize.Width, writer.PageSize.Height), XStringFormats.TopLeft);
                startY += rowsInterline;
            }
        }

        public void SetPdfHeader(string value, XBrush color, int fontSize, string fontFamily, bool isBold)
        {
            header = new PdfModel(value, color, fontSize, fontFamily, isBold);
        }

        public void SetPdfRows(string[] value, XBrush color, int fontSize, string fontFamily, bool isBold)
        {
            rows = new List<PdfModel>();
            foreach(string row in value)
            {
                rows.Add(new PdfModel(row, color, fontSize, fontFamily, isBold));
            }
        }
    }
}
