// There are no consumers of this class! See comment in HelperTest.cs

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;

//namespace SFA.DAS.Commitments.Api.Client
//{
//    public class QueryStringHelper
//    {
//        public string GetQueryString(object obj)
//        {
//            if (obj == null)
//                return string.Empty;

//            var result = new List<string>();
//            var props = obj.GetType().GetProperties().Where(p => p.GetValue(obj, null) != null);
//            foreach (var p in props)
//            {
//                var value = p.GetValue(obj, null);
//                if (value is ICollection enumerable)
//                {
//                    result.AddRange(from object v in enumerable select $"{p.Name}={v}");
//                }
//                else
//                {
//                    // doesn't currently handle all scenarios (only the ones we currently need)
//                    // if it needs to be extended, see..
//                    // https://stackoverflow.com/questions/6553183/check-to-see-if-a-given-object-reference-or-value-type-is-equal-to-its-default/6553303#6553303
//                    // https://stackoverflow.com/questions/325426/programmatic-equivalent-of-defaulttype

//                    var valueType = value.GetType();

//                    if (!value.Equals(valueType.IsValueType ? Activator.CreateInstance(valueType) : null))
//                        result.Add($"{p.Name}={value}");
//                }
//            }

//            return result.Any() 
//                ? "?" + string.Join("&", result.ToArray())
//                : string.Empty;
//        }
//    }
//}
