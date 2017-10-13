using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ResxFinder.Model
{
    public class StringResource
    {
        public string Name { get; set; }

        public string Text { get; private set; }

        private System.Drawing.Point m_Location = System.Drawing.Point.Empty;

        public System.Drawing.Point Location { get { return (m_Location); } private set { m_Location = value; } }

        public FileParser Parent {get; private set;}

        public StringResource()
        {
        }

        public StringResource(FileParser parent,
            string name,
            string text,
            System.Drawing.Point location)
        {
            Parent = parent;
            Name = name;
            Text = text;
            Location = location;
        }


        public void Offset(int lineOffset,
                           int columnOffset)
        {
            if ((lineOffset == 0) && (columnOffset == 0))
                return;

            m_Location.Offset(lineOffset, columnOffset);
        }

        public override string ToString()
        {
            return $"{Name ?? "Unknown name"}, {Text ?? "Unknown text"}, location: {Location}.";
        }
    }
}
