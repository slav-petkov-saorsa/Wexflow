window.onload = function () {
    "use strict";

    let updateLanguage = function (language) {
        document.getElementById("lnk-dashboard").innerHTML = language.get("lnk-dashboard");
        document.getElementById("lnk-manager").innerHTML = language.get("lnk-manager");
        document.getElementById("lnk-designer").innerHTML = language.get("lnk-designer");
        document.getElementById("lnk-history").innerHTML = language.get("lnk-history");
        document.getElementById("lnk-users").innerHTML = language.get("lnk-users");
        document.getElementById("lnk-profiles").innerHTML = language.get("lnk-profiles");
        document.getElementById("spn-logout").innerHTML = language.get("spn-logout");
    };

    let language = new Language("lang", updateLanguage);
    language.init();

    let uri = Common.trimEnd(Settings.Uri, "/");
    let lnkRecords = document.getElementById("lnk-records");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
    let lnkNotifications = document.getElementById("lnk-notifications");
    let imgNotifications = document.getElementById("img-notifications");
    let searchText = document.getElementById("search-records");
    let username = "";
    let userProfile = -1;
    let auth = "";

    let suser = getUser();

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        let user = JSON.parse(suser);

        username = user.Username;
        let password = user.Password;
        auth = "Basic " + btoa(username + ":" + password);

        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (user.Password !== u.Password) {
                    Common.redirectToLoginPage();
                } else {
                    if (u.UserProfile === 0 || u.UserProfile === 1) {
                        Common.get(uri + "/hasNotifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
                            lnkRecords.style.display = "inline";
                            lnkManager.style.display = "inline";
                            lnkDesigner.style.display = "inline";
                            lnkApproval.style.display = "inline";
                            lnkUsers.style.display = "inline";
                            lnkNotifications.style.display = "inline";

                            userProfile = u.UserProfile;
                            if (u.UserProfile === 0) {
                                lnkProfiles.style.display = "inline";
                            }

                            if (hasNotifications === true) {
                                imgNotifications.src = "images/notification-active.png";
                            } else {
                                imgNotifications.src = "images/notification.png";
                            }

                            let btnLogout = document.getElementById("btn-logout");
                            document.getElementById("navigation").style.display = "block";
                            document.getElementById("content").style.display = "block";

                            btnLogout.onclick = function () {
                                deleteUser();
                                Common.redirectToLoginPage();
                            };
                            document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                            searchText.onkeyup = function (event) {
                                event.preventDefault();

                                if (event.keyCode === 13) { // Enter
                                    loadRecords();
                                }

                                return false;
                            };

                            loadRecords();

                        }, function () { }, auth);
                    } else {
                        Common.redirectToLoginPage();
                    }

                }
            }, function () { }, auth);

        function loadRecords() {
            let loadRecordsTable = function (records) {
                let items = [];
                for (let i = 0; i < records.length; i++) {
                    let record = records[i];
                    items.push("<tr>"
                        + "<td class='check'><input type='checkbox'></td>"
                        + "<td class='id'>" + record.Id + "</td>"
                        + "<td class='name'>" + record.Name + "</td>"
                        + "<td class='approved'>" + "<input type='checkbox' " + (record.Approved === true ? "checked" : "") + " disabled>" + "</td>"
                        + "<td class='start-date'>" + (record.StartDate === "" ? "-" : record.StartDate) + "</td>"
                        + "<td class='end-date'>" + (record.EndDate === "" ? "-" : record.EndDate) + "</td>"
                        + "<td class='assigned-to'>" + (record.AssignedTo === "" ? "-" : record.AssignedTo) + "</td>"
                        + "<td class='assigned-on'>" + (record.AssignedOn === "" ? "-" : record.AssignedOn) + "</td>"
                        + "</tr>");

                }

                let table = "<table id='records-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th class='check'><input id='check-all' type='checkbox'></th>"
                    + "<th class='id'></th>"
                    + "<th id='th-assigned-by' class='name'>" + "Name" + "</th>"
                    + "<th id='th-assigned-on' class='approved'>" + "Approved" + "</th>"
                    + "<th id='th-message' class='start-date'>" + "Start date" + "</th>"
                    + "<th id='th-message' class='end-date'>" + "End date" + "</th>"
                    + "<th id='th-message' class='assigned-to'>" + "Assigned to" + "</th>"
                    + "<th id='th-message' class='assigned-on'>" + "Assigned on" + "</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                let divNotifications = document.getElementById("content");
                divNotifications.innerHTML = table;

                let recordsTable = document.getElementById("records-table");
                let rows = recordsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                let recordIds = [];
                for (let i = 0; i < rows.length; i++) {
                    let row = rows[i];
                    let checkBox = row.getElementsByClassName("check")[0].firstChild;
                    checkBox.onchange = function () {
                        let currentRow = this.parentElement.parentElement;
                        let recordId = currentRow.getElementsByClassName("id")[0].innerHTML;
                        if (this.checked === true) {
                            recordIds.push(recordId);
                        } else {
                            recordIds = Common.removeItemOnce(recordIds, recordId);
                        }
                    };
                }

                document.getElementById("check-all").onchange = function () {
                    for (let i = 0; i < rows.length; i++) {
                        let row = rows[i];
                        let checkBox = row.getElementsByClassName("check")[0].firstChild;
                        let recordId = row.getElementsByClassName("id")[0].innerHTML;

                        if (checkBox.checked === true) {
                            checkBox.checked = false;
                            recordIds = Common.removeItemOnce(recordIds, recordId);
                        } else {
                            checkBox.checked = true;
                            recordIds.push(recordId);
                        }
                    }
                };

                document.getElementById("btn-delete").onclick = function () {
                    if (recordIds.length === 0) {
                        Common.toastInfo("Select records to delete.");
                    } else {
                        let cres = confirm("Are you sure you want to delete " + (recordIds.length == 1 ? "this" : "these") + " record" + (recordIds.length == 1 ? "" : "s") + "?");
                        if (cres === true) {
                            Common.post(uri + "/deleteRecords", function (res) {
                                if (res === true) {
                                    for (let i = recordIds.length - 1; i >= 0; i--) {
                                        let recordId = recordIds[i];
                                        for (let i = 0; i < rows.length; i++) {
                                            let row = rows[i];
                                            let id = row.getElementsByClassName("id")[0].innerHTML;
                                            if (recordId === id) {
                                                recordIds = Common.removeItemOnce(recordIds, recordId);
                                                row.remove();
                                            }
                                        }
                                    }
                                }
                            }, function () { }, recordIds, auth);
                        }
                    }
                };

            };

            if (userProfile === 0) {
                Common.get(uri + "/searchRecords?s=" + encodeURIComponent(searchText.value), function (records) {
                    loadRecordsTable(records);
                }, function () { }, auth);
            } else if (userProfile === 1) {
                Common.get(uri + "/searchRecordsCreatedByOrAssignedTo?s=" + encodeURIComponent(searchText.value), function (records) {
                    loadRecordsTable(records);
                }, function () { }, auth);
            }
        }
    }

};