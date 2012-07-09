using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace EnvironmentVariablesEditor
{

    public class EnvironmentVariable : ExportableObject, IEditableObject
    {

        private EnvironmentVariable _copy;

        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public EnvironmentVariableTarget Target { get; set; }

        public EnvironmentVariable()
        {

        }

        public EnvironmentVariable(string key, string value, EnvironmentVariableTarget target)
        {
            Key = key;
            Value = value;
            Target = target;
        }

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (_copy == null)
                _copy = new EnvironmentVariable();

            _copy.Key = Key;
            _copy.Value = Value;
            _copy.Target = Target;
        }

        public void CancelEdit()
        {
            Value = _copy.Value;
            Key = _copy.Key;
            Target = _copy.Target;
        }

        public void EndEdit()
        {
            _copy.Key = null;
            _copy.Value = null;
        }

        #endregion IEditableObject Members

    }

}
