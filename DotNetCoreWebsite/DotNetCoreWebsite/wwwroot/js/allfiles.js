var isSet = false;

function checkboxChange() {
    var allCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]");

    isSet = !isSet;
    for (var i = 0; i < allCheckBoxes.length; i++) {
        allCheckBoxes[i].checked = isSet;
    }

    setDownloadButtonVisibility();
}

function downloadSelected() {
    var path = new URL(window.location.toString()).searchParams.get("q");
    var fileNames = [];
    var allSelectedCheckBoxes = document.querySelectorAll(".downloadCheckBox input[type=checkbox]:checked");

    for (var i = 0; i < allSelectedCheckBoxes.length; i++) {
        var link = allSelectedCheckBoxes[i].parentElement.parentElement.querySelector(".downloadLink a").href;
        var fileName = new URL(link).searchParams.get("q");

        if (path) {
            fileName = fileName.replace(path + "/", "");
        }

        fileNames.push(fileName);
    }

    var params = fileNames.map(x => "q=" + encodeURIComponent(x));

    if (path)
        params.push("path=" + encodeURIComponent(path));

    window.location = "DownloadMultiFile?" + params.join("&");
}

function getDownloadLinks() {
    var linksArray = Array.from(document.querySelectorAll(".downloadCheckBoxInput:checked")).map(x => x.closest("tr").querySelector(".downloadLink a").href);
    alert(linksArray.join("\n"));
}

function setDownloadButtonVisibility() {
    $('#downloadSelectedButton').setVisible($('.downloadCheckBoxInput:checked').length > 0);
    $('#getDownloadLinksButton').setVisible($('.downloadCheckBoxInput:checked').length > 0);
}

function toLocalDate(utcDateString) {
    var date = utcDateString.replace(" UTC", "").replace(" ", "T") + ":00.000Z";
    return formatDate(new Date(date));
}

function formatDate(d) {
    var month = '' + (d.getMonth() + 1);
    var day = '' + d.getDate();
    var year = d.getFullYear();

    var hours = d.getHours();
    var minutes = d.getMinutes();

    return `${year}-${pad2Left(month)}-${pad2Left(day)} ${pad2Left(hours)}:${pad2Left(minutes)}`;
}

function pad2Left(n) {
    return n.toString().padStart(2, "0");
}

function convertDatesToLocal() {
    var allDates = document.querySelectorAll(".utcdate");
    for (x of allDates) {
        x.innerHTML = toLocalDate(x.innerHTML);
    }
}

function httpcall(url, onsuccess) {
    var xmlhttp = new XMLHttpRequest();

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == XMLHttpRequest.DONE) {
            if (xmlhttp.status == 200) {
                var result = xmlhttp.responseText;
                try {
                    onsuccess(result);
                }
                catch (err) {
                    console.log(err);
                }
            }
            else if (xmlhttp.status == 400) {
                console.log('There was an error 400');
            }
            else {
                console.log(`unexpected status code: ${xmlhttp.status}`);
            }
        }
    };

    xmlhttp.open("GET", url, true);

    xmlhttp.send();
}

function getFolderSizes() {
    httpcall("/Home/GetFolderSize" + document.location.search, function (dataStr) {
        var data = JSON.parse(dataStr);

        var rows = document.querySelectorAll("table tbody tr.xfolder");
        for (var row of rows) {
            var tds = row.querySelectorAll("td");
            var folderName = tds[1].querySelector("a").innerText;
            var sizeInfo = data[folderName];
            tds[2].innerText = sizeInfo.fileSizeString;
            tds[2].dataset["sortValue"] = sizeInfo.fileSize;
        }
    });
}

$(document).ready(function () {
    convertDatesToLocal();

    $('.downloadCheckBoxInput').on("click", function (event) {
        setDownloadButtonVisibility();
    })

    getFolderSizes();
});

// Allow table sorting
$('th').click(function () {
    var table = $(this).parents('table').eq(0)
    var rows = table.find('tr:gt(1)').toArray().sort(comparer($(this).index()))
    this.asc = !this.asc
    if (!this.asc) { rows = rows.reverse() }
    for (var i = 0; i < rows.length; i++) { table.append(rows[i]) }
})
function comparer(index) {
    return function (a, b) {
        var valA = getCellValue(a, index), valB = getCellValue(b, index)
        return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB)
    }
}
function getCellValue(row, index) {
    $elem = $(row).children('td').eq(index);
    var dataValue = $elem[0].dataset["sortValue"];
    if (dataValue) return dataValue;
    return $elem.text()
}
