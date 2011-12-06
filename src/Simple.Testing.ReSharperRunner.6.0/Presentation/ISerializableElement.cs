using System.Xml;

namespace Simple.Testing.ReSharperRunner.Presentation
{
  public interface ISerializableElement
  {
    void WriteToXml(XmlElement parent);
  }
}