!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Current User</title>
</head>
<body>
The currently logged on user is <span id="username"><% =HttpContext.Current.User.Identity.Name %></span>.
</body>
</html>