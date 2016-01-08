using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace EnvironmentVariablesEditor {

    public class EnvironmentVariable : ExportableObject, IEditableObject {

        private EnvironmentVariable copy;

        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public EnvironmentVariableTarget Target { get; set; }

        public EnvironmentVariable() {

        }

        public EnvironmentVariable(string key, string value, EnvironmentVariableTarget target) {
            Key = key;
            Value = value;
            Target = target;
        }

        #region IEditableObject Members

        public void BeginEdit() {
            if (copy == null) {
                copy = new EnvironmentVariable();
            }

            copy.Key = Key;
            copy.Value = Value;
            copy.Target = Target;
        }

        public void CancelEdit() {
            Value = copy.Value;
            Key = copy.Key;
            Target = copy.Target;
        }

        public void EndEdit() {
            copy.Key = null;
            copy.Value = null;
        }

        #endregion IEditableObject Members

    }

}
