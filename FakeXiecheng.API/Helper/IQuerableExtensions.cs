using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using FakeXiecheng.API.Services;

namespace FakeXiecheng.API.Helper
{
    public static class IQuerableExtensions
    {
        public static IQueryable<T> ApplySort<T>( 
            this IQueryable<T> source,
            string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary
            )
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                return source;
            }

            var orderByString = string.Empty;

            var orderByAfterSplit = orderBy.Split(',');

            foreach(var order in orderByAfterSplit)
            {
                var trimmedOrder = order.Trim();

                var orderDescending = trimmedOrder.EndsWith(" desc");

                var indexOfFirstSpace = trimmedOrder.IndexOf(" ");

                var propertyName = indexOfFirstSpace == -1
                    ? trimmedOrder
                    : trimmedOrder.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mappint for {propertyName} is missing");
                }

                var propertyMappingValue = mappingDictionary[propertyName];
                if(propertyMappingValue == null)
                {
                    throw new ArgumentNullException("PropertyMappingValue");
                }

                foreach(var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    orderByString = orderByString +
                        (string.IsNullOrEmpty(orderByString) ? string.Empty : ", ")
                        + destinationProperty
                        + (orderDescending ? " descending" : " ascending");
                }

            }

            return source.OrderBy(orderByString);

        }
    }
}
