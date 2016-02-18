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
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using System;
using System.Reflection;

using Mono.Cecil;

namespace NinjaTurtles
{
    static internal class TypeReferenceExtensions
    {
        static public string ToAssemblyQualifiedName(this TypeReference typeReference)
        {
            string assemblyQualifiedName = Assembly.CreateQualifiedName(typeReference.Scope.Name, typeReference.FullName);
            if (typeReference.IsGenericInstance)
            {
                foreach (var genericParameter in ((GenericInstanceType)typeReference).GenericArguments)
                {
                    assemblyQualifiedName = assemblyQualifiedName.Replace(genericParameter.FullName,
                                                                          genericParameter.ToAssemblyQualifiedName());
                }
            }
            return assemblyQualifiedName;
        }

        static public Type ToSystemType(this TypeReference typeReference, Assembly assembly)
        {
            string fullNameToResolve = typeReference.ToAssemblyQualifiedName();
            Type type = null;
            try
            {
                type = Type.GetType(fullNameToResolve);
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {}
            return type ?? TypeResolver.ResolveTypeFromReferences(assembly, fullNameToResolve);
        }
    }
}
