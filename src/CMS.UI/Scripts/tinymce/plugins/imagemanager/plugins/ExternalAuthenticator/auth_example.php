<?php
	// Change this secret key so it matches the one in the imagemanager/filemanager config
	$secretKey = "someSecretKey";

	// Override any config values here
	$config = array();
	// $config['filesystem.rootpath'] = 'c:/Inetpub/wwwroot/filemanager';

	// Generates a unique key of the config values with the secret key
	$key = md5(implode('', array_values($config)) . $secretKey);
?>

<html>
<body onload="document.forms[0].submit();">
<form method="post" action="<?php echo htmlentities($_GET['return_url']); ?>">
<input type="hidden" name="key" value="<?php echo htmlentities($key); ?>" />
<?php
	foreach ($config as $key => $value) {
		echo '<input type="hidden" name="' . htmlentities(str_replace('.', '__', $key)) . '" value="' . htmlentities($value) . '" />';
	}
?>
</form>
</body>
</html>