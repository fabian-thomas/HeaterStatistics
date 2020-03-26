String.prototype.toCompatibleDateTime = function () {
	var split = this.split(".");
	var timeSplit = split[2].split(" ");
	return `${timeSplit[0]}/${split[1]}/${split[0]} ${timeSplit[1]}`;
};

var main_div = document.getElementById("main_div");

var request = new XMLHttpRequest();
request.open("GET", "../DataProcessing/data.json");
request.addEventListener("load", function (event) {
	if (request.status >= 200 && request.status < 300) {
		data = JSON.parse(request.responseText);
		console.log(data);
		for (let entry of data) {
			if (entry.Name !== "timestamp") {
				if (entry.Name == "TK" || entry.Name == "TRG") {
					let label = document.createElement("label");
					label.innerHTML = entry.Name;
					let valueLabel = document.createElement("label");
					if (entry.Unit != "bool") valueLabel.innerHTML = entry.Value + entry.Unit;
					else if (entry.Value == 0) valueLabel.innerHTML = "true";
					else valueLabel.innerHTML = "false";
					valueLabel.classList.add("value_label");
					main_div.appendChild(label);
					main_div.appendChild(valueLabel);
				}
			} else {
				document.getElementById("timestamp-label").innerHTML = entry.Value;
				// Überprüfen ob die Daten älter als 2 Minuten sind
				console.log(
					new Date(entry.Value.toCompatibleDateTime()).getTime(),
					new Date(entry.Value.toCompatibleDateTime()).getTime() + 2 * 60 * 1000,
					Date.now(),
					new Date(entry.Value.toCompatibleDateTime()).getTime() + 2 * 60 * 1000 < Date.now()
				);
				if (new Date(entry.Value.toCompatibleDateTime()).getTime() + 2 * 60 * 1000 < Date.now())
					document.getElementById("timestamp-label").style.color = "red";
			}
		}
	} else {
		console.error(request.statusText, request.responseText);
	}
});
request.send();

setInterval(_ => location.reload(), 30 * 1000); // refreshen der Website alle 30 Sekunden
