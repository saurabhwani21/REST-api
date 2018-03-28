using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Services
{
    //This class will take care of mapping.
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {

        public Dictionary<string, PropertyMappingValue> _mappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _mappingDictionary = mappingDictionary;
        }
    }
}
