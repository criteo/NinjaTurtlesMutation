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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using NLog;

namespace NinjaTurtles
{
	internal class TypeResolver
	{
        #region Logging

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #endregion

        private static Assembly LoadReferencedAssembly(AssemblyName reference, String codeBaseFolder)
        {
            Assembly referencedAssembly = null;

            try
            {
                referencedAssembly = Assembly.Load(reference);
            }
            catch (FileNotFoundException) { }
            if (referencedAssembly != null)
                return (referencedAssembly);
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                referencedAssembly = Assembly.LoadFile(Path.Combine(codeBaseFolder, reference.Name) + ".dll");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
            return (referencedAssembly);
        }

        internal static Type ResolveTypeFromReferences(Assembly callingAssembly, string className)
		{
            _log.Debug("Resolving type \"{0}\" in \"{1}\".", className, callingAssembly.GetName().Name);
            Type type = ResolveTypeFromReferences(callingAssembly, className, new List<string>());
            if (type == null)
            {
                _log.Error("Could not find type \"{0}\".", className);
            }
            return type;
		}

        private static Type ResolveTypeFromReferences(Assembly assembly, string className, IList<string> consideredAssemblies)
		{
            _log.Trace("Searching for type \"{0}\" in \"{1}\".", className, assembly.GetName().Name);
            var type = assembly.GetLoadableTypes().SingleOrDefault(t => t.FullName == className);
            if (type != null)
            {
                _log.Trace("Found type \"{0}\" in \"{1}\".", className, assembly.GetName().Name);
                return type;
            }
		    var codeBaseFolder = Path.GetDirectoryName(assembly.Location);
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
				if (consideredAssemblies.Contains(reference.Name))
                    continue;
				consideredAssemblies.Add(reference.Name);
                Assembly referencedAssembly = LoadReferencedAssembly(reference, codeBaseFolder);
                if (referencedAssembly == null)
                    continue;
                type = ResolveTypeFromReferences(referencedAssembly, className, consideredAssemblies);
                if (type == null)
                    continue;
                return type;
            }
            return null;
        }
    }
}

