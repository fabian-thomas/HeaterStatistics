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

		let tpo;
		let tvls = [];
		let tb;
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
				}

				// values (TVLs, Tpo, TB) als integer in den oben deklrarierten Variablen abspeichern
				if (entry.Name == "TPo") tpo = parseInt(entry.Value);
				else if (entry.Name == "TB1") tb = parseInt(entry.Value);
				else if (entry.Name.startsWith("TVLs_")) tvls.push(parseInt(entry.Value)); // tvls 1-8 als Array abspeichern; Vorsicht: array beginnt bei 0 -> tvls[0] == TVLs_1
			} else {
				document.getElementById("timestamp-label").innerHTML = entry.Value;
				// Überprüfen ob die Daten älter als 2 Minuten sind
				if (new Date(entry.Value.toCompatibleDateTime()).getTime() + 2 * 60 * 1000 < Date.now())
					document.getElementById("timestamp-label").style.color = "red";
			}
		}

		console.log("tpo", tpo, "tb", tb, "tvls", tvls);

		// Farbregeln mit den abgespeicherten Werten überprüfen
		if ((45 < tb && 40 > tb) != (tvls[0] > tpo && tvls[1] > tpo && tvls[2] > tpo && tvls[3] > tpo))
			// != dient in diesem Fall als XOR
			document.body.style.backgroundColor = "yellow";

		if (
			tb < 40 ||
			(tvls[0] > tpo &&
				tvls[1] > tpo &&
				tvls[2] > tpo &&
				tvls[3] > tpo &&
				tvls[4] > tpo &&
				tvls[5] > tpo &&
				tvls[6] > tpo) ||
			(45 < tb && 40 > tb && tvls[0] > tpo && tvls[1] > tpo && tvls[2] > tpo && tvls[3] > tpo)
		)
			document.body.style.backgroundColor = "red";
	} else {
		console.error(request.statusText, request.responseText);
	}
});
request.send();

setInterval(_ => location.reload(), 30 * 1000); // refreshen der Website alle 30 Sekunden
