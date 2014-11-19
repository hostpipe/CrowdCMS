<%@ Page Language="C#" %>
<script runat="server">
	/**
	 * This is login example page check the input parameters and sets up a session.
	 * Use this page together with the Moxiecode.Manager.Authenticators.SessionAuthenticator.
	 */

	string message = "";
	string username = "demo";
	string password = ""; // Change the password to something suitable

	void Page_Load(object sender, EventArgs e) {
		if (password == "")
			message = "You must set a password in the file \"login_session_auth.aspx\" inorder to login using this page or reconfigure it the authenticator config options to fit your needs. Consult the <a href=\"http://wiki.moxiecode.com/index.php/Main_Page\" target=\"_blank\">Wiki</a> for more details.";

		// Handle logic (this is where you should put your custom login logic)
		if (Request["login"] == username && Request["password"] == password && password != "") {
			// Set the sessions that the SessionAuthenticatorImpl class checks for
			Session["mc_isLoggedIn"] = "true";
			Session["mc_user"] = Request["login"];
			Session["mc_groups"] = "";

			// Override config options
			//Session["imagemanager.filesystem.rootpath"] = "some path";
			//Session["filemanager.filesystem.rootpath"] = "c:/";

			Response.Redirect(Request["return_url"], true);
		} else if (Request["submit_button"] != null) {
			message = "Wrong username/password.";
		}
	}
</script>

<html>
<head>
<title>Sample login page (SessionAuthenticator)</title>
<style>
body { font-family: Arial, Verdana; font-size: 11px; }
fieldset { display: block; width: 170px; }
legend { font-weight: bold; }
label { display: block; }
div { margin-bottom: 10px; }
div.last { margin: 0; }
div.container { position: absolute; top: 50%; left: 50%; margin: -100px 0 0 -85px; }
h1 { font-size: 14px; }
.button { border: 1px solid gray; font-family: Arial, Verdana; font-size: 11px; }
.error { color: red; margin: 0; margin-top: 10px; }
</style>
</head>
<body>

<div class="container">
	<form action="login_session_auth.aspx" method="post">
		<input type="hidden" name="return_url" value="<%= Server.HtmlEncode(Request["return_url"]) %>" />

		<fieldset>
			<legend>Example login</legend>

			<div>
				<label>Username:</label>
				<input type="text" name="login" class="text" value="<%= Server.HtmlEncode(Request["login"]) %>" />
			</div>

			<div>
				<label>Password:</label>
				<input type="password" name="password" class="text" value="<%= Server.HtmlEncode(Request["password"]) %>" />
			</div>

			<div class="last">
				<input type="submit" name="submit_button" value="Login" class="button" />
			</div>

<% if (message != "") { %>
			<div class="error">
				<%= message %>
			</div>
<% } %>
		</fieldset>
	</form>
</div>

</body>
</html>
