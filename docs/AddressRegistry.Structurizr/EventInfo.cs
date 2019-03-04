namespace AddressRegistry.Structurizr
{
    using System;
    using System.Collections.Generic;

    public class EventInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }

        public List<string> Properties { get; set; }
    }
}
