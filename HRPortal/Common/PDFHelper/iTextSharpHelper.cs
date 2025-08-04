using HRPortal.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace HRPortal.Common.PDFHelper
{
    public class iTextSharpHelper
    {
        public static byte[] ExportPDF(string txt, bool horizontal = false)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(txt);
                using (MemoryStream msInput = new MemoryStream(data))
                {
                    using (Document doc = new Document())
                    {
                        if (horizontal)
                            doc.SetPageSize(PageSize.A4.Rotate());

                        PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);
                        PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);
                        doc.Open();
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new UnicodeFontFactory());
                        PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, writer);
                        writer.SetOpenAction(action);
                    }
                }
                return outputStream.ToArray();
            }
        }


        public static byte[] MergePdfFiles(byte[] pdf1, byte[] pdf2)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, outputStream);
                document.Open();

                PdfReader reader1 = new PdfReader(pdf1);
                CopyPdfPages(reader1, document, writer);

                PdfReader reader2 = new PdfReader(pdf2);
                CopyPdfPages(reader2, document, writer);

                document.Close();

                return outputStream.ToArray();
            }
        }


        private static void CopyPdfPages(PdfReader reader, Document document, PdfWriter writer)
        {
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                document.NewPage();
                PdfImportedPage page = writer.GetImportedPage(reader, i);
                writer.DirectContent.AddTemplate(page, 0, 0);
            }
        }

        public static byte[] ReadPdf(string path)
        {
            // 檢查路徑是否為空
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("路徑不能為空", path);
            }

            // 獲取文件副檔名
            string extension = Path.GetExtension(path).ToLowerInvariant();
            string fileName = Path.GetFileName(path);

            // 檢查副檔名是否為 .pdf
            if (extension.ToLower() != ".pdf")
            {
                throw new InvalidOperationException("檔案不是 PDF 格式");
            }

            // 檢查文件是否存在
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("找不到文件:" + fileName, path);
            }

            // 讀取並返回文件的字節數組
            return File.ReadAllBytes(path);
        }


    }
}