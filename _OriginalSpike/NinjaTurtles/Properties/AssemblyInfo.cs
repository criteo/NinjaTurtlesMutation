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

using System.Reflection;

[assembly: AssemblyTitle("NinjaTurtles")]
[assembly: AssemblyDescription("Mutation testing library for .NET")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("NinjaTurtles")]
[assembly: AssemblyProduct("NinjaTurtles")]
[assembly: AssemblyCopyright("Copyright © 2012 David Musgrove")]

[assembly: AssemblyVersion("0.1.0.3")]
[assembly: AssemblyFileVersion("0.1.0.3")]
