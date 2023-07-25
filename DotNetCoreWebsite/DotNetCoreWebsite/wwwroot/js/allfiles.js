var isSet = false;

function onMainCheckboxClick() {
    let allCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]");

    isSet = !isSet;
    for (let i = 0; i < allCheckBoxes.length; i++) {
        allCheckBoxes[i].checked = isSet;
    }

    setDownloadButtonVisibility();
    onCheckboxClick();
}

function onCheckboxClick() {
    document.querySelector('#downloadSelectedButton').href = getDownloadSelectedUrl();
}

function getDownloadSelectedUrl() {
    let path = new URL(window.location.toString()).searchParams.get("q");
    let fileNames = [];
    let allSelectedCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]:checked");

    for (let i = 0; i < allSelectedCheckBoxes.length; i++) {
        let link = allSelectedCheckBoxes[i].parentElement.parentElement.querySelector(".downloadLink a").href;
        let fileName = new URL(link).searchParams.get("q");

        if (path) {
            fileName = fileName.replace(path + "/", "");
        }

        fileNames.push(fileName);
    }

    let params = fileNames.map(x => "q=" + encodeURIComponent(x));

    if (path)
        params.push("path=" + encodeURIComponent(path));

    return "DownloadMultiFile?" + params.join("&");
}

function getDownloadLinks() {
    let linksArray = Array.from(document.querySelectorAll(".downloadCheckBoxInput:checked")).map(x => x.closest("tr").querySelector(".downloadLink a").href);
    let linksJoined = linksArray.join("<br />");
    document.getElementById("linksModalContent").innerHTML = linksJoined;
    makeModal("#linksModal");
}

function setDownloadButtonVisibility() {
    let checkedCount = document.querySelectorAll('.downloadCheckBoxInput:checked').length;
    setVisible('#downloadSelectedButton', checkedCount > 0);
    setVisible('#getDownloadLinksButton', checkedCount > 0);
}

function toLocalDate(utcDateString) {
    let date = utcDateString.replace(" UTC", "").replace(" ", "T") + ":00.000Z";
    return formatDate(new Date(date));
}

function formatDate(d) {
    let month = '' + (d.getMonth() + 1);
    let day = '' + d.getDate();
    let year = d.getFullYear();

    let hours = d.getHours();
    let minutes = d.getMinutes();

    return `${year}-${pad2Left(month)}-${pad2Left(day)} ${pad2Left(hours)}:${pad2Left(minutes)}`;
}

function pad2Left(n) {
    return n.toString().padStart(2, "0");
}

function convertDatesToLocal() {
    let allDates = document.querySelectorAll(".utcdate");
    for (x of allDates) {
        x.innerHTML = toLocalDate(x.innerHTML);
    }
}

async function getFolderSizes() {
    let response = await fetch("/Home/GetFolderSize" + document.location.search);
    if (!response.ok)
        throw new Error();
    let data = await response.json();
    let rows = document.querySelectorAll("table tbody tr.xfolder");
    for (let row of rows) {
        let tds = row.querySelectorAll("td");
        let folderName = tds[1].querySelector("a").innerText;
        let sizeInfo = data[folderName];
        tds[2].innerText = sizeInfo.fileSizeString;
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

document.addEventListener("DOMContentLoaded", function (event) {
    convertDatesToLocal();

    addOnClick('.downloadCheckBoxInput', function (event) {
        setDownloadButtonVisibility();
    })

    addOnClick('#selectAll', function (event) {
        selectText('linksModalContent');
    });

    getFolderSizes();

    allowTableSorting();
});

function setVisible(selector, isVisible) {
    let elems = document.querySelectorAll(selector);
    for (let elem of elems) {
        elem.style.visibility = isVisible ? "visible" : "hidden";
    }
}

function addOnClick(selector, func) {
    let elems = document.querySelectorAll(selector);
    for (let elem of elems) {
        elem.addEventListener("click", func, false);
    }
}

function allowTableSorting() {
    // Allow table sorting
    addOnClick('th', function () {
        let table = this.closest("table");
        let tbody = table.querySelector("tbody");
        let trs = tbody.querySelectorAll('tr');
        let comparer = getComparer(whichIndex(this));
        let rows = Array.from(trs).sort(comparer);
        this.asc = !this.asc;
        if (!this.asc) { rows = rows.reverse() }
        for (let i = 0; i < rows.length; i++) {
            tbody.appendChild(rows[i]);
        }
    });
}

function getComparer(index) {
    return function (a, b) {
        let valA = getCellValue(a, index), valB = getCellValue(b, index)
        return isNumeric(valA) && isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB)
    }
}

function getCellValue(row, index) {
    let elem = row.querySelectorAll("td")[index];
    let dataValue = elem.dataset["sortValue"];
    if (dataValue) return dataValue;
    return elem.innerText;
}

function isNumeric(text) {
    return !isNaN(new Number(text));
}

function whichIndex(elem) {
    let children = elem.parentElement.children;
    for (let i = 0; i < children.length; i++)
        if (children[i] == elem)
            return i;
    return -1;
}

function makeModal(selector) {
    var modal = document.querySelector(selector);
    var container = modal.querySelector(".modal-container");

    modal.classList.remove("modal-hidden");

    modal.addEventListener("click", function (e) {
        if (e.target !== modal && e.target !== container) return;
        modal.classList.add("modal-hidden");
    });
}