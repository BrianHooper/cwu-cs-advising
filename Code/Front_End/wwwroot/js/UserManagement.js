// Document ready
$(document).ready(function () {
    // Load user list from server
    CreateUserList();
});


// On Save users click
$(document).on("click", "#SaveUsers", function () {
    // Define user object and user object list
    var User = { Username: "", Admin: true, ResetPassword: false, isActive : true };
    var ModifiedUsers = [];

    // Read each row in the user container, add to userlist if modified
    for (var i = 0; i < $("#UserContainer").children().length; i++) {
        var UserRow = $("#UserContainer").children().eq(i);
        if (HasAttrValue(UserRow, "modified", "true")) {
            User.Username = UserRow.children().eq(1).text();
            User.Admin = HasAttrValue(UserRow.children().eq(2), "toggled", "true");
            User.ResetPassword = HasAttrValue(UserRow.children().eq(3), "toggled", "true");
            User.isActive = HasAttrValue(UserRow.children().eq(4), "toggled", "true");
            ModifiedUsers.push(User);
        }
    }

    // Pass ajax to server
    $.ajax({
        type: "POST",
        url: "/UserManagement?handler=SendUsers",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: JSON.stringify(ModifiedUsers),

        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            alert(response);
            ResetModifiers();
        },
        failure: function (response) {
            alert(response);
        }
    });
});

// Create user list
function CreateUserList() {
    $("#UserContainer").html("");
    for (var i = 0; i < UserList.length; i++) {
        $("#UserContainer").append(CreateUserRow(UserList[i]));
    }
    
}


// Create user row
function CreateUserRow(User) {
    var Row = $("<div></div>");
    Row.attr("class", "Flex CenterFlex Row");
    Row.attr("modified", "false");

    var DeleteColumn = $("<div>" + deleteIcon + "</div>");
    DeleteColumn.attr("class", "Element DeleteRow");
    Row.append(DeleteColumn);

    var UsernameColumn = $("<div>" + User.Username + "</div>");
    UsernameColumn.attr("class", "Element CourseElement");
    Row.append(UsernameColumn);

    var AdminColumn = $("<div></div>");
    AdminColumn.attr("class", "Element WideToggleElement");
    if (User.Admin === true) {
        Toggle(AdminColumn);
    } else {
        AdminColumn.attr("toggled", "false");
    }
    
    Row.append(AdminColumn);

    var ActiveColumn = $("<div></div>");
    ActiveColumn.attr("class", "Element WideToggleElement");
    if (User.IsActive === true) {
        Toggle(ActiveColumn);
    } else {
        ActiveColumn.attr("toggled", "false");
    }
    Row.append(ActiveColumn);

    var ResetColumn = $("<div></div>");
    ResetColumn.attr("class", "Element WideToggleElement");
    if (User.ResetPassword === true) {
        Toggle(ResetColumn);
    } else {
        ResetColumn.attr("toggled", "false");
    }
    Row.append(ResetColumn);

    return Row;
}

// Delete user row
$(document).on('click', ".DeleteRow", function() {
    var UserName = $(this).parent().children().eq(1).text();
    if(confirm("Are you sure you want to delete " + UserName + "?")) {
        $(this).parent().remove();
    }
});

// Toggle element
$(document).on('click', ".WideToggleElement", function () {
    $(this).parent().attr("modified", "true");
    Toggle($(this));
});

// Toggle element
function Toggle(Element) {
    if (HasAttrValue(Element, "toggled", "true")) {
        Element.attr("toggled", "false");
        Element.html("");
    } else {
        Element.attr("toggled", "true");
        Element.html(checkIcon);
    }
}

// Prompt the user for a new username
$(document).on("click", "#AddNewUserButton", function () {
    var NewUsername = prompt("Enter new username:");
    if (NewUsername === null) {
        return;
    }

    if (NewUsername !== undefined && NewUsername.length > 0) {
        var User = { Username: "", Admin: true, IsActive: true, ResetPassword: false };
        User.Username = NewUsername;
        UserList.push(User);
        CreateUserList();
    }    
});

// Set all modifers to false
function ResetModifiers() {
    $(".Row").each(function () {
        $(this).attr("modified", "false");
    });
}