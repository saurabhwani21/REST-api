<?php 
/*
Filname: userScore.php
This page displays the score for a given user. 
*/
require "MyCurl.class.php";
$url = 'http://localhost:6058/api/';

//Logging out 
if (isset($_POST["logout"])) 
{
	echo "Logout pressed!";
	header("location:index.php");
}

//Getting single user score
if (isset($_POST["GetSingleUserScore"])) 
{
	// Extracting user id 	
	$tempId = $_POST['uid'];
	// Extracting user name
	$name = $_POST['name'];
	// Extracting IMEI of the device 
	$imei = $_POST['imei'];
	$response = MyCurl::getRemoteFile($url . "users/". $tempId . "/scores/", "application/json");
	$jsonScore = json_decode($response, true);
  
	// Displaying the user's score in table 
	$scoreTable  = "<table class='table table-striped'>";
	$scoreTable .= "<th>Score</th>";
	$scoreTable .= "<th align='center'>Timestamp</th>";
	$scoreTable .= "<th>Screen Lock</th>";
	$scoreTable .= "<th>OS</th>";
	$scoreTable .= "<th>Unknown Sources</th>";
	$scoreTable .= "<th>Harmful Apps</th>";
	$scoreTable .= "<th>Developer Options</th>";
	$scoreTable .= "<th>Basic Integrity</th>";
	$scoreTable .= "<th>Android Compatibility</th>";
	
	foreach($jsonScore as $key)
	{
		foreach($key as $value)
		{
			$scoreTable .= "<tr><td align='center'>" .$value["instanceScore"] . " </td>";
			$scoreTable .= "<td align='left'>" . str_replace("T", " ",$value["timestamp"]) . " </td>";
			$scoreTable .= "<td align='center'>" .$value["screenLock"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["os"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["unknownSources"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["harmfulApps"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["devOpt"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["integrity"] . " </td>";
			$scoreTable .= "<td align='center'>" .$value["compatibility"] . " </td></tr>";
		}
	}
	$scoreTable .= "</table>";
}
?>

<!DOCTYPE html>
<html lang="en">
<head>
  <title>Bootstrap Example</title>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
  <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
</head>
<body>
<form class="form-horizontal" action="index.php" method="POST">
<div class="container">
<h2> User Score Information</h2>
<?php echo $scoreTable; ?>
    <div class="form-group">        
      <div class="col-sm-offset-1 col-sm-2">
        <button type="submit" class="btn btn-default" style="background-color:#4CAF50; color:white" name="logout">Logout</button>
      </div>
    </div>
	  </form>
</body>
</html>