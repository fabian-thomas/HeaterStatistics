var request = new XMLHttpRequest();
request.open("GET", "data.json");
request.addEventListener('load', function(event) {
    if (request.status >= 200 && request.status < 300) {
        var obj = JSON.parse(request.responseText);
        document.getElementById("temperature").innerHTML = obj.temperature;
        document.getElementById("kessel").innerHTML = obj.kessel;
        document.getElementById("timestamp").innerHTML = obj.timestamp;
       console.log(request.responseText);
    } else {
       console.error(request.statusText, request.responseText);
    }
});
request.send();
