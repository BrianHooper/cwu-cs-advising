// Global icons
var deleteIcon = "&#10060;";
var checkIcon = "&#10004;";
var editIcon = "&#9998;";

function RemoveStringFromArray(Array, Str) {
    var NewArray = [];
    for (var i = 0; i < Array.length; i++) {
        if (!StringMatch(Array[i], Str)) {
            NewArray.push(Array[i]);
        }
    }
    return NewArray;
}

/*  Returns true if the array contains an element
    matching the given string */
function StringArrayContains(Array, Str) {
    if (Array === false || Array === undefined) {
        return false;
    }
    for (var i = 0; i < Array.length; i++) {
        if (Array[i].indexOf(Str) === 0) {
            return true;
        }
    }
    return false;
}

/*  Returns true if strA is equal to strB
    not safe for Internet Explorer but who cares    */
function StringMatch(strA, strB) {
    return strA.indexOf(strB) >= 0;
}

/*  Checks if an HTML object has a given attribute  */
function HasAttribute(Object, Attribute) {
    return Object.attr(Attribute) !== undefined;
}

/*  Checks if an HTML object has an attribute, and
    that the attribute matches a value  */
function HasAttrValue(Object, Attribute, Value) {
    if (!HasAttribute(Object, Attribute)) {
        return false;
    } else {
        return StringMatch(Object.attr(Attribute), Value);
    }
}

$(document).on("click", "#ChangePasswordLink", function () {
    var ChangePwUser = $("#CurrentUser").text();
    // Pass ajax to server
    $.ajax({
        type: "POST",
        url: "/UserManagement?handler=ChangePassword",
        data: ChangePwUser,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response) {
                window.location.href = "ChangePassword";
            } else {
                alert("Failed to retrieve user");
                return false;
            }
        },
        failure: function (response) {
            alert("Failed to retrieve user");
            return false;
        }
    });
});