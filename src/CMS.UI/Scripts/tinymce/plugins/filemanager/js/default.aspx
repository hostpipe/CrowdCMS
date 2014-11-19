<%@ Page Language="C#" ContentType="text/javascript" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Moxiecode.Manager" %>
<%@ Import Namespace="Moxiecode.Manager.Utils" %>
<%
	string type = StringUtils.Sanitize(Request["type"]), theme = StringUtils.Sanitize(Request["theme"]), package = StringUtils.Sanitize(Request["package"]);
	ClientResources resources = new ClientResources();

	ManagerEngine man = new ManagerEngine(type);
	man.DispatchEvent(EventType.Init);

	resources.Load("../pages/" + theme + "/resources.xml");

	// Load plugin resources
	foreach (Plugin plugin in man.Plugins) {
		if (plugin.ShortName != null)
			resources.TryLoad("../plugins/" + plugin.ShortName + "/resources.xml");
	}

	ClientResourceFile[] files = resources.GetFiles(package);

	if (resources.IsDebugEnabled || man.Config["general.debug"] == "true") {
		string pagePath = PathUtils.ToUnixPath(Path.GetDirectoryName(HttpContext.Current.Request.Url.AbsolutePath));
		Response.Write("// Debug enabled, scripts will be loaded without compression\n");

		foreach (ClientResourceFile file in files)
			Response.Write("document.write('<script type=\"text/javascript\" src=\"" + pagePath + "/" + file.Path + "\"></script>');\n");
	} else {
		JSCompressor compressor = new JSCompressor();

		compressor.CacheFileName = theme + "_" + package;
		compressor.GzipCompress = true;

		foreach (ClientResourceFile file in files)
			compressor.AddFile(file.Path, file.RemoveWhiteSpace);

		compressor.Compress(Request, Response);
	}
%>