using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

public interface ISlideVisitor
{
    void VisitTextSlide(TextSlide textSlide);
    void VisitUniformSlide(UniformSlide uniformSlide);
}

public interface ISlide
{
    void Accept(ISlideVisitor visitor);
}

public class TextSlide : ISlide
{
    public enum Alignment { Right, Left, Center };
    private string _text;
    private Alignment _alignment;
    public string Text {  get { return _text; } }
    public Alignment TextAlignment { get { return _alignment; } }
    public TextSlide(string text, Alignment alignment) { _text = text; _alignment = alignment; }
    public void Accept(ISlideVisitor visitor) { visitor.VisitTextSlide(this); }
}

public class UniformSlide : ISlide
{
    private bool _on;
    public bool On { get { return _on; } }
    public void Accept (ISlideVisitor visitor) { visitor.VisitUniformSlide(this); }
    public UniformSlide(bool on) { _on = on; }
}

namespace FontParser
{
    class MatrixShowParser
    {
        private List<ISlide> _slideList = new List<ISlide>();
        public List<ISlide> Slides { get { return _slideList; } }
        public MatrixShowParser(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/slides");
            foreach (XmlNode mainNode in nodes)
            {
                foreach (XmlNode slideNode in mainNode.ChildNodes)
                {
                    if (slideNode.Name == "text")
                    { 
                        TextSlide.Alignment alignment = TextSlide.Alignment.Center;
                        XmlAttribute attribute = slideNode.Attributes["align"];
                        if (attribute != null) {
                            if (attribute.Value.ToLower() == "right")
                            {
                                alignment = TextSlide.Alignment.Right;
                            } else if (attribute.Value.ToLower() == "left")
                            {
                                alignment = TextSlide.Alignment.Left;
                            } else if (attribute.Value.ToLower() == "center")
                            {
                                alignment = TextSlide.Alignment.Center;
                            } else
                            {
                                Trace.Fail(string.Format("Unrecognized align value: {0}", attribute.Value));
                            }
                        }
                        _slideList.Add(new TextSlide(slideNode.InnerText, alignment));
                    }
                    else if (slideNode.Name.ToLower() == "allon")
                    {
                        _slideList.Add(new UniformSlide(true));
                    }
                    else if (slideNode.Name.ToLower() == "alloff")
                    {
                        _slideList.Add(new UniformSlide(false));
                    }
                    else
                    {
                        Trace.Fail(string.Format("Unrecognized slide type: {0}", slideNode.Name));
                    }

                }

            }

        }
    }
}
