'
' VB Example of the template plugin.
'

Imports System
Imports System.IO
Imports System.Collections
Imports System.Collections.Specialized
Imports Moxiecode.Manager
Imports Moxiecode.Manager.FileSystems
Imports Moxiecode.Manager.Utils

Namespace Moxiecode.Manager.Plugins
	' <summary>
	'  This is a template plugin to be used to create new plugins. Rename all Template references below to your plugins name
	'  and implement the methods you need.
	' </summary>
	Public Class TemplatePlugin
		Inherits Moxiecode.Manager.Plugin

		' <summary>
		'  Constructor for the TemplatePlugin.
		' </summary>
		Public Sub New()
		End Sub
 
		' <summary>
		'  Short name for the plugin, used in the authenticator config option for example
		'  so that you don't need to write the long name for it namespace.classname.
		' </summary>
		Public Overrides ReadOnly Property ShortName As String
			Get 
				Return "Template"
			End Get
		End Property
 
		' <summary>
		'  Gets called on a authenication request. This method should check sessions or simmilar to verify that the user has access to the backend.
		'  This method should return true if the current request is authenicated or false if it's not.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <returns>true/false if the user is authenticated</returns>
		Public Overrides Function OnAuthenticate(ByVal man As ManagerEngine) As Boolean
			Return False
		End Function
 
		' <summary>
		'  Gets called after any authenication is performed and verified.
		'  This method should return false if the execution chain is to be broken.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnInit(ByVal man As ManagerEngine) As Boolean
			Dim config As ManagerConfig =  man.Config 
 
			' Override a config option
			config("somegroup.someoption") = "somevalue"
 
			Return True
		End Function
 
		' <summary>
		'  Gets called when a user has logged in to the system. This event should be dispatched from the login page.
		'  These events is not fired internaly and should be fired/dispatched externally.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnLogin(ByVal man As ManagerEngine) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called when a user has logged out from the system. This event should be dispatched from the logout page.
		'  These events is not fired internaly and should be fired/dispatched externally.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnLogout(ByVal man As ManagerEngine) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called before a file action occurs for example before a rename or copy.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="action">File action type.</param>
		' <param name="file1">File object 1 for example from in a copy operation.</param>
		' <param name="file2">File object 2 for example to in a copy operation. Might be null in for example a delete.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnBeforeFileAction(ByVal man As ManagerEngine, ByVal action As FileAction, ByVal file1 As IFile, ByVal file2 As IFile) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called after a file action was perforem for example after a rename or copy.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="action">File action type.</param>
		' <param name="file1">File object 1 for example from in a copy operation.</param>
		' <param name="file2">File object 2 for example to in a copy operation. Might be null in for example a delete.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnFileAction(ByVal man As ManagerEngine, ByVal action As FileAction, ByVal file1 As IFile, ByVal file2 As IFile) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called before a RPC command is handled.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">RPC Command to be executed.</param>
		' <param name="input">RPC input object data.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnBeforeRPC(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As Hashtable) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets executed when a RPC command is to be executed.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">RPC Command to be executed.</param>
		' <param name="input">RPC input object data.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnRPC(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As Hashtable) As Object
			Return Nothing
		End Function
 
		' <summary>
		'  Gets called before data is streamed to client.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Stream command that is to be performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnBeforeStream(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called when data is streamed to client. This method should setup HTTP headers, content type
		'  etc and simply send out the binary data to the client and the return false ones that is done.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Stream command that is to be performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnStream(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called after data was streamed to client.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Stream command that is to was performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnAfterStream(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called before data is streamed/uploaded from client.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Upload command that is to be performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnBeforeUpload(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called when data is streamed/uploaded from client. This method should take care of
		'  any uploaded files and move them to the correct location.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Upload command that is to be performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnUpload(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Object
			Return Nothing
		End Function
 
		' <summary>
		'  Gets called before data is streamed/uploaded from client.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="cmd">Upload command that is to was performed.</param>
		' <param name="input">Array of input arguments.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnAfterUpload(ByVal man As ManagerEngine, ByVal cmd As String, ByVal input As NameValueCollection) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called when custom data is to be added for a file custom data can for example be
		'  plugin specific name value items that should get added into a file listning.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="file">File reference to add custom info/data to.</param>
		' <param name="type">Where is the info needed for example list or info.</param>
		' <param name="custom">Name/Value array to add custom items to.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnCustomInfo(ByVal man As ManagerEngine, ByVal file As IFile, ByVal type As String, ByVal custom As Hashtable) As Boolean
			Return True
		End Function
 
		' <summary>
		'  Gets called when the user selects a file and inserts it into TinyMCE or a form or similar.
		' </summary>
		' <param name="man">ManagerEngine reference that the plugin is assigned to.</param>
		' <param name="file">Implementation of the BaseFile class that was inserted/returned to external system.</param>
		' <returns>true/false if the execution of the event chain should continue execution.</returns>
		Public Overrides Function OnInsertFile(ByVal man As ManagerEngine, ByVal file As IFile) As Boolean
			Return True
		End Function
	End Class
End Namespace
