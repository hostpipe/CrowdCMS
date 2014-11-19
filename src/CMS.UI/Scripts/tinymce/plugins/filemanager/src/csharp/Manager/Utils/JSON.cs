/*
 * $Id: JSON.cs 9 2007-05-27 10:47:07Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace Moxiecode.Manager.Utils {
	/// <summary>
	/// Description of JSON.
	/// </summary>
	public class JSON {
		/// <summary>
		/// 
		/// </summary>
		public static void SerializeRPC(string id, object error, object obj, Stream stream) {
			JSONWriter writer = new JSONWriter(new StreamWriter(stream));

			// Start JSON output
			writer.WriteStartObject();
			writer.WritePropertyName("result");

			// Serialize result set
			if (obj is Moxiecode.Manager.Utils.ResultSet) {
				ResultSet rs = (ResultSet) obj;

				writer.WriteStartObject();

				// Write header
				writer.WritePropertyName("header");
				writer.WriteStartObject();

				foreach (string key in rs.Header.Keys) {
					writer.WritePropertyName(key);
					writer.WriteValue(rs.Header[key]);
				}

				writer.WriteEndObject();

				// Write columns
				writer.WritePropertyName("columns");
				writer.WriteStartArray();

				foreach (string col in rs.Columns)
					writer.WriteValue(col);

				writer.WriteEndArray();

				// Write data
				writer.WritePropertyName("data");
				writer.WriteStartArray();

				foreach (ArrayList row in rs.Data) {
					writer.WriteStartArray();

					foreach (object item in row)
						WriteValue(writer, item);

					writer.WriteEndArray();
				}

				writer.WriteEndArray();

				// Write config
				if (rs.Config != null) {
					writer.WritePropertyName("config");
					writer.WriteStartObject();

					foreach (string key in rs.Config.AllKeys) {
						writer.WritePropertyName(key);
						writer.WriteValue(rs.Config[key]);
					}

					writer.WriteEndObject();
				}

				// End result
				writer.WriteEndObject();
			} else
				WriteValue(writer, obj);

			// Write id
			writer.WritePropertyName("id");
			writer.WriteValue(id);

			// Write error
			writer.WritePropertyName("error");
			writer.WriteValue(null);

			// Close
			writer.WriteEndObject();
			writer.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="obj"></param>
		public static void WriteObject(TextWriter writer, object obj) {
			WriteValue(new JSONWriter(writer), obj);
			writer.Flush();
		}

		private static void WriteValue(JSONWriter writer, object obj) {
			if (obj == null)
				writer.WriteNull();

			if (obj is System.String)
				writer.WriteValue((string) obj);

			if (obj is System.Boolean)
				writer.WriteValue((bool) obj);

			if (obj is System.Double)
				writer.WriteValue(Convert.ToDouble(obj));

			if (obj is System.Int32)
				writer.WriteValue(Convert.ToInt32(obj));

			if (obj is System.Int64)
				writer.WriteValue(Convert.ToInt64(obj));

			if (obj is ArrayList) {
				writer.WriteStartArray();

				foreach (object val in ((ArrayList) obj))
					WriteValue(writer, val);

				writer.WriteEndArray();
			}

			if (obj is NameValueCollection) {
				writer.WriteStartObject();

				string[] keys = GetReversedKeys(obj);
				foreach (string key in keys) {
					writer.WritePropertyName(key);
					WriteValue(writer, ((NameValueCollection) obj)[key]);
				}

				writer.WriteEndObject();
			}

			if (obj is Hashtable) {
				writer.WriteStartObject();

				string[] keys = GetReversedKeys(obj);
				foreach (string key in keys) {
					writer.WritePropertyName((string) key);
					WriteValue(writer, ((Hashtable) obj)[key]);
				}

				writer.WriteEndObject();
			}
		}

		private static string[] GetReversedKeys(object obj) {
			ICollection keyCollection;
			string[] keys;
			int count;

			if (obj is Hashtable)
				keyCollection = ((Hashtable) obj).Keys;
			else
				keyCollection = ((NameValueCollection) obj).AllKeys;

			count = keyCollection.Count;
			keys = new string[count];

			foreach (string key in keyCollection)
				keys[--count] = key;

			Array.Sort(keys);

			return keys;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static object ParseJSON(TextReader reader) {
			return ReadValue(new JSONReader(reader));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static JSONRpcCall ParseRPC(TextReader reader) {
			JSONRpcCall call = new JSONRpcCall();
			object obj = ParseJSON(reader);
			Hashtable jsonRpc = (Hashtable) obj;

			call.Method = (string) jsonRpc["method"];
			call.Id = (string) jsonRpc["id"];
			call.Args = (Hashtable) ((ArrayList) jsonRpc["params"])[0];

			return call;
		}

		#region private methods

/*		private static void Debug(string str) {
			System.Web.HttpContext.Current.Trace.Write(str);
		}*/

		private static object ReadValue(JSONReader reader) {
			Stack parents = new Stack();
			object cur = null;
			string key = null;
			object obj;

			while (reader.Read()) {
				//Debug(reader.ToString());

				switch (reader.TokenType) {
					case JSONToken.Boolean:
					case JSONToken.Integer:
					case JSONToken.String:
					case JSONToken.Float:
					case JSONToken.Null:
						if (cur is Hashtable) {
							//Debug(key + "=" + reader.ToString());
							((Hashtable) cur)[key] = reader.Value;
						} else if (cur is ArrayList)
							((ArrayList) cur).Add(reader.Value);
						else
							return reader.Value;

						break;

					case JSONToken.PropertyName:
						key = (string) reader.Value;
						break;

					case JSONToken.StartArray:
					case JSONToken.StartObject:
						if (reader.TokenType == JSONToken.StartObject)
							obj = new Hashtable();
						else
							obj = new ArrayList();

						if (cur is Hashtable) {
							//Debug(key + "=" + reader.ToString());
							((Hashtable) cur)[key] = obj;
						} else if (cur is ArrayList)
							((ArrayList) cur).Add(obj);

						parents.Push(cur);
						cur = obj;

						break;

					case JSONToken.EndArray:
					case JSONToken.EndObject:
						obj = parents.Pop();

						if (obj != null)
							cur = obj;

						break;
				}
			}

			return cur;
		}
		
		private static object ReadValue2(JSONReader jsonReader) {
			jsonReader.Read();

			switch (jsonReader.TokenType) {
				case JSONToken.Boolean:
					return (bool) jsonReader.Value;

				case JSONToken.Integer:
					return Convert.ToInt32(jsonReader.Value);

				case JSONToken.String:
					return (string) jsonReader.Value;

				case JSONToken.Float:
					return (double) jsonReader.Value;

				case JSONToken.Null:
					return null;

				case JSONToken.StartObject:
					Hashtable hash = new Hashtable();

					while (jsonReader.TokenType != JSONToken.EndObject) {
						if (jsonReader.TokenType == JSONToken.PropertyName)
							hash[jsonReader.Value] = ReadValue(jsonReader);
						else
							jsonReader.Read();
					}

					return hash;

				case JSONToken.StartArray:
					ArrayList list = new ArrayList();

					while (jsonReader.TokenType != JSONToken.EndArray) {
						if (jsonReader.TokenType == JSONToken.EndArray && jsonReader.Value == null)
							break;

						list.Add(ReadValue(jsonReader));
					}

					return list;
			}

			return null;
		}

		private static bool FindNext(JSONReader reader, JSONToken token) {
			if (reader.TokenType == token)
				return true;

			while (reader.Read() && reader.TokenType != JSONToken.EndObject && reader.TokenType != JSONToken.EndArray) {
				if (reader.TokenType == token)
					return true;
			}

			return false;
		}

		private static bool FindNextValue(JSONReader reader) {
			switch (reader.TokenType) {
				case JSONToken.Boolean:
				case JSONToken.Float:
				case JSONToken.Integer:
				case JSONToken.Null:
				case JSONToken.String:
					return true;
			}

			while (reader.Read() && reader.TokenType != JSONToken.EndObject) {
				switch (reader.TokenType) {
					case JSONToken.Boolean:
					case JSONToken.Float:
					case JSONToken.Integer:
					case JSONToken.Null:
					case JSONToken.String:
						return true;
				}
			}

			return false;
		}

		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public class JSONRpcCall {
		private string id, method;
		private Hashtable args;

		/// <summary>
		/// 
		/// </summary>
		public JSONRpcCall() {
			this.args = new Hashtable();
		}

		/// <summary>
		/// 
		/// </summary>
		public string Method {
			get { return method; }
			set { method = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Id {
			get { return id; }
			set { id = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Hashtable Args {
			get { return args; }
			set { args = value; }
		}
	}
}
