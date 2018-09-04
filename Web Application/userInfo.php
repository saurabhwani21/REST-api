<?php
/*
Filename: userInfo.php
This page displays the user information. 
*/
require "MyCurl.class.php";
$url = 'http://localhost:6058/api/';
$userId ="";

if(isset($_GET['id']) && isset($_GET['userType']))
{
	$userType = $_GET['userType'];
	$userId = $_GET['id'];

	//If user type is Regular user
	if($userType === 'regular')
	{	
		$response = MyCurl::getRemoteFile($url . "users/". $userId, "application/json");
		$jsonUserInfo = json_decode($response, true);
		$name = $jsonUserInfo['name'];
		$imei = $jsonUserInfo['imei'];
	}
	// For ADMIN type user 
	else
	{
		$response = MyCurl::getRemoteFile($url . "users/", "application/json");
		$allUserInfoArray = json_decode($response, true);
		foreach($allUserInfoArray as $item)
		{			
				$name = $item['name'];
				$imei = $item['imei'];		
		}			
}
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
<body style="background-color:#6b7a89;">

<div class="container">
  <h2>User Information</h2>
  <form class="form-horizontal" action="userScore.php" method="POST">
      <div class="form-group">
      <div class="col-sm-10">
        <span id='name'><input type="hidden" name="uid" value="<?php echo $userId; ?>"></span>
      </div>
      </div>
    <div class="form-group">
      <label class="control-label col-sm-1" for="email">Name:</label>
      <div class="col-sm-10">
        <span id='name'><?php echo $name; ?></span>
      </div>
    </div>
    <div class="form-group">
      <label class="control-label col-sm-1" for="pwd">IMEI:</label>
      <div class="col-sm-10">          
       <span id='imei'><?php echo $imei; ?></span> 
      </div>
    </div>
    <div class="form-group">        
      <div class="col-sm-offset-1 col-sm-2">
        <button type="submit" class="btn btn-default" style="background-color:#4CAF50; color:white" name="GetSingleUserScore">Get Score</button>
      </div>
    </div>
  </form>
</div>
<div class="container">
<?php echo $scoreTable; ?>
</div>
</body>
</html>




