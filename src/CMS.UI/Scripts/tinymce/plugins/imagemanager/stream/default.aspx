<%@ Page Language="C#" ValidateRequest="false" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="Moxiecode.Manager" %>
<%@ Import Namespace="Moxiecode.Manager.Utils" %>
<%
	string prefix, cmd, type, theme, package, file, domain;
	object result;
	ManagerEngine man;
	ClientResources resources = new ClientResources();

	// Get input
	cmd = Request["cmd"];
	domain = Request["domain"];
	type = StringUtils.Sanitize(Request["type"]);
	theme = StringUtils.Sanitize(Request["theme"]);
	package = StringUtils.Sanitize(Request["package"]);
	file = StringUtils.Sanitize(Request["file"]);

	if (package != null) {
		resources.Load("../pages/" + theme + "/resources.xml");

		if (type != null) {
			man = new ManagerEngine(type);
			man.DispatchEvent(EventType.Init);

			// Load plugin resources
			foreach (Plugin plugin in man.Plugins) {
				if (plugin.ShortName != null)
					resources.TryLoad("../plugins/" + plugin.ShortName + "/resources.xml");
			}
		}

		ClientResourceFile resourceFile = resources.GetFile(package, file);

		Response.ContentType = resourceFile.ContentType;
		Response.WriteFile(Server.MapPath(resourceFile.Path));
	} else {
		// Handle stream command
		try {
			if (cmd != null) {
				// Parse prefix
				Match match = Regex.Match(cmd, @"^([a-z]+)\.(.*)");
				prefix = match.Groups[1].Value;
				cmd = match.Groups[2].Value;

				// Setup file manager
				man = new ManagerEngine(prefix);

				if (man.IsAuthenticated) {
					// Initialize
					man.DispatchEvent(EventType.Init);

					// Dispatch events
					if (Request.RequestType.ToLower() == "get") {
						man.DispatchEvent(EventType.BeforeStream, cmd, Request.QueryString);
						man.DispatchEvent(EventType.Stream, cmd, Request.QueryString);
						man.DispatchEvent(EventType.AfterStream, cmd, Request.QueryString);
					} else if (Request.RequestType.ToLower() == "post") {
						NameValueCollection args = new NameValueCollection();

						args.Add(Request.Form);
						args.Add(Request.QueryString);

						man.DispatchEvent(EventType.BeforeUpload, cmd, args);
						result = man.ExecuteEvent(EventType.Upload, cmd, args);

						if (Request["html4"] == null) {
							JSON.SerializeRPC(
								"u1",
								null,
								result,
								Response.OutputStream
							);
						} else {
							Response.Write("<html><body><script type=\"text/javascript\">parent.handleJSON(");

							// Call RPC
							JSON.SerializeRPC(
								"u1",
								null,
								result,
								Response.OutputStream
							);

							Response.Write(");</script></body></html>");
						}

						man.DispatchEvent(EventType.AfterUpload, cmd, args);
					}
				} else
					throw new ManagerException("Not authenticated.");
			}
		} catch (ManagerException ex) {
			if (Request.RequestType.ToLower() == "post") {
				Response.Write("<html><body><script type=\"text/javascript\">");

				if (domain != null)
					Response.Write("document.domain='" + domain + "';");

				Response.Write("parent.handleJSON({\"result\":null,\"id\":null,\"error\":{\"errstr\":\"" + StringUtils.Escape(ex.Message) + "\",\"errfile\":\"\",\"errline\":null,\"errcontext\":\"\",\"level\":\"FATAL\"}}");
				Response.Write(");</script></body></html>");
			} else
				Response.Write(ex.Message);
		}
	}
%>