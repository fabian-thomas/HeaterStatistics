var main_div = document.getElementById("main_div");

var request = new XMLHttpRequest();
request.open("GET", "../DataProcessing/data.json");
request.addEventListener("load", function(event) {
	if (request.status >= 200 && request.status < 300) {
		data = JSON.parse(request.responseText);
		console.log(data);
		for (let entry of data) {
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
	} else {
		console.error(request.statusText, request.responseText);
	}
});
request.send();
