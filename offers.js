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
        const price = offer.unitPrice.toString().replace(".", ",");
        storePane.insertAdjacentHTML("beforeend", `
            <div class="offer">
                <h4>${offer.name}</h4>
                <span class="price">${price} kr/${prettifyBaseUnit(offer.baseUnit)}</span>
                <p>${offer.description}</p>
            </div>
        `);
    }
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
