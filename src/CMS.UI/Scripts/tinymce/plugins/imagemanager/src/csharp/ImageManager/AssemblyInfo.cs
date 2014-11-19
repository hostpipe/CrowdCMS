/*
 * $Id: AssemblyInfo.cs 861 2012-04-11 12:24:30Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// Information about this assembly is defined by the following
// attributes.
//
// change them to the information which is associated with the assembly
// you compile.

#if (UNSAFE)
[assembly: AssemblyTitle("Moxiecode ImageManager Plugin")]
#else
[assembly: AssemblyTitle("Moxiecode ImageManager Plugin (Safe)")]
#endif

[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("Full")]
[assembly: AssemblyCompany("Moxiecode Systems AB")]
[assembly: AssemblyProduct("MCImageManager")]
[assembly: AssemblyCopyright("Moxiecode Systems AB")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible(false)]

// Needed for some security exception
[assembly:AllowPartiallyTrustedCallers]
[assembly: CLSCompliant(true)]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all values by your own or you can build default build and revision
// numbers with the '*' character (the default):

//[assembly: AssemblyVersion("1.0.0.0")]
//[assembly: AssemblyKeyFile("MCImageManager.snk")]
