//download itextsharp and reference


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.parser;
using System.Drawing;

namespace TGMTcs
{
    public class TGMTpdf
    {        
        public static string ReadText(string filePath, int pageNumber = -1)
        {
            if (pageNumber == 0)
                throw new Exception("page must bigger than 0");

            PdfReader reader = new PdfReader(filePath);

            StringWriter output = new StringWriter();
            SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();


            if (pageNumber == -1)
                pageNumber = reader.NumberOfPages;
            for (int i = 1; i <= pageNumber; i++)
                output.WriteLine(PdfTextExtractor.GetTextFromPage(reader, i, strategy));

            return output.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<System.Drawing.Image> ExtractImages(string PDFSourcePath)
        {
            List<System.Drawing.Image> imgList = new List<System.Drawing.Image>();

            RandomAccessFileOrArray RAFObj = null;
            PdfReader PDFReaderObj = null;
            PdfObject PDFObj = null;
            PdfStream PDFStremObj = null;

            try
            {
                RAFObj = new RandomAccessFileOrArray(PDFSourcePath);
                PDFReaderObj = new PdfReader(RAFObj, null);
                for (int i = 0; i <= PDFReaderObj.XrefSize - 1; i++)
                {
                    PDFObj = PDFReaderObj.GetPdfObject(i);

                    if ((PDFObj != null) && PDFObj.IsStream())
                    {
                        PDFStremObj = (PdfStream)PDFObj;
                        iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(PdfName.SUBTYPE);

                        if ((subtype != null) && subtype.ToString() == PdfName.IMAGE.ToString())
                        {
                            try
                            {

                                PdfImageObject PdfImageObj =  new PdfImageObject((PRStream)PDFStremObj);

                                System.Drawing.Image ImgPDF = PdfImageObj.GetDrawingImage();


                                imgList.Add(ImgPDF);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
                PDFReaderObj.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return imgList;
        }
    }
}
