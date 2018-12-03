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
    class Font
    {
        private Dictionary<char, CharBitmap> _CharToBitmap = new Dictionary<char, CharBitmap>();
        public Font(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/font/character");
            foreach (XmlNode node in nodes)
            {
                CharBitmap charBitmap = new CharBitmap(node);
                string chars = node.Attributes["char"].Value;
                foreach (char c in chars)
                {
                    Trace.Assert(!_CharToBitmap.ContainsKey(c), "Character already defines");
                    _CharToBitmap.Add(c, charBitmap);
                }

            }

        }

        public CharBitmap getCharBitmap(char c)
        {
            return _CharToBitmap[c];
        }
    }
}
