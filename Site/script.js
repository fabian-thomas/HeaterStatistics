var main_div = document.getElementById("main_div");

var request = new XMLHttpRequest();
request.open("GET", "data.json");
request.addEventListener("load", function(event) {
	if (request.status >= 200 && request.status < 300) {
		// var obj = JSON.parse(request.responseText);
		// document.getElementById("temperature").innerHTML = obj.temperature;
		// document.getElementById("kessel").innerHTML = obj.kessel;
		// document.getElementById("timestamp").innerHTML = obj.timestamp;
		data = JSON.parse(request.responseText);
		for (let entry of data) {
			console.log(entry);
			let label = document.createElement("label");
			label.innerHTML = entry.Name;
			let valueLabel = document.createElement("label");
			if (entry.Unit != "bool") valueLabel.innerHTML = entry.Value + entry.Unit;
			else if (entry.Value == 0) valueLabel.innerHTML = "true";
			else valueLabel.innerHTML = "false";
			valueLabel.id = entry.Name;
			valueLabel.classList.add("value_label");
			main_div.appendChild(label);
			main_div.appendChild(valueLabel);
		}

		console.log(request.responseText);
	} else {
		console.error(request.statusText, request.responseText);
	}
});
request.send();
