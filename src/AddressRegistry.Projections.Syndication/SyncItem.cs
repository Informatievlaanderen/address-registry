namespace AddressRegistry.Projections.Syndication
{
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name = "Content", Namespace = "")]
    public class SyncItem<TObject>
    {
        [DataMember]
        public XmlElement Event { get; set; }
        [DataMember]
        public TObject Object { get; set; }
    }
}
