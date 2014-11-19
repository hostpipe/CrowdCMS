<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
	/**
	 * This is login example page that used ASP.Net FormsAuthentication to verify user access.
	 * Use this page together with the Moxiecode.Manager.Authenticators.ASPNETAuthenticator.
	 */
	void Login_Click(Object sender, EventArgs e) {
		if (Page.IsValid) {
			if (FormsAuthentication.Authenticate(login.Text, password.Text))
				Response.Redirect(Request["return_url"], true);
			else {
				message.Text = "Wrong username/password.";
				message.Visible = true;
			}
		}
	}
</script>


<html>
<head>
<title>Sample login page (ASPNETAuthenticator)</title>
<style>
body, td { font-family: Arial, Verdana; font-size: 11px; }
fieldset { display: block; width: 170px; }
legend { font-weight: bold; }
label { display: block; }
div { margin-bottom: 10px; }
div.last { margin: 0; }
div.container { position: absolute; top: 50%; left: 50%; margin: -100px 0 0 -85px; }
h1 { font-size: 14px; }
.button { border: 1px solid gray; font-family: Arial, Verdana; font-size: 11px; }
.error { color: red; margin: 0; margin-top: 10px; display: block; }
</style>
</head>
<body>

<div class="container">
	<form runat="server">
		<input type="hidden" name="return_url" value="<%= Server.HtmlEncode(Request["return_url"]) %>" />

		<fieldset>
			<legend>Example login</legend>

			<div>
				<label>Username:</label>
				<asp:TextBox id="login" class="text" runat="server"></asp:TextBox>
			</div>

			<div>
				<label>Password:</label>
				<asp:TextBox id="password" class="text" runat="server"></asp:TextBox>
			</div>

			<div>
				<asp:button id="login_button" onclick="Login_Click" text="Login" runat="server" />
			</div>

			<div class="last">
				<table border="0">
					<tr>
						<td><asp:CheckBox id="remember" runat="server" /></td>
						<td><span>Remember credentials?</span> </td>
					</tr>
				</table>
			</div>

			<asp:Label id="message" class="error" visible="false" runat="server" />
		</fieldset>
	</form>
</div>

</body>
</html>
