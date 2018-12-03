using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Ports;



namespace FontParser
{
    public class PhraseSlideVisitor : ISlideVisitor
    {
        private string[] _names;
        private int _stringIdx = 1;

        public PhraseSlideVisitor(string[] names) { _names = names; }
        public void VisitTextSlide(TextSlide textSlide)
        {
            _names[_stringIdx++] = new string(textSlide.Text.Reverse().ToArray());
        }

        public void VisitUniformSlide(UniformSlide uniformSlide)
        {
            _names[_stringIdx++] = string.Format("<All {0}>", uniformSlide.On ? "On" : "Off");
        }
    }

    class BitmapSlideVisitor : ISlideVisitor
    {
        private string[] _stringizedBitmaps;
        private Font _font;
        private int _width;
        private int _curStringIdx = 0;
        private int _curCharIdx = 0;
        private char[] _stringizedColumns;

        public BitmapSlideVisitor(string[] stringizedBitmaps, Font font, int columns)
        {
            _stringizedBitmaps = stringizedBitmaps;
            _font = font;
            _width = columns;
            _stringizedColumns = new char[columns];
        }

        private int StringizedColumn(bool[] val)
        {
            int bits = 0;
            for (int i = 0; i < 6; i++)
            {
                bits <<= 1;
                if (val[i]) bits |= 1;
            }
            _stringizedColumns[_curCharIdx++] = (char)('1' + bits);
            return 0;
        }

        private void RecordStringizedBitmap()
        {
            _stringizedBitmaps[_curStringIdx++] = new string(_stringizedColumns);
            _curCharIdx = 0;
        }

        public void VisitUniformSlide(UniformSlide uniformSlide)
        {
            bool[] colPixels = uniformSlide.On ? ColBlock.OnCol : ColBlock.OffCol;
            for (int i=0; i<_width; i++)
            {
                StringizedColumn(colPixels);
            }
            RecordStringizedBitmap();
        }

        public void VisitTextSlide(TextSlide textSlide)
        {
            string text = textSlide.Text;
            Console.WriteLine("Processing text: {0}", text);
            ColBlock block = new ColBlock();
            for (int j = 0; j < text.Length; j++)
            {
                char c = text[j];
                if (c == ' ')
                {
                    block.AddWhiteSpace(4, true);
                }
                else
                {
                    block.AddCharBitmap(_font.getCharBitmap(c), true);
                }
            }

            int colLeft = _width - block.Size;
            Trace.Assert(colLeft >= 0, string.Format("Text does not fit in {0} columns: {1}", _width, text));
            int before = 0, after = 0;

            switch (textSlide.TextAlignment)
            {
                case TextSlide.Alignment.Center:
                    before = colLeft / 2;
                    after = colLeft - (colLeft / 2);
                    break;

                case TextSlide.Alignment.Left:
                    before = 0;
                    after = colLeft;
                    break;

                case TextSlide.Alignment.Right:
                    before = colLeft;
                    after = 0;
                    break;

            }

            for (int j = 0; j < before; j++)
            {
                StringizedColumn(ColBlock.OffCol);
            }

            block.getCols(StringizedColumn);

            for (int j = 0; j < after; j++)
            {
                StringizedColumn(ColBlock.OffCol);
            }

            RecordStringizedBitmap();

        }
    }

    class Program
    {
        private static SerialPort _serialPort;
        private const int width = 40;
        private static string[] _slideNames;
        private static string[] _stingizedBitmaps;

        private static void Show(int slideNum)
        {
            _serialPort.WriteLine("");
            _serialPort.WriteLine(_stingizedBitmaps[slideNum]);
        }

        private static void Commander()
        {
            int slideCount = _slideNames.Length;
            int curSlide = 0;

            bool exit = false;

            while (!exit)
            {
                Console.OutputEncoding = new UTF8Encoding();
                Show(curSlide);
                Console.WriteLine("{0}: {1}", curSlide, _slideNames[curSlide]);
                Console.Write(">");
                string commandLine = Console.ReadLine();
                int commandCharIdx = 0;

                while (commandCharIdx < commandLine.Length && Char.IsWhiteSpace(commandLine[commandCharIdx])) {
                    commandCharIdx++;
                }
                if (commandCharIdx == commandLine.Length) continue; // no command

                char commandChar = Char.ToLower(commandLine[commandCharIdx]);
                switch (commandChar)
                {
                    case '?':
                        Console.WriteLine("n: next slide");
                        Console.WriteLine("p: previous slide");
                        Console.WriteLine("l: list slides");
                        Console.WriteLine("g <num>: goto slide <num>");
                        Console.WriteLine("q: quit");
                        break;

                    case 'n':
                        curSlide++;
                        if (curSlide >= slideCount)
                        {
                            Console.WriteLine("No more slides.");
                            curSlide = slideCount - 1;
                        }
                        break;
                    case 'p':
                        curSlide--;
                        if (curSlide < 0)
                        {
                            Console.WriteLine("No more slides.");
                            curSlide = 0;
                        }
                        break;
                    case 'l':
                        for (int i = 0; i < slideCount; i++)
                        {
                            Console.WriteLine("{0}: {1}", i, _slideNames[i]);
                        }
                        Console.WriteLine("");
                        break;
                    case 'g':

                        string numberString = "";
                        if (commandCharIdx + 1 < commandLine.Length)
                        {
                            numberString = commandLine.Substring(commandCharIdx + 1);
                        }
                        int newSlide;
                        bool hasNum = int.TryParse(numberString, out newSlide);
                        if (!hasNum)
                        {
                            Console.WriteLine("slide number must be specified.");
                        }
                        if (newSlide >= 0 && newSlide < slideCount)
                        {
                            curSlide = newSlide;
                        }
                        else
                        {
                            Console.WriteLine("no such slide.");
                        }
                        break;
                    case 'q':
                        exit = true;
                        break;
                }
            }


        }

        //static private Dictionary<char, CharBitmap> _CharToBitmap = new Dictionary<char, CharBitmap>();
        static void Main(string[] args)
        {

            _serialPort = new SerialPort
            {
                PortName = args[1], //Set your board COM
                BaudRate = 19200
            };
            _serialPort.Open();
            

            MatrixShowParser showParser = new MatrixShowParser(args[0]);
            List<ISlide> slides = showParser.Slides;

            _slideNames = new string[slides.Count + 2];

            _slideNames[0] = "<beginning>";
            _slideNames[slides.Count+1] = "<end>";

            PhraseSlideVisitor phraseVisitor = new PhraseSlideVisitor(_slideNames);
            foreach (ISlide slide in slides)
            {
                slide.Accept(phraseVisitor);
            }

            _stingizedBitmaps = new string[slides.Count + 2];
            UniformSlide emptySlide = new UniformSlide(false);
            string fontFile = AppDomain.CurrentDomain.BaseDirectory + @"\font.xml";
            Font font = new Font(fontFile);
            BitmapSlideVisitor bitmapVisitor = new BitmapSlideVisitor(_stingizedBitmaps, font, width);

            emptySlide.Accept(bitmapVisitor); // beginning

            foreach(ISlide slide in slides)
            {
                slide.Accept(bitmapVisitor);
            }

            emptySlide.Accept(bitmapVisitor); // end

            Commander();

            _serialPort.Close();
        }

        
    }
}
