using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace NinjaTurtlesMutation
{
    public static class TestUtils
    {
        public static class Attributes
        {
            public static List<string> Tests = new List<string> {"TestAttribute", "TestCaseAttribute"};
        }

        public static class Filtering
        {
            public static bool IsTestMethod(MethodDefinition method)
            {
                return MethodHasAttributes(method, Attributes.Tests);
            }
        }

        public static class NUnit
        {
            public static IDictionary<string, string> GetTestsNameDictionnaryTranslation(AssemblyDefinition assembly)
            {
                var methodsWithAttributes = new Dictionary<string, string>();
                foreach (var type in assembly.MainModule.Types)
                    GetMethodsNameWithAttributesFromType(type, Attributes.Tests, methodsWithAttributes);
                return methodsWithAttributes;
            }

            private static void GetMethodsNameWithAttributesFromType(TypeDefinition type, IList<string> searchedAttributes, IDictionary<string, string> matchingMethods)
            {
                foreach (var method in type.Methods)
                {
                    if (!MethodHasAttributes(method, searchedAttributes))
                        continue;
                    var methodName = method.Name;
                    var methodNunitName = String.Format("{0}.{1}", type.FullName.Replace("/", "+"), methodName);
                    if (matchingMethods.ContainsKey(methodName))
                        continue;
                    matchingMethods.Add(methodName, methodNunitName);
                }
                if (type.NestedTypes == null)
                    return;
                foreach (var nestedType in type.NestedTypes)
                    GetMethodsNameWithAttributesFromType(nestedType, searchedAttributes, matchingMethods);
            }
        }

        private static bool MethodHasAttributes(MethodDefinition method, IEnumerable<string> searchedAttributes)
        {
            var attributesTypes = method.CustomAttributes.Select(a => a.AttributeType.Name).ToList();
            return attributesTypes.Intersect(searchedAttributes).Any();
        }
    }
}
