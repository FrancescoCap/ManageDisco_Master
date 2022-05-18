using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Model
{   
    public class PdfModel
    {
        public string Value { get; private set; }
        public XBrush Color { get; private set; }
        public int FontSize { get; private set; }
        public string FontFamily { get; private set; }
        public bool IsBold { get; private set; }

        public PdfModel(string value, XBrush color, int fontSize, string fontFamily, bool isBold)
        {
            Value = value;
            Color = color;
            FontSize = fontSize;
            FontFamily = fontFamily;
            IsBold = isBold;
        }
               
    }

}
