<?php
/*
Filename : index.php
The main page of the web application. 
*/
require "MyCurl.class.php";
?>


<?php
$url = 'http://localhost:6058/api/';
if (isset($_GET["getInfo"])) {
	$response = MyCurl::getRemoteFile($url . "users/", "application/json");
    echo "<pre>" . htmlentities($response) . "</pre><hr/>";
}
	

$loggedIn = false;
//To store user id. 
$tempId="";
//Get single user information
if (isset($_POST["login"])) {  
   if(isset($_POST['uname']) && isset($_POST['psw'])){
	   $username = $_POST['uname'];
	   $password = $_POST['psw'];
	   
	   $payload = json_encode( array( "username" => $username, "password" => $password));
	   //Sending username and password to API
	   $ch = curl_init( $url . "users/login");
	   curl_setopt( $ch, CURLOPT_POSTFIELDS, $payload );
	   curl_setopt( $ch, CURLOPT_HTTPHEADER, array('Content-Type:application/json'));
	   # Return response instead of printing.
	   curl_setopt( $ch, CURLOPT_RETURNTRANSFER, true );
	   # Send request.
	   $result = curl_exec($ch);
	   curl_close($ch);
	   
	   //Decoding response for Username and password
	   $responseUserID = json_decode($result, true);
	  	 
	   //Send Usename and Password to API for authentication
		if($responseUserID != null)
		{		 
		   $loggedIn = true;
		   //Get user ID in response 
		   $tempId = $responseUserID['uid'];
		   echo $tempId;
		   echo "usertype: " . $responseUserID['userType'];
	  
			header("location:userInfo.php?id=".$tempId . "&userType=". $responseUserID['userType']);
	    } 
	    else
	    {
		   echo "<div align='center' class='alert alert-warning'>
		   <strong>Warning!</strong> Invalid username or password! Try again.
		   </div>";
	    }
   }
}

//Get single user score based on ID
if (isset($_POST["GetSingleUserScore"])&& isset($_POST["userid"])) {	
	$tempId = $_POST['userid'];
	echo $tempId;
	$response = MyCurl::getRemoteFile($url . "users/". $tempId . "/scores/", "application/json");
	$json1 = json_decode($response, true);
	echo "<pre>";
	var_dump($json1);
	echo "</pre>";
}

//Get all users information for Admin
if (isset($_POST["GetAllUserInfo"])) 
{	
	$response = MyCurl::getRemoteFile($url . "users/", "application/json");
	$jsonAllUsers = json_decode($response, true);
	echo "<pre>";
	var_dump($jsonAllUsers);
	echo "</pre>";
	$displayAllUserText = "";	
	/**foreach ($jsonAllUsers as $persons){
		foreach ($persons as $app){
		 $UID = $app['@attributes']['id'];
		 $Name = $app['@attributes']['name'];
		 $Uname = $app['@attributes']['username'];
		 $IMEI = $app['@attributes']['imei'];
		
		$displayAllUserText .= "<span>Username:</span>" . $Uname . "<br/>";
        $displayAllUserText .= "<span>User ID:</span>" . $UID . " ";
        $displayAllUserText .= "<span>Name:</span>" . $name . "<br/>";
        $displayAllUserText .= "<span>IMEI:</span>" . $imei . "<br/>";
		
		echo $displayAllUserText;}
		
	***/
}
$DisplayLogin = "";
$DisplaySingleUserScore = "";
?>

<!doctype html>
<html lang="en">
<head>
    <meta charset="'utf-8"/>
    <title> Android Security System</title>
	 <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
	 <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css">
	<script src="https://code.jquery.com/jquery-3.2.1.slim.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js"></script>
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" ></script>
	<style>
	body {font-family: Arial, Helvetica, sans-serif;}

/* Full-width input fields */
input[type=text], input[type=password] {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
}

/* Set a style for all buttons */
button {
    background-color: #4CAF50;
    color: white;
    padding: 14px 20px;
    margin: 8px 0;
    border: none;
    cursor: pointer;
    width: 100%;
}

button:hover {
    opacity: 0.8;
}

/* Extra styles for the cancel button */
.cancelbtn {
    width: auto;
    padding: 10px 18px;
    background-color: #f44336;
}

/* Center the image and position the close button */
.imgcontainer {
    text-align: center;
    margin: 24px 0 12px 0;
    position: relative;
}

img.avatar {
    width: 40%;
    border-radius: 50%;
}

.container {
    padding: 16px;
}

span.psw {
    float: right;
    padding-top: 16px;
}

/* The Modal (background) */
.modal {
    display: none; /* Hidden by default */
    position: fixed; /* Stay in place */
    z-index: 1; /* Sit on top */
    left: 0;
    top: 0;
    width: 100%; /* Full width */
    height: 100%; /* Full height */
    overflow: auto; /* Enable scroll if needed */
    background-color: rgb(0,0,0); /* Fallback color */
    background-color: rgba(0,0,0,0.4); /* Black w/ opacity */
    padding-top: 60px;
}

/* Modal Content/Box */
.modal-content {
    background-color: #fefefe;
    margin: 5% auto 15% auto; /* 5% from the top, 15% from the bottom and centered */
    border: 1px solid #888;
    width: 80%; /* Could be more or less, depending on screen size */
}

/* The Close Button (x) */
.close {
    position: absolute;
    right: 25px;
    top: 0;
    color: #000;
    font-size: 35px;
    font-weight: bold;
}

.close:hover,
.close:focus {
    color: red;
    cursor: pointer;
}

/* Add Zoom Animation */
.animate {
    -webkit-animation: animatezoom 0.6s;
    animation: animatezoom 0.6s
}

@-webkit-keyframes animatezoom {
    from {-webkit-transform: scale(0)} 
    to {-webkit-transform: scale(1)}
}
    
@keyframes animatezoom {
    from {transform: scale(0)} 
    to {transform: scale(1)}
}

/* Change styles for span and cancel button on extra small screens */
@media screen and (max-width: 300px) {
    span.psw {
       display: block;
       float: none;
    }
    .cancelbtn {
       width: 100%;
    }
}


</style>
</head>
	<body style="margin-top:100px; background-color:#6b7a89;">
	
	<center>
	<!--onclick="document.getElementById('id02').style.display='block'" -->
	<!--<form class='modal-content animate' action='index.php' method='POST'>
		 <input type='hidden' name='userid' value='<?php //echo $tempId ?>'>
		  <button  type='submit' style='width:auto;' name='GetSingleUserScore'>Get Score</button>
	</form>
	<div id="id02" class="modal">
	  <?php
	  //echo "<pre>";
		//var_dump($json1);
		//echo "</pre>";
	  ?>
	</div> 
	
	<form class="modal-content animate" action="index.php" method="POST">
		  <button  type="submit" style="width:auto;" name="GetAllUserInfo">Get All Users</button>
	</form>
	<div id="id03" class="modal">
	  <?php
	  //echo "<pre>";
		//var_dump($jsonAllUsers);
		//echo "</pre>";
	  ?>-->
	
	
	<!-- Display login form -->
	
	<h2>Android Security System</h2>

	<button onclick="document.getElementById('id01').style.display='block'" style="width:auto;">Login</button>
	</center>
	<div id="id01" class="modal">
	  
	  <form class="modal-content animate" action="index.php" method="POST" style="background-color:#c8cfd6">


		<div class="container" >
		  <label for="uname"><b>Username</b></label>
		  <input type="text" placeholder="Enter Username" name="uname" required>

		  <label for="psw"><b>Password</b></label>
		  <input type="password" placeholder="Enter Password" name="psw" required>
			
		  <button type="submit" name="login">Login</button>
		  <label>
			<input type="checkbox" checked="checked" name="remember"> Remember me
		  </label>
		</div>

		<div class="container" style="background-color:#c8cfd6">
		  <button type="button" onclick="document.getElementById('id01').style.display='none'" class="cancelbtn">Cancel</button>
		 <!-- <span class="psw">Forgot <a href="#">password?</a></span>-->
		</div>
	  </form>
	</div>





	<script>
	// Get the modal
	var modal = document.getElementById('id01');

	// When the user clicks anywhere outside of the modal, close it
	window.onclick = function(event) {
		if (event.target == modal) {
			modal.style.display = "none";
		}
	}
	</script>
		
	</body>
</html>






