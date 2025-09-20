const lunchItems = document.querySelectorAll(".lunch-list > .item");
const dinnerItems = document.querySelectorAll(".dinner-list > .item");
const title = document.getElementById("title");
const previousMonthButton = document.getElementById("previous-month-button");
const nextMonthButton = document.getElementById("next-month-button");
const weekNow = Temporal.Now.plainDateISO().weekOfYear;

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

function getFirstNWeeksOfMonth(year, month, count) {
    const weeks = new Set();
    const date = new Date(year, month, 1);
    const offsetToFirstMonday = (8 - date.getDay()) % 7;
    date.setDate(date.getDate() + offsetToFirstMonday);

    while (weeks.size < count && date.getMonth() === month) {
        const plainDate = Temporal.PlainDate.from({
            year: date.getFullYear(),
            month: date.getMonth() + 1,
            day: date.getDate(),
        });

        weeks.add(plainDate.weekOfYear);
        date.setDate(date.getDate() + 7);
    }

    console.log(weeks)

    return [...weeks];
}

function fitText(text, targetHeight) {
    let fontSize = 20;
    text.style.fontSize = fontSize + "px";

    const minFontSize = window.screen.width < 600 ? 8 : 12;
    while (text.offsetHeight > targetHeight && fontSize > minFontSize) {
        fontSize -= 0.5;
        text.style.fontSize = fontSize + "px";
    }
}

function fitAllText() {
    const targetHeight = lunchItems[0].querySelector(".name").offsetHeight;
    for (let i = 0; i < lunchItems.length; i++) {
        const lunchNameElement = lunchItems[i].querySelector(".name");
        fitText(lunchNameElement, targetHeight);

        const dinnerNameElement = dinnerItems[i].querySelector(".name");
        fitText(dinnerNameElement, targetHeight);
    }
}

function getNextMonthNumber() {
    return selectedMonth == 11 ? 0 : selectedMonth + 1;
}

function getPreviousMonthNumber() {
    return selectedMonth == 0 ? 11 : selectedMonth - 1;
}

function clearPopups() {
    for (const popup of document.querySelectorAll(".popup")) {
        popup.classList.add("hidden");
    }
}

function showPopup(week, data, listElement) {
    clearPopups();

    const popup = listElement.querySelector(".popup");
    popup.classList.remove("hidden");

    const weekElement = popup.querySelector(".week");
    if (week == weekNow) {
        weekElement.textContent = "Nu";
        weekElement.classList.add("now");
    } else {
        weekElement.textContent = `v${week}`;
        weekElement.classList.remove("now");
    }

    popup.querySelector(".popup-header").textContent = data.name;
    popup.querySelector("p").innerHTML = data.notes;
}

const getWeeksInMonth = (year, month) => {
    const date = Temporal.PlainDate.from({
        year,
        month: month + 1,
        day: 1
    });

    return date.daysInMonth == 31 ? 5 : 4;
};

function reload() {
    clearPopups();

    const now = new Date();
    title.textContent = monthNames[selectedMonth];
    previousMonthButton.textContent = "← " + monthNames[getPreviousMonthNumber()];
    nextMonthButton.textContent = monthNames[getNextMonthNumber()] + " →";

    const weeks = getFirstNWeeksOfMonth(now.getFullYear(), selectedMonth, 5);
    const targetHeight = lunchItems[0].querySelector(".name-container").offsetHeight;
    const emptyItem = {
        dinner: {
            name: "-",
            notes: "-",
        },
        lunch: {
            name: "-",
            notes: "-",
        },
    };

    for (let i = 0; i < lunchItems.length; i++) {
        const week = weeks[i];
        const isEmptyWeek = i == weeks.length;
        if (isEmptyWeek) {
            lunchItems[i].style.visibility = "hidden";
            dinnerItems[i].style.visibility = "hidden";
            continue;
        }

        lunchItems[i].style.visibility = "";
        dinnerItems[i].style.visibility = "";

        const dataItem = dataItems[week];

        const lunchNameElement = lunchItems[i].querySelector(".name");
        lunchNameElement.textContent = `${dataItem.lunch.name}`;
        fitText(lunchNameElement, targetHeight);
        lunchNameElement.onclick = () => {
            return showPopup(week, dataItem.lunch, lunchItems[i].parentElement);
        }

        const dinnerNameElement = dinnerItems[i].querySelector(".name");
        dinnerNameElement.textContent = `${dataItem.dinner.name}`;
        fitText(dinnerNameElement, targetHeight);
        dinnerNameElement.onclick = () => {
            return showPopup(week, dataItem.dinner, dinnerItems[i].parentElement);
        }

        const lunchWeekElement = lunchItems[i].querySelector(".week");
        const dinnerWeekElement = dinnerItems[i].querySelector(".week");
        if (week == weekNow) {
            lunchWeekElement.textContent = "Nu";
            dinnerWeekElement.textContent = "Nu";

            lunchWeekElement.classList.add("now");
            dinnerWeekElement.classList.add("now");
        } else {
            lunchWeekElement.textContent = `v${week}`;
            dinnerWeekElement.textContent = `v${week}`;

            lunchWeekElement.classList.remove("now");
            dinnerWeekElement.classList.remove("now");
        }
    }

    fitAllText();
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

for (const popupBackButton of document.querySelectorAll(".popup-back-button")) {
    popupBackButton.addEventListener("click", clearPopups);
}

window.addEventListener("resize", fitAllText);
