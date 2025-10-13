const offersElement = document.getElementById("offers");

fetch("/offers/latest.json").then(async response => {
    const offersByStore = await response.json();
    for (const store in offersByStore) {
        const offers = offersByStore[store];
        if (offers.length == 0) {
            continue;
        }

        buildOfferList(store, offers);
    }
});

function buildOfferList(store, offers) {
    const storePane = document.createElement("div");
    storePane.className = "store";
    offersElement.appendChild(storePane);

    const header = document.createElement("h3");
    header.className = "store-name";
    header.textContent = store.replace("-", " ");
    storePane.appendChild(header);

    for (const offer of offers) {
        const weekTag = offer.relevantForWeek
            ? `<span class="week-tag">v${offer.relevantForWeek}</span>`
            : "";

        storePane.insertAdjacentHTML("beforeend", `
            <div class="offer">
                <div class="name">
                    <h4>${offer.name}</h4>
                    ${weekTag}
                </div>
                <div class="content">
                    <div class="text">
                        <span class="price">${prettifyPrice(offer.unitPrice)} kr/${prettifyBaseUnit(offer.baseUnit)}</span>
                        <p>${offer.description}</p>
                    </div>
                    <a class="image-link" href="${offer.publicationUrl}" target="_blank"><img src="${offer.imageUrl}"></a>
                </div>
            </div>
        `);
    }
}

function prettifyPrice(price) {
    price = Math.round(price * 100) / 100;
    price = price.toString().replace(".", ",");

    const parts = price.split(",");
    if (parts.length == 2 && parts[1].length == 1) {
        return price + "0";
    }

    return price;
}

function prettifyBaseUnit(baseUnit) {
    if (baseUnit == "kilogram")
        return "kg";

    if (baseUnit == "gram")
        return "g";

    if (baseUnit == "liter")
        return "l";

    if (baseUnit == "milliliter")
        return "ml";

    return baseUnit;
}
