var deleteIcon = "&#10060;";
var checkIcon = "&#10004;";
var editIcon = "&#9998;";

var Departments = [];
var StartingCourses = CourseListFromServer;
var CourseList = JSON.parse(StartingCourses);

var ModifiedCourses = false;
var UniqueCourseId = 0;

$(document).ready(function () {
    ResetCourseContainer();
    LoadDepartments(CourseList);
});

function GetCourseNumber(Course) {
    return Course.match(/\d+/);
}

function GetCoursePrefix(Course) {
    return Course.match(/[a-zA-Z]/);
}

function MatchesCourseNumber(Course, Min, Max) {
    var CourseNumber = GetCourseNumber(Course);
    return CourseNumber >= Min && CourseNumber <= Max;
}

function SearchCourses(Min, Max, Department, Summer, Fall, Winter, Spring) {
    var MatchingCourses = [];
    for (var i = 0; i < CourseList.length; i++) {
        var MatchesCourse = false;

        if (Summer && StringMatch(CourseList[i].Offered, "3")) {
            MatchesCourse = true;
        } else if (Fall && StringMatch(CourseList[i].Offered, "4")) {
            MatchesCourse = true;
        } else if (Winter && StringMatch(CourseList[i].Offered, "1")) {
            MatchesCourse = true;
        } else if (Spring && StringMatch(CourseList[i].Offered, "2")) {
            MatchesCourse = true;
        }

        if (!StringMatch(Department, "any") && !StringMatch(CourseList[i].Department, Department)) {
            MatchesCourse = false;
        }

        if (!MatchesCourseNumber(CourseList[i].Number, Min, Max)) {
            MatchesCourse = false;
        }

        if (MatchesCourse) {
            MatchingCourses.push(CourseList[i]);
        }
    }
    return MatchingCourses;
}

$(document).on('click', '#SearchCourses', function () {
    if (ModifiedCourses) {
        if (confirm("Save unsaved changes before reloading courses?")) {
            SaveCourses();
        }
    }

    ResetCourseContainer();

    var MinCourseNumber = $("#MinCourseSearch").val();
    var MaxCourseNumber = $("#MaxCourseSearch").val();
    var Department = $("#DepartmentSearch").val();
    var OfferedSummer = $("#SummerSearch").is(":checked");
    var OfferedFall = $("#FallSearch").is(":checked");
    var OfferedWinter = $("#WinterSearch").is(":checked");
    var OfferedSpring = $("#SpringSearch").is(":checked");

    var MatchingCourses = SearchCourses(MinCourseNumber, MaxCourseNumber, Department, OfferedSummer, OfferedFall, OfferedWinter, OfferedSpring);
    for (var i = 0; i < MatchingCourses.length; i++) {
        LoadCourse(MatchingCourses[i]);
    }
});

function SaveCourses() {
    var Courses = ReadCoursesToList();
    ModifiedCourses = false;
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
    CourseHeader.append($('<div class="Element CourseNumberElement">Number</div>'));
    CourseHeader.append($('<div class="Element CourseElement CourseNumberInput">Credits</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">Su</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">F</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">W</div>'));
    CourseHeader.append($('<div class="Element ButtonElement">S</div>'));
    CourseHeader.append($('<div class="Element WideFlex">Prereqs</div>'));
    $("#CourseContainer").append(CourseHeader);
}

function LoadDepartments(Courses) {
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
    RowDiv.attr("UniqueId", UniqueCourseId++);

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
    NumberBox.attr("class", "Element CourseNumberElement");
    NumberBox.append($("<input type='text' value='' />"));
    RowDiv.append(NumberBox);

    // Credits
    var CreditsBox = $("<div></div>");
    CreditsBox.attr("class", "Element CourseElement CourseNumberInput");
    CreditsBox.append($("<input type='text' value='' />"));
    RowDiv.append(CreditsBox);

    // Quarters offered
    for (var j = 0; j < 4; j++) {
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
    ModifiedCourses = true;
});

$(document).on('input', '.Element', function () {
    $(this).parent().attr("modified", "true");
    ModifiedCourses = true;
});

$(document).on('click', '#AddNewCourseButton', function () {
    $("#CourseContainer").append(CreateCourseRow());
});

$(document).on('click', '.DeleteRow', function () {
    var CourseTitle = $(this).parent().children().eq(1).children().first().val();
    if (CourseTitle.length > 0) {
        if (confirm("Are you sure you want to delete " + CourseTitle + "?")) {
            ModifiedCourses = true;
            $(this).parent().remove();
        }
    } else {
        $(this).parent().remove();
    }
});

/*  Handles toggling of check marks */
$(document).on('click', '.ToggleElement', function () {
    ModifiedCourses = true;
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

function ResetModifiers() {
    ModifiedCourses = false;
    $(".Row").each(function () {
        if (HasAttrValue($(this), "modified", "true")) {
            $(this).attr("modified", "false");
        }
    });
}

/*  Builds the Json object to pass back to the server   */
$(document).on("click", "#SaveCourses", function () {
    var Courses = ReadCoursesToList();
    var ModifiedCoursesJson = JSON.stringify(Courses);
    console.log(ModifiedCoursesJson);
    PassCoursesToServer(Courses);
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

function GetCourseByName(Name) {
    for (var i = 0; i < CourseList.length; i++) {
        if (StringMatch(CourseList[i].Title, Name)) {
            return CourseList[i];
        }
    }
    return null;
}

function CreatePrereqSearch() {
    var SearchBox = $("<div></div>");
    var CourseNumbers = $("<div class='Border' style='margin-bottom: 10px;'></div>");
    CourseNumbers.append("Courses Numbered:");
    CourseNumbers.append($("<input type='text' style='width: 40px; margin-left: 10px;' value='0' id='MinPrereqSearch'/>"));
    CourseNumbers.append(" to ");
    CourseNumbers.append($("<input type='text' style='width: 40px;' value='999' id='MaxPrereqSearch'/>"));
    SearchBox.append(CourseNumbers);

    var DepartmentSearch = $("<div class='Border' style='margin-bottom: 10px;'></div>");
    DepartmentSearch.append("Department: ");
    var DepartmentList = $("<select class='CourseSelection' style='width: 200px;' id='PrereqDepartmentSearch'></select>");
    DepartmentList.append("<option value='any'>--- Any ---</option>");
    for (var i = 0; i < Departments.length; i++) {
        DepartmentList.append("<option value='" + Departments[i] + "'>" + Departments[i] + "</option>");
    }
    DepartmentList.val("any");
    DepartmentSearch.append(DepartmentList);
    SearchBox.append(DepartmentSearch);

    var QuarterSearch = $("<div class='Border'></div>");
    QuarterSearch.append("<span style='margin-right: 10px;'>Courses Offered: </span>");
    QuarterSearch.append($("<input type='checkbox' id='SummerPrereqSearch' />" + " Summer"));
    QuarterSearch.append(" Summer");
    QuarterSearch.append($("<input type='checkbox' id='FallPrereqSearch' checked /> Fall"));
    QuarterSearch.append(" Fall");
    QuarterSearch.append($("<input type='checkbox' id='WinterPrereqSearch' checked /> "));
    QuarterSearch.append(" Winter");
    QuarterSearch.append($("<input type='checkbox' id='SpringPrereqSearch' checked /> "));
    QuarterSearch.append(" Spring");
    SearchBox.append(QuarterSearch);

    SearchBox.append($("<button type='button' class='RegularButton' id='SearchForPrereqs'>Search</button>"));
    var PrereqSearchResults = $("<div></div>");
    PrereqSearchResults.attr("id", "PrereqSearchResults");
    SearchBox.append(PrereqSearchResults);


    return SearchBox;
}

$(document).on('click', '#SearchForPrereqs', function () {
    var min = $("#MinPrereqSearch").val();
    var max = $("#MaxPrereqSearch").val();
    var dept = $("#PrereqDepartmentSearch").val();
    var pSummer = $("#SummerPrereqSearch").is(":checked");
    var pFall = $("#FallPrereqSearch").is(":checked");
    var pWinter = $("#WinterPrereqSearch").is(":checked");
    var pSpring = $("#SpringPrereqSearch").is(":checked");
    var MatchingCourses = SearchCourses(min, max, dept, pSummer, pFall, pWinter, pSpring);
    for (var i = 0; i < MatchingCourses.length; i++) {
        $("#PrereqSearchResults").append(AddedPrereqButtion(MatchingCourses[i].Number, checkIcon, "AddPrereq"));
    }
});

var PrereqEditId = 0;

function AddedPrereqButtion(Name, Icon, Class) {
    var PrereqButton = $("<div class='" + Class + "'></div>");
    PrereqButton.append("<span>" + Icon + "</span>");
    PrereqButton.append("<span>" + Name + "</span>");
    return PrereqButton;
}

$(document).on("click", ".PrereqBox", function () {
    PrereqEditId = $(this).parent().attr("uniqueId");

    var background = $("<div class='PrereqPopup'></div>");
    $("#CourseContainer").append($("<div class='PrereqPopup'>&nbsp;</div>"));

    var PrereqBox = $("<div></div>");
    PrereqBox.attr("class", "PrereqPopubBox");
    PrereqBox.attr("style", "display: flex; flex-direction: column; justify-content:space-between;");
    PrereqBox.append($("<div class='Flex CenterFlex'><div class='WideFlex Title'>Modify Course Prerequisites</div><div class='ClosePrereqs'>" + deleteIcon + "</div></div>"));
    PrereqBox.append(CreatePrereqSearch());
    var CourseName = $(this).parent().children().eq(1).children().first().val();
    PrereqBox.append("<div class='Title' id='PrereqCourseTitle'>" + CourseName + "</div>");

    var PrereqListContainer = $("<div></div>");
    PrereqListContainer.attr("id", "PrereqListContainer");
    for (var i = 0; i < $(this).children().length; i++) {
        var Prereq = $(this).children().eq(i).html();
        PrereqListContainer.append(AddedPrereqButtion(Prereq, deleteIcon, "DeletePrereq"));
    }
    PrereqBox.append(PrereqListContainer);

    PrereqBox.append($("<div class='Flex CenterFlex'><button type='button' id='SavePrereqs' class='RegularButton'>Save</button><button type='button' class='ClosePrereqs RegularButton'>Close</button></div>"));
    $("#CourseContainer").append(PrereqBox);
});

function GetCourseRowById(id) {
    var MatchingRow = null;
    $(".Row").each(function () {
        if ($(this).attr("uniqueId") === id) {
            MatchingRow = $(this);
        }
    });
    return MatchingRow;
}

$(document).on("click", "#SavePrereqs", function () {
    var Prereqs = [];
    for (var i = 0; i < $("#PrereqListContainer").children().length; i++) {
        Prereqs.push($("#PrereqListContainer").children().eq(i).children().last().html());
    }
    var Row = GetCourseRowById(PrereqEditId);
    Row.attr("modified", "true");
    ModifiedCourses = true;
    if (Row !== null) {
        var PrereqContainer = Row.children().last();
        PrereqContainer.html(editIcon);
        for (var j = 0; j < Prereqs.length; j++) {
            PrereqContainer.append($("<span class='Prereq'>" + Prereqs[j] + "</span>"));
        }
    }
    
    $(".PrereqPopup").remove();
    $(".PrereqPopubBox").remove();
});

$(document).on("click", ".ClosePrereqs", function () {
    $(".PrereqPopup").remove();
    $(".PrereqPopubBox").remove();
});

$(document).on("click", ".DeletePrereq", function () {
    $(this).remove();
});

$(document).on("click", ".AddPrereq", function () {
    var Prereq = $(this).children().last().html();
    if (!ContainsPrereq(Prereq)) {
        $("#PrereqListContainer").append(AddedPrereqButtion(Prereq, deleteIcon, "AddPrereq"));
        $(this).remove();
    } else {
        alert("Error: Course already contains prerequisite.");
    }
});

function ContainsPrereq(PrereqName) {
    for (var i = 0; i < $("#PrereqListContainer").children().length; i++) {
        if (StringMatch($("#PrereqListContainer").children().eq(i).children().last().html(), PrereqName)) {
            return true;
        }
    }
    return false;
}