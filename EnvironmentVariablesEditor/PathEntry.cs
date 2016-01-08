using System.ComponentModel;
using System.Xml.Serialization;

namespace EnvironmentVariablesEditor {

    public class PathEntry : ExportableObject, IEditableObject {

        private PathEntry copy;

        [XmlAttribute]
        public string Value { get; set; }

        public PathEntry() {

        }

        public PathEntry(string value) {
            Value = value;
        }

        #region IEditableObject Members

        public void BeginEdit() {
            if (copy == null) {
                copy = new PathEntry();
            }

            copy.Value = Value;
        }

        public void CancelEdit() {
            Value = copy.Value;
        }

        public void EndEdit() {
            copy.Value = null;
        }

        #endregion IEditableObject Members

    }

}
