using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontParser
{
    class ColBlock
    {
        public static bool[] OffCol;
        public static bool[] OnCol;
        private SortedDictionary<int, CharBitmap> _CharsInBlock =
            new SortedDictionary<int, CharBitmap>();
        private int _RightMostPosition = 0;
        private int _LeftMostPosition = 0;

        public int Size { get { return _RightMostPosition - _LeftMostPosition; } }
        static ColBlock()
        {
            OffCol = new bool[6];
            OnCol = new bool[6];
            for (int i=0; i<6; i++)
            {
                OffCol[i] = false;
                OnCol[i] = true;
            }
        }

        public void AddCharBitmap(CharBitmap charBitmap, bool rtl)
        {
            // rtl only for now
            if (Size > 0)
            {
                _LeftMostPosition--;
            }
            _LeftMostPosition -= charBitmap.Cols;
            _CharsInBlock[_LeftMostPosition] = charBitmap;
        }

        public void AddWhiteSpace(int size, bool rtl)
        {
            // rtl only for now
            _LeftMostPosition -= size;
        }

        public void getCols(Func<bool[],int> callback)
        {
            int curCol = _LeftMostPosition;
            foreach (var kv in _CharsInBlock)
            {
                int startCol = kv.Key;
                while (curCol < startCol)
                {
                    callback(OffCol);
                    curCol++;
                }
                CharBitmap bitmap = kv.Value;
                int cols = bitmap.Cols;
                curCol += cols;
                for (int i=0; i<cols; i++)
                {
                    callback(bitmap.GetCol(i));
                }
                //if (kv.Key + cols < _RightMostPosition)
                //{
                //    callback(OffCol);
                //}
            }
        }
    }
}
