using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace EnvironmentVariablesEditor
{
    [XmlInclude(typeof(PathEntry))]
    [XmlInclude(typeof(EnvironmentVariable))]
    public class ExportableObject
    {

    }

}
