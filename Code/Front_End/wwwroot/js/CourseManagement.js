var deleteIcon = "&#10060;";
var checkIcon = "&#10004;";

$(document).ready(function () {
});

$(document).on('click', '.ToggleElement', function () {
    if (HasAttrValue($(this), "toggled", "false")) {
        $(this).attr("toggled", "true");
        $(this).html(checkIcon);
    } else {
        $(this).attr("toggled", "false");
        $(this).html("&nbsp;");
    }
});

$(document).on("click", "#SaveCourses", function () {
    $(".Row").each(function () {
        if (!$(this).hasClass("skip")) {
            if (HasAttrValue($(this), "modified", "true")) { 
                var Course = { "Title": "", "Department": "", "Number": "", "Credits": "", "Offered": "", "PreReqs": [] };
                var Row = $(this).children();
                Course.Title = Row.eq(1).children().first().val();
                Course.Department = Row.eq(2).children().first().val();
                Course.Number = Row.eq(3).children().first().val();
                Course.Credits = Row.eq(4).children().first().val();



                var Offered = "";
                if (HasAttrValue(Row.eq(5), "toggled", "true")) Offered += "3";
                if (HasAttrValue(Row.eq(6), "toggled", "true")) Offered += "4";
                if (HasAttrValue(Row.eq(7), "toggled", "true")) Offered += "1";
                if (HasAttrValue(Row.eq(8), "toggled", "true")) Offered += "2";
                Course.Offered = Offered;

                console.log(Course);
            }
            
        }
    });
});

$(document).on("click", ".PrereqBox", function () {
    var DialogBox = $("<div></div>");
    DialogBox.attr("class", "PrereqPopup");
    var PrereqBox = $("<div></div>");
    PrereqBox.attr("class", "PrereqPopubBox");
    PrereqBox.attr("style", "display: flex; flex-direction:column; justify-content:space-between;")


    PrereqBox.append($("<div class='Flex CenterFlex'><div class='WideFlex'>Prereqs</div><div class='ClosePrereqs'>" + deleteIcon + "</div></div>"));
    PrereqBox.append("<div style='height: 100%;'>CONTENT</div>");
    PrereqBox.append($("<div class='Flex CenterFlex'><div id='SavePrereqs' class='DivButton'>Save</div><div class='ClosePrereqs DivButton'>Close</div></div>"));
    
    $("#CourseContainer").append(DialogBox);
    $("#CourseContainer").append(PrereqBox);
});

$(document).on("click", "#SavePrereqs", function () {
    $(".PrereqPopup").remove();
    $(".PrereqPopubBox").remove();
});

$(document).on("click", ".ClosePrereqs", function () {
    $(".PrereqPopup").remove();
    $(".PrereqPopubBox").remove();
});