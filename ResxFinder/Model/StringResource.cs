using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ResxFinder.Model
{
  public class StringResource
  {

    public StringResource()
    {
    }

    public StringResource(string name,
                          string text,
                          System.Drawing.Point location)
    {
      this.Name     = name;
      this.Text     = text;
      this.Location = location;
    }

    public string Name { get; set; }

    public string Text { get; private set; }

    private System.Drawing.Point m_Location = System.Drawing.Point.Empty;

    public System.Drawing.Point Location { get { return (m_Location); } private set { m_Location = value; } }

    public void Offset(int lineOffset,
                       int columnOffset)
    {
      if ((lineOffset == 0) && (columnOffset == 0))
        return;

            m_Location.Offset(lineOffset, columnOffset);
    }

        public override string ToString()
        {
            return $"{Name ?? "Unknown name"}, {Text ?? "Unknown text"}.";
        }
    }
}
