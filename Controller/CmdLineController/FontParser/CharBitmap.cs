using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace FontParser
{

    public class CharBitmap
    {
        const int height = 6;
        int cols = 0;
        //const int maxWidth = 10;
        private Dictionary<int, bool[]> _Bitmap;
        private bool[] MakeCol(int col)
        {
            if (col >= cols)
            {
                Debug.Assert(col == cols);
                bool[] newCol = new bool[height];
                for (int i= 0; i < height; i++)
                {
                    newCol[i] = false;
                }
                _Bitmap.Add(col, newCol);
                cols++;
            }
            return _Bitmap[col];
        }

        public bool[] GetCol(int colnum)
        {
            return _Bitmap[colnum];
        }

        public int Cols { get { return cols; } }
        public CharBitmap(XmlNode charNode)
        {
            _Bitmap = new Dictionary<int, bool[]>();
            // bool widthSet = false;
            XmlNodeList rowNodes = charNode.SelectNodes("row");
            int row = 0;
            foreach (XmlNode rowNode in rowNodes)
            {
                Trace.Assert(row < height, "Too many rows");
                string pixelString = rowNode.Attributes["pixels"].Value;
                int col = 0;
                foreach (char c in pixelString)
                {
                    if (char.IsWhiteSpace(c)) continue;
                    bool[] colPixels = MakeCol(col);
                    switch (c)
                    {
                        case '.':
                            colPixels[row] = false;
                            break;

                        case '*':
                            colPixels[row] = true;
                            break;

                        default:
                            // ERROR
                            Trace.Assert(false, "Not a pixel representation");
                            break;
                    }
                    col++;
                }
                row++;
            }

        }
    }
}
