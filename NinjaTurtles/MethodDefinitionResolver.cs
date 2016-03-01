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
using System.Linq;

using Mono.Cecil;

using NLog;

namespace NinjaTurtles
{
    internal class MethodDefinitionResolver
    {
        #region Logging

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #endregion

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string methodName)
        {
            _log.Debug("Resolving method \"{0}\" in \"{1}\".", methodName, typeDefinition.FullName);
            try
            {
                MethodDefinition methodDefinition = typeDefinition.Methods.Single(m => m.Name == methodName);
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                return methodDefinition;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Sequence contains no matching element")
                {
                    _log.Error("Method \"{0}\" is unrecognised.", methodName);
                    throw new ArgumentException(string.Format("Method \"{0}\" is unrecognised.", methodName), "methodName");
                }
                _log.Error("Method \"{0}\" is overloaded.", methodName);
                throw new ArgumentException(string.Format("Method \"{0}\" is overloaded.", methodName), "methodName");
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string returnType, string methodName, GenericParameter[] methodGenerics)
        {
            _log.Debug("Resolving method \"{0}\" in \"{1}\".", methodName, typeDefinition.FullName);
            try
            {
                MethodDefinition methodDefinition = typeDefinition.Methods.Single(
                    m => m.Name == methodName &&
                        m.ReturnType.FullName == returnType && 
                            m.GenericParameters.Select(g => g.FullName)
                                .SequenceEqual(methodGenerics.Select(g => g.FullName)));
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                return methodDefinition;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Sequence contains no matching element")
                {
                    _log.Error("Method \"{0}\" is unrecognised.", methodName);
                    throw new ArgumentException(string.Format("Method \"{0}\" is unrecognised.", methodName), "methodName");
                }
                _log.Error("Method \"{0}\" is overloaded.", methodName);
                throw new ArgumentException(string.Format("Method \"{0}\" is overloaded.", methodName), "methodName");
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string methodName, TypeReference[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                _log.Warn("\"ResolveMethod\" overload with parameter types called unnecessarily.");
                return ResolveMethod(typeDefinition, methodName);
            }
            try
            {
                MethodDefinition methodDefinition =
                    typeDefinition.Methods.Single(
                        m => m.Name == methodName
                             && m.Parameters.Select(p => p.ParameterType.FullName)
                                 .SequenceEqual(parameterTypes.Select(p => p.FullName)));
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                return methodDefinition;
            }
            catch (InvalidOperationException)
            {
                _log.Error("Method \"{0}\" with specified parameter types is unrecognised.", methodName);
                throw new ArgumentException(string.Format("Method \"{0}\" with specified parameter types is unrecognised.", methodName), "methodName");
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string returnType, string methodName, GenericParameter[] methodGenerics, TypeReference[] parameterTypes)
        {
            Console.WriteLine("IN ResolveMethod: TypeDefinition [{0}], return type [{1}], methodName [{2}], generics [{3}], TypeReference[] fullname [[{4}]]", typeDefinition, returnType, methodName, string.Join("], [", methodGenerics.Select(g => g.FullName)), string.Join("], [", parameterTypes.Select(p => p.FullName)));
            if (parameterTypes == null)
            {
                _log.Warn("\"ResolveMethod\" overload with parameter types called unnecessarily.");
                return ResolveMethod(typeDefinition, methodName);
            }
            try
            {
                MethodDefinition methodDefinition =
                    typeDefinition.Methods.Single(
                        m => m.Name == methodName
                            && m.ReturnType.FullName == returnType
                                && m.Parameters.Select(p => p.ParameterType.FullName)
                                    .SequenceEqual(parameterTypes.Select(p => p.FullName))
                                        && m.GenericParameters.Select(g => g.FullName)
                                            .SequenceEqual(methodGenerics.Select(g => g.FullName)));
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                Console.WriteLine("OUT ResolveMethod: TypeDefinition [{0}], methodName [{1}], TypeReference[] [[{2}]]", typeDefinition, methodName, string.Join<TypeReference>("], [", parameterTypes));
                return methodDefinition;
            }
            catch (InvalidOperationException)
            {
                _log.Error("Method \"{0}\" with specified parameter types is unrecognised.", methodName);
                Console.WriteLine("OUT unrecognised ResolveMethod: TypeDefinition [{0}], methodName [{1}], TypeReference[] [[{2}]]", typeDefinition, methodName, string.Join<TypeReference>("], [", parameterTypes));
                throw new ArgumentException(string.Format("Method \"{0}\" with specified parameter types is unrecognised. TypeDefinition: [{1}] TypeReference[]: [[{2}]]", methodName, typeDefinition, string.Join<TypeReference>("], [", parameterTypes)), "methodName");
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string methodName, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                _log.Warn("\"ResolveMethod\" overload with parameter types called unnecessarily.");
                return ResolveMethod(typeDefinition, methodName);
            }
            try
            {
                MethodDefinition methodDefinition =
                    typeDefinition.Methods.Single(
                        m => m.Name == methodName
                             && m.Parameters.Select(p => p.ParameterType.Name.Replace("TypeDefinition", "Type"))
                                 .SequenceEqual(parameterTypes.Select(p => p.Name.Replace("TypeDefinition", "Type"))));
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                return methodDefinition;
            }
            catch (InvalidOperationException)
            {
                _log.Error("Method \"{0}\" with specified parameter types is unrecognised.", methodName);
                throw new ArgumentException(string.Format("Method \"{0}\" with specified parameter types is unrecognised.", methodName), "methodName");
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string returnType, string methodName, GenericParameter[] methodGenerics, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                _log.Warn("\"ResolveMethod\" overload with parameter types called unnecessarily.");
                return ResolveMethod(typeDefinition, methodName);
            }
            try
            {
                MethodDefinition methodDefinition =
                    typeDefinition.Methods.Single(
                        m => m.Name == methodName
                            && m.ReturnType.FullName == returnType
                             && m.Parameters.Select(p => p.ParameterType.Name.Replace("TypeDefinition", "Type"))
                                 .SequenceEqual(parameterTypes.Select(p => p.Name.Replace("TypeDefinition", "Type")))
                                    && m.GenericParameters.Select(g => g.FullName)
                                        .SequenceEqual(methodGenerics.Select(g => g.FullName)));
                _log.Debug("Method \"{0}\" successfully resolved in \"{1}\".", methodName, typeDefinition.FullName);
                return methodDefinition;
            }
            catch (InvalidOperationException)
            {
                _log.Error("Method \"{0}\" with specified parameter types is unrecognised.", methodName);
                throw new ArgumentException(string.Format("Method \"{0}\" with specified parameter types is unrecognised.", methodName), "methodName");
            }
        }
    }
}
