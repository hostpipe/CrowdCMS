/*
 * $Id: InstanceFactory.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Reflection;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	///  This class created dynamic instances of classes based on a string.
	/// </summary>
	public class InstanceFactory {
 		/// <summary>
 		///  Created a instance from the specified class name.
 		/// </summary>
 		/// <param name="class_name">Class name to create dynamic instance from.</param>
 		/// <returns>Dynamic class object instance reference.</returns>
		public static Object CreateInstance(string class_name) {
 			Type type = null;
 			string[] parts = class_name.Split(new char[] {','});

 			if (parts.Length == 2) {
 				Assembly.Load(parts[1]);
 				class_name = parts[0];
 			}

 			// Loop all asseblies
 			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
 				type = asm.GetType(class_name, false, true);
 				
 				// Found type
 				if (type != null)
 					break;
 			}
 
	 		if (type != null && type.IsClass) {
	 			try {
	 				return type.GetConstructor(new Type[0]).Invoke(null);
	 			} catch (Exception e) {
	 				throw new Exception("Could not create a dynamic instance of \"" + class_name + "\".", e);
	 			}
	 		} else
	 			throw new Exception("Could not create a dynamic instance of \"" + class_name + "\", type not found.");
		}
	}
}
