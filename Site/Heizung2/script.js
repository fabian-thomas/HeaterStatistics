String.prototype.toCompatibleDateTime = function() {
	var split = this.split(".");
	var timeSplit = split[2].split(" ");
	return `${timeSplit[0]}/${split[1]}/${split[0]} ${timeSplit[1]}`;
};

var main_div = document.getElementById("main_div");

var request = new XMLHttpRequest();
request.open("GET", "../DataProcessing/data.json");
request.addEventListener("load", function(event) {
	if (request.status >= 200 && request.status < 300) {
		data = JSON.parse(request.responseText);
		console.log(data);

		let TPo;
		let TVLs_1;
		let TB1;
		for (let entry of data) {
			if (entry.Name !== "timestamp") {
				if (entry.Name == "TPo" || entry.Name == "TVLs_1" || entry.Name == "TB1") {
					let label = document.createElement("label");
					label.innerHTML = entry.Name;
					let valueLabel = document.createElement("label");
					if (entry.Unit != "bool") valueLabel.innerHTML = entry.Value + entry.Unit;
					else if (entry.Value == 0) valueLabel.innerHTML = "true";
					else valueLabel.innerHTML = "false";
					valueLabel.classList.add("value_label");
					main_div.appendChild(label);
					main_div.appendChild(valueLabel);

					// values (TVLs, Tpo, TB) als integer in den oben deklrarierten Variablen abspeichern
					if (entry.Name == "TPo") TPo = parseInt(entry.Value);
					else if (entry.Name == "TB1") TB1 = parseInt(entry.Value);
					else if (entry.Name == "TVLs_1") TVLs_1 = parseInt(entry.Value);
				}
			} else {
				document.getElementById("timestamp-label").innerHTML = entry.Value;
				// Überprüfen ob die Daten älter als 2 Minuten sind
				if (new Date(entry.Value.toCompatibleDateTime()).getTime() + 2 * 60 * 1000 < Date.now())
					document.getElementById("timestamp-label").style.color = "red";
			}
		}

		console.log("TPo", TPo, "TB1", TB1, "TVLs_1", TVLs_1);

		// Farbregeln mit den abgespeicherten Werten überprüfen
		if (TB1 > 45 && TVLs_1 - 4 <= TPo) document.body.style.backgroundColor = "green";

		if ((45 < TB1 && 40 > TB1) != TVLs_1 - 4 > TPo)
			// != dient in diesem Fall als XOR
			document.body.style.backgroundColor = "yellow";

		if (TB1 < 40 || TVLs_1 - 8 > TPo || (45 < TB1 && 40 > TB1 && TVLs_1 - 4 > TPo))
			document.body.style.backgroundColor = "red";
	} else {
		console.error(request.statusText, request.responseText);
	}
});
request.send();

setInterval(_ => location.reload(), 30 * 1000); // refreshen der Website alle 30 Sekunden
