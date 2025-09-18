const lunchItems = document.querySelectorAll(".lunch-list > .item");
const dinnerItems = document.querySelectorAll(".dinner-list > .item");
const title = document.getElementById("title");
const previousMonthButton = document.getElementById("previous-month-button");
const nextMonthButton = document.getElementById("next-month-button");

let dataItems = {};
let selectedMonth = new Date().getMonth();
const monthNames = [
    "Januari",
    "Februari",
    "Mars",
    "April",
    "Maj",
    "Juni",
    "Juli",
    "Augusti",
    "September",
    "Oktober",
    "November",
    "December",
];

function getFirstFourWeeksOfMonth(year, month) {
    const weeks = new Set();
    const date = new Date(year, month, 1);
    while (weeks.size < 4 && date.getMonth() === month) {
        const plainDate = Temporal.PlainDate.from({
            year: date.getFullYear(),
            month: date.getMonth() + 1,
            day: date.getDate()
        });
        weeks.add(plainDate.weekOfYear);
        date.setDate(date.getDate() + 7);
    }

    return [...weeks];
}

function fitText(text, targetHeight) {
    let fontSize = 20;
    text.style.fontSize = fontSize + "px";

    while (text.offsetHeight > targetHeight && fontSize > 12) {
        fontSize -= 0.5;
        text.style.fontSize = fontSize + "px";
    }
}

function getNextMonthNumber() {
    return selectedMonth == 11 ? 0 : selectedMonth + 1;
}

function getPreviousMonthNumber() {
    return selectedMonth == 0 ? 11 : selectedMonth - 1;
}

function reload() {
    const now = new Date();

    title.textContent = monthNames[selectedMonth];
    previousMonthButton.textContent = "← " + monthNames[getPreviousMonthNumber()];
    nextMonthButton.textContent = monthNames[getNextMonthNumber()] + " →";

    const weeks = getFirstFourWeeksOfMonth(now.getFullYear(), selectedMonth);
    const weekNow = Temporal.Now.plainDateISO().weekOfYear;
    const targetHeight = lunchItems[0].querySelector(".name").offsetHeight;
    for (let i = 0; i < 4; i++) {
        const week = weeks[i];
        const dataItem = dataItems[week];
        const weekText = week == weekNow ? "Nu" : `v${week}`;

        const lunchNameElement = lunchItems[i].querySelector(".name");
        lunchItems[i].querySelector(".week").textContent = weekText;
        lunchNameElement.textContent = `${dataItem.lunch}`;
        fitText(lunchNameElement, targetHeight);

        const dinnerNameElement = dinnerItems[i].querySelector(".name");
        dinnerItems[i].querySelector(".week").textContent = weekText;
        dinnerNameElement.textContent = `${dataItem.dinner}`;
        fitText(dinnerNameElement, targetHeight);
    }

    for (let i = 0; i < 4; i++) {
        const lunchNameElement = lunchItems[i].querySelector(".name");
        fitText(lunchNameElement, targetHeight);

        const dinnerNameElement = dinnerItems[i].querySelector(".name");
        fitText(dinnerNameElement, targetHeight);
    }
}

fetch("items.json").then(async response => {
    dataItems = await response.json();
    reload();
});

previousMonthButton.addEventListener("click", () => {
    selectedMonth = getPreviousMonthNumber();
    reload();
});

nextMonthButton.addEventListener("click", () => {
    selectedMonth = getNextMonthNumber();
    reload();
});
