using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Helper
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields
            )
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var expandoObjectList = new List<ExpandoObject>();

            //To avoid iteaor through list , create property list
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrEmpty(fields))
            {
                // Get all object ExpandoObject properties
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase
                    | BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',');

                foreach(var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();

                    var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase
                    | BindingFlags.Public | BindingFlags.Instance);

                    if(propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} doesnot exist for {typeof(TSource)}");
                    }

                    propertyInfoList.Add(propertyInfo);
                }
            }

            foreach (TSource sourceObject in source)
            {
                var dataShapedObject = new ExpandoObject();

                foreach(var propertyInfo in propertyInfoList)
                {
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                expandoObjectList.Add(dataShapedObject);
            }

            return expandoObjectList;
        }
    }
}
