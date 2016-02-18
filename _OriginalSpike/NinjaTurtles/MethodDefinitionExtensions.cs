#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System.Linq;

using Mono.Cecil;

using NinjaTurtles.Attributes;

namespace NinjaTurtles
{
    internal static class MethodDefinitionExtensions
    {
        internal static bool HasTestedAttributeMatching(this MethodDefinition testMethod, MethodDefinition method)
        {
            var candidateAttributes = testMethod.CustomAttributes
                .Where(a => a.AttributeType.Name == typeof(MethodTestedAttribute).Name
                            && (string)a.ConstructorArguments[1].Value == method.Name);
            if (!candidateAttributes.Any()) return false;
            foreach (var attribute in candidateAttributes)
            {
                var typeReference = attribute.ConstructorArguments[0].Value as TypeReference;
                var typeName = attribute.ConstructorArguments[0].Value as string;
                if (typeReference != null)
                {
                    if (typeReference.Resolve().FullName != method.DeclaringType.FullName)
                    {
                        continue;
                    }
                }
                else if (typeName != null)
                {
                    if (typeName != method.DeclaringType.FullName)
                    {
                        continue;
                    }
                }
                else continue;
                if (attribute.HasProperties)
                {
                    if (attribute.Properties[0].Name != "ParameterTypes") continue;
                    var types = (CustomAttributeArgument[])attribute.Properties[0].Argument.Value;
                    if (!Enumerable.SequenceEqual(
                        method.Parameters.Select(p => p.ParameterType.FullName),
                        types.Select(t => ((TypeReference)t.Value).FullName)))
                    {
                        continue;
                    }
                }
                return true;
            }
            return false;
//            return testMethod.CustomAttributes
//                .Where(a => a.AttributeType.Name == typeof(MethodTestedAttribute).Name)
//                .Any(a => (a.ConstructorArguments[0].Value is TypeReference
//                           && ((TypeReference)a.ConstructorArguments[0].Value).Resolve().FullName == method.DeclaringType.FullName)
//                          || (a.ConstructorArguments[0].Value is string
//                              && (string)a.ConstructorArguments[0].Value == method.DeclaringType.FullName)
//                          && (string)a.ConstructorArguments[1].Value == method.Name);
        }

        internal static string GetQualifiedName(this MethodDefinition method)
        {
            return string.Format("{0}.{1}",
                                 method.DeclaringType.FullName,
                                 method.Name);
        }
    }
}
