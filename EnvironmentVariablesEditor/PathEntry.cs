using System.ComponentModel;
using System.Xml.Serialization;

namespace EnvironmentVariablesEditor
{

    public class PathEntry : ExportableObject, IEditableObject
    {

        private PathEntry _copy;

        [XmlAttribute]
        public string Value { get; set; }

        public PathEntry()
        {

        }

        public PathEntry(string value)
        {
            Value = value;
        }

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (_copy == null)
                _copy = new PathEntry();

            _copy.Value = Value;
        }

        public void CancelEdit()
        {
            Value = _copy.Value;
        }

        public void EndEdit()
        {
            _copy.Value = null;
        }

        #endregion IEditableObject Members

    }

}
