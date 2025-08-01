var isSet = false;

function onMainCheckboxClick() {
    const allCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]");

    isSet = !isSet;
    for (const box of allCheckBoxes) {
        box.checked = isSet;
    }

    setDownloadButtonVisibility();
    onCheckboxClick();
}

function onCheckboxClick() {
    document.querySelector('#downloadSelectedButton').href = getDownloadSelectedUrl();
}

function getDownloadSelectedUrl() {
    const path = new URL(window.location.toString()).searchParams.get("q");
    const fileNames = [];
    const allSelectedCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]:checked");

    for (const box of allSelectedCheckBoxes) {
        const link = box.parentElement.parentElement.querySelector(".downloadLink a").href;
        let fileName = new URL(link).searchParams.get("q");

        if (path) {
            fileName = fileName.replace(path + "/", "");
        }

        fileNames.push(fileName);
    }

    const params = fileNames.map(x => "q=" + encodeURIComponent(x));

    if (path)
        params.push("path=" + encodeURIComponent(path));

    return "DownloadMultiFile?" + params.join("&");
}

function getDownloadLinks() {
    const linksArray = Array.from(document.querySelectorAll(".downloadCheckBoxInput:checked")).map(x => x.closest("tr").querySelector(".downloadLink a").href);
    const linksJoined = linksArray.join("<br />");
    document.getElementById("linksModalContent").innerHTML = linksJoined;
    makeModal("#linksModal");
}

function setDownloadButtonVisibility() {
    const checkedCount = document.querySelectorAll('.downloadCheckBoxInput:checked').length;
    setVisible('#downloadSelectedButton', checkedCount > 0);
    setVisible('#getDownloadLinksButton', checkedCount > 0);
}

function toLocalDate(utcDateString) {
    const date = utcDateString.replace(" UTC", "").replace(" ", "T") + ":00.000Z";
    return formatDate(new Date(date));
}

function formatDate(d) {
    const month = '' + (d.getMonth() + 1);
    const day = '' + d.getDate();
    const year = d.getFullYear();

    const hours = d.getHours();
    const minutes = d.getMinutes();

    return `${year}-${pad2Left(month)}-${pad2Left(day)} ${pad2Left(hours)}:${pad2Left(minutes)}`;
}

function pad2Left(n) {
    return n.toString().padStart(2, "0");
}

function convertDatesToLocal() {
    const allDates = document.querySelectorAll(".utcdate");
    for (const x of allDates) {
        x.innerHTML = toLocalDate(x.innerHTML);
    }
}

async function getFolderSizes() {
    const response = await fetch("/Home/GetFolderSize" + document.location.search);
    if (!response.ok)
        throw new Error();
    const data = await response.json();
    const rows = document.querySelectorAll("table tbody tr.xfolder");
    for (const row of rows) {
        const tds = row.querySelectorAll("td");
        const folderName = tds[1].querySelector("a").textContent;
        const sizeInfo = data[folderName];
        tds[2].textContent = sizeInfo.fileSizeString;
        tds[2].dataset["sortValue"] = sizeInfo.fileSize;
    }
}

function selectText(elementId) {
    let doc = document, text = doc.getElementById(elementId), range, selection;
    if (doc.body.createTextRange) {
        range = document.body.createTextRange();
        range.moveToElementText(text);
        range.select();
    } else if (window.getSelection) {
        selection = window.getSelection();
        range = document.createRange();
        range.selectNodeContents(text);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

function setVisible(selector, isVisible) {
    const elems = document.querySelectorAll(selector);
    for (const elem of elems) {
        elem.style.visibility = isVisible ? "visible" : "hidden";
    }
}

function addOnClick(selector, func) {
    const elems = document.querySelectorAll(selector);
    for (const elem of elems) {
        elem.addEventListener("click", func, false);
    }
}

function allowTableSorting() {
    // Allow table sorting
    addOnClick('th', function () {
        const table = this.closest("table");
        const tbody = table.querySelector("tbody");
        const trs = tbody.querySelectorAll('tr');
        const comparer = getComparer(whichIndex(this));
        let rows = Array.from(trs).sort(comparer);
        this.asc = !this.asc;
        if (!this.asc) {
            rows = rows.reverse();
        }
        for (const row of rows) {
            tbody.appendChild(row);
        }
    });
}

function getComparer(index) {
    return function (a, b) {
        const valA = getCellValue(a, index), valB = getCellValue(b, index)
        return isNumeric(valA) && isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB)
    }
}

function getCellValue(row, index) {
    const elem = row.querySelectorAll("td")[index];
    const dataValue = elem.dataset["sortValue"];
    if (dataValue)
        return dataValue;
    return elem.textContent;
}

function isNumeric(text) {
    return !isNaN(new Number(text));
}

function whichIndex(elem) {
    const children = elem.parentElement.children;
    for (let i = 0; i < children.length; i++)
        if (children[i] == elem)
            return i;
    return -1;
}

function makeModal(selector) {
    const modal = document.querySelector(selector);
    const container = modal.querySelector(".modal-container");

    modal.classList.remove("modal-hidden");

    modal.addEventListener("click", (e) => {
        if (e.target !== modal && e.target !== container) return;
        modal.classList.add("modal-hidden");
    });
}

function runOnLoad(f){
    addEventListener("load", f);
}

runOnLoad(() => {
    convertDatesToLocal();
});

runOnLoad(() => {
    addOnClick('.downloadCheckBoxInput', () => {
        setDownloadButtonVisibility();
    });
});

runOnLoad(() => {
    addOnClick('#selectAll', () => {
        selectText('linksModalContent');
    });
});

runOnLoad(() => {
    getFolderSizes();
});

runOnLoad(() => {
    allowTableSorting();
});