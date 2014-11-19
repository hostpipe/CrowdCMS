CrowdCMS
========

Crowd CMS is a Web Content Management System Built Using ASP.Net MVC 4

Website: http://www.crowdcms.co.uk


What's in the package
=====================

'src' folder
------------
This contains the full source code as a visual studio solution. It's built uing ASP.Net MVC 4 in c# using the Razor view engine.


'db' folder
----------
This folder holds the SQL script that will allow you to set up the initial data for the Crowd CMS database. An automnated setup wizard is on the development path!


'lib' folder
------------
This contains associated libraries that you may need.


Initial Setup 
=============

Server Permissions
------------------
You will need to grant permissions on a few folders to ensure the system can run correctly

	* /Sitemaps	  grant read and write to your ASP.NET identity
	* /Images     grant read/write access to your ASP.Net identity (including subfolders)
	* /docs		  grant read/write access to your ASP.Net identity (inclding subfolders)
	* /Logs		  grant read and write to your ASP.NET identity.  Errors will be logged in here (useful for release versions)

Update CMS.UI/web.config with the appropriate connection string for your server and enter email settings.

	1. You'll need to add your data source, database, user and password
    2. Generate your database creation script from the CMS.BL Entity Model (Again, an automnated setup wizard is on our development path!)
	3. Change the <smtp... > "from" attribute to your email address
	4. Update the <network... > tag with the correct credentials for your system


Need help?
==========

You can make contact with the folks behind Crowd CMS via Twitter - [@CrowdCMS](https://twitter.com/crowdcms).