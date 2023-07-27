// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//const myElement = document.getElementById('hoursInput');
//console.log(myElement);

function startTimer(duration) {
    var timerDisplay = document.getElementById("timer");

    var timer = duration;
    var hours, minutes, seconds;

    var intervalId = setInterval(function () {
        hours = parseInt(timer / 3600, 10);
        minutes = parseInt((timer % 3600) / 60, 10);
        seconds = parseInt(timer % 60, 10);

        hours = hours < 10 ? "0" + hours : hours;
        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        timerDisplay.textContent = hours + ":" + minutes + ":" + seconds;

        if (--timer < 0) {
            clearInterval(intervalId);
            // Perform any action you need when the timer reaches 00:00:00
            alert("Timer has expired!");
        }
    }, 1000);
}

// Set the initial expiration time in JavaScript (2 hours in this example)
/*var hoursInput = myElement.dataset.hoursInput;; // Replace this with the desired hours from your input*/
var hoursInput = 2;
console.log(hoursInput);

var expirationTime = Math.floor(Date.now() / 1000) + hoursInput * 3600;

// Store the expiration time in localStorage
localStorage.setItem("expiration_time", expirationTime);

// Get the expiration time from localStorage
var currentTime = Math.floor(Date.now() / 1000);
var remainingTime = expirationTime - currentTime;
startTimer(remainingTime);