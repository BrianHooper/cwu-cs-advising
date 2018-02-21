var deleteIcon = "&#10060;";
var checkIcon = "&#10004;";
var editIcon = "&#9998;";

var Departments = [];
var StartingCourses = '[{"Title":"Discrete Math","Department":"Mathematics","Number":"330","Credits":"5","Offered":"32","PreReqs":["CS 301"]},{"Title":"Data Structures","Department":"Computer Science","Number":"302","Credits":"4","Offered":"4","PreReqs":[]},{"Title":"Operating Systems","Department":"Computer Science","Number":"470","Credits":"4","Offered":"1","PreReqs":["CS 361"]}]';
var CourseList = JSON.parse(StartingCourses);

$(document).ready(function () {
    ResetCourseContainer();
    LoadDepartments(CourseList);
    //LoadCourses(CourseList);
});

$(document).on('click', '#SearchCourses', function () {
    if (!SaveCourses()) {
        return;
    }
    ResetCourseContainer();

    var MinCourseNumber = $("#MinCourseSearch").val();
    var MaxCourseNumber = $("#MaxCourseSearch").val();
    var Department = $("#DepartmentSearch").val();
    var OfferedSummer = $("#SummerSearch").is(":checked");
    var OfferedFall = $("#FallSearch").is(":checked");
    var OfferedWinter = $("#WinterSearch").is(":checked");
    var OfferedSpring = $("#SpringSearch").is(":checked");

    for (var i = 0; i < CourseList.length; i++) {
        var MatchesCourse = false;
        
        if ((OfferedSummer && StringMatch(CourseList[i].Offered, "3"))) {
            MatchesCourse = true;
        } else if ((OfferedFall && StringMatch(CourseList[i].Offered, "4"))) {
            MatchesCourse = true;
        } else if ((OfferedWinter && StringMatch(CourseList[i].Offered, "1"))) {
            MatchesCourse = true;
        } else if ((OfferedSpring && StringMatch(CourseList[i].Offered, "2"))) {
            MatchesCourse = true;
        }

        if (!StringMatch(Department, "any") && !StringMatch(CourseList[i].Department, Department)) {
            MatchesCourse = false;
        } 

        if (CourseList[i].Number < MinCourseNumber || CourseList[i].Number > MaxCourseNumber) {
            MatchesCourse = false;
        }
        
        if (MatchesCourse) {
            LoadCourse(CourseList[i]);
        }
    }
});

function SaveCourses() {
    var Courses = ReadCoursesToList();
    return true;
}

function ResetCourseContainer() {
    $("#CourseContainer").html("");
    $("#CourseContainer").append($("<div class='Title'>Courses</div>"));
    var CourseHeader = $("<div></div>");
    CourseHeader.attr("class", "Flex CenterFlex Row skip");
    CourseHeader.append($('<div class="Element ButtonElement">' + deleteIcon + '</div>'));
    CourseHeader.append($('<div class="Element CourseElement">Course Title</div>'));
    CourseHeader.append($('<div class="Element CourseElement">Department</div>'));
    CourseHeader.append($('<div class="Element CourseElement CourseNumberInput">Number</div>'));
    CourseHeader.append($('<div class="Element CourseElement CourseNumberInput">Credits</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">Su</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">F</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">W</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">S</div>'));
    CourseHeader.append($('<div class="Element WideFlex">Prereqs</div>'));
    $("#CourseContainer").append(CourseHeader);
}

function LoadDepartments(Courses) {
    Departments.push("");
    for (var i = 0; i < Courses.length; i++) {
        if (!StringArrayContains(Departments, Courses[i].Department)) {
            Departments.push(Courses[i].Department);
            $("#DepartmentSearch").append("<option value='" + Courses[i].Department + "'>" + Courses[i].Department + "</option>");
        }
    }
}

function LoadCourses(Courses) {
    for (var i = 0; i < Courses.length; i++) {
        LoadCourse(Courses[i]);
    }
}

function LoadCourse(Course) {
    var CourseRow = CreateCourseRow();
    CourseRow.children().eq(1).children().first().val(Course.Title);

    CourseRow.children().eq(2).children().first().val(Course.Department);
    CourseRow.children().eq(3).children().first().val(Course.Number);
    CourseRow.children().eq(4).children().first().val(Course.Credits);

    if (StringMatch(Course.Offered, "3")) {
        ToggleClick(CourseRow.children().eq(5));
    }
    if (StringMatch(Course.Offered, "4")) {
        ToggleClick(CourseRow.children().eq(6));
    }
    if (StringMatch(Course.Offered, "1")) {
        ToggleClick(CourseRow.children().eq(7));
    }
    if (StringMatch(Course.Offered, "2")) {
        ToggleClick(CourseRow.children().eq(8));
    }

    for (var j = 0; j < Course.PreReqs.length; j++) {
        CourseRow.children().eq(9).append($("<span class='Prereq'>" + Course.PreReqs[j] + "</span>"));
    }

    $("#CourseContainer").append(CourseRow);
}

function CreateCourseRow() {
    // Container
    var RowDiv = $("<div></div>");
    RowDiv.attr("class", "Flex CenterFlex Row");
    RowDiv.attr("modified", "false");

    // Delete
    var DeleteBox = $("<div></div>");
    DeleteBox.attr("class", "Element DeleteRow");
    DeleteBox.append(deleteIcon);
    RowDiv.append(DeleteBox);

    // Title
    var TitleBox = $("<div></div>");
    TitleBox.attr("class", "Element CourseElement");
    TitleBox.append($("<input type='text' value='' />"));
    RowDiv.append(TitleBox);

    // Department
    var DepartmentBox = $("<div></div>");
    DepartmentBox.attr("class", "Element CourseElement");
    var DepartmentSelect = $("<select></select>");
    
    for (var i = 0; i < Departments.length; i++) {
        DepartmentSelect.append($("<option value='" + Departments[i] + "'>" + Departments[i] + "</option>"));
    }
    
    DepartmentBox.append(DepartmentSelect);
    RowDiv.append(DepartmentBox);

    // Number
    var NumberBox = $("<div></div>");
    NumberBox.attr("class", "Element CourseElement CourseNumberInput");
    NumberBox.append($("<input type='text' value='' />"));
    RowDiv.append(NumberBox);

    // Credits
    var CreditsBox = $("<div></div>");
    CreditsBox.attr("class", "Element CourseElement CourseNumberInput");
    CreditsBox.append($("<input type='text' value='' />"));
    RowDiv.append(CreditsBox);

    // Quarters offered
    for (var i = 0; i < 4; i++) {
        var QuarterBox = $("<div></div>");
        QuarterBox.attr("class", "Element ToggleElement");
        QuarterBox.attr("toggled", "false");
        QuarterBox.append("&nbsp;");
        RowDiv.append(QuarterBox);
    }

    // Prereqs
    var PrereqsBox = $("<div></div>");
    PrereqsBox.attr("class", "Element WideFlex PrereqBox");
    PrereqsBox.append(editIcon);
    RowDiv.append(PrereqsBox);

    return RowDiv;
}

$(document).on('change', '.Element', function () {
    $(this).parent().attr("modified", "true");
});

$(document).on('input', '.Element', function () {
    $(this).parent().attr("modified","true");
});

$(document).on('click', '#AddNewCourseButton', function () {
    $("#CourseContainer").append(CreateCourseRow());
});

$(document).on('click', '.DeleteRow', function () {
    var CourseTitle = $(this).parent().children().eq(1).children().first().val();
    if (CourseTitle.length > 0) {
        if (confirm("Are you sure you want to delete " + CourseTitle + "?")) {
            $(this).parent().remove();
        }
    } else {
        $(this).parent().remove();
    }
});

/*  Handles toggling of check marks */
$(document).on('click', '.ToggleElement', function () {
    $(this).parent().attr("modified", "true");
    ToggleClick($(this));
});

function ToggleClick(Element) {
    if (HasAttrValue(Element, "toggled", "false")) {
        Element.attr("toggled", "true");
        Element.html(checkIcon);
    } else {
        Element.attr("toggled", "false");
        Element.html("&nbsp;");
    }
}

/*  Builds the Json object to pass back to the server   */
$(document).on("click", "#SaveCourses", function () {
    var Courses = ReadCoursesToList();
    var ModifiedCoursesJson = JSON.stringify(Courses);
    console.log(ModifiedCoursesJson);
});

function ReadCoursesToList() {
    var Courses = [];
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

                for (var i = 0; i < Row.eq(9).children().length; i++) {
                    Course.PreReqs.push(Row.eq(9).children().eq(i).html());
                }

                Courses.push(Course);
            }
        }
    });
    return Courses;
}

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