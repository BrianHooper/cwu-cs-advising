var DegreeList;
var CourseList;
var Modified = false;
var CurrentDegree;

// On Document ready
$(document).ready(function () {
    $("#ResultsContainer").hide();
    CourseList = JSON.parse(CourseListFromServer);
    DegreeList = JSON.parse(DegreeModel);
    LoadDegreeSelect();
});

// On click create new degree
$(document).on("click", "#CreateDegreeButton", function () {
    var DegreeName = $("#CreateDegreeName").val();
    var DegreeYear = parseInt($("#CreateDegreeYear").val());
    

    if (DegreeName.length === 0) {
        return;
    }

    if (Modified) {
        alert("Please save or discard your changes to the current degree before loading a new degree.");
    }

    $("#CreateDegreeName").val("");
    for (var i = 0; i < DegreeList.length; i++) {
        if (StringMatch(DegreeList[i].name, DegreeName) && DegreeList[i].year === DegreeYear) {
            alert("Error: Degree already exists.");
            return;
        }
    }

    Modified = true;
    CurrentDegree = { name: DegreeName, year: DegreeYear, requirements: [] };
    $("#DegreeRequirementBox").html("");
    $("#CurrentDegreeName").text(DegreeYear + " - " + DegreeName);
    LoadDegreeRequirements();
    $("#ResultsContainer").show();
});

$(document).on("click", "#LoadDegreeButton", function () {
    if (Modified) {
        alert("Please save or discard your changes to the current degree before loading a new degree.");
    }

    $("#ResultsContainer").show();
    
    var DegreeID = $("#DegreeSelect").val();
    if (DegreeID < 0 || DegreeID >= DegreeList.length) {
        alert("Error selecting degree.");
        return;
    }

    $("#DegreeRequirementBox").html("");
    $("#CourseContainer").html("");
    $("#CurrentDegreeName").text(DegreeList[DegreeID].year + " - " + DegreeList[DegreeID].name);
    CurrentDegree = DegreeList[DegreeID];
    console.log(CurrentDegree);
    LoadDegreeRequirements();
});

$(document).on("click", "#SaveDegreeButton", function () {
    if (CurrentDegree === null || CurrentDegree === undefined) {
        return;
    }

    $.ajax({
        type: "POST",
        url: "/ManageDegrees?handler=SendDegree",
        data: JSON.stringify(CurrentDegree),

        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            Modified = false;
            alert("Degree saved.");
            $("#ResultsContainer").hide();
        },
        failure: function (response) {
            console.log(response);
        }
    });
});

$(document).on('click', '#SearchCourses', function () {
    $("#CourseContainer").html("");

    var MinCourseNumber = $("#MinCourseSearch").val();
    var MaxCourseNumber = $("#MaxCourseSearch").val();
    var Department = $("#DepartmentSearch").val();
    var OfferedSummer = $("#SummerSearch").is(":checked");
    var OfferedFall = $("#FallSearch").is(":checked");
    var OfferedWinter = $("#WinterSearch").is(":checked");
    var OfferedSpring = $("#SpringSearch").is(":checked");

    var MatchingCourses = SearchCourses(MinCourseNumber, MaxCourseNumber, Department, OfferedSummer, OfferedFall, OfferedWinter, OfferedSpring);
    for (var i = 0; i < MatchingCourses.length; i++) {
        LoadCourse(MatchingCourses[i].ID);
    }
});

function LoadCourse(CourseName) {
    var CourseBox = $("<span></span>");
    CourseBox.text(CourseName);
    CourseBox.attr("class", "AddCourseRequirement");
    $("#CourseContainer").append(CourseBox);
}

function LoadDegreeSelect() {
    for (var i = 0; i < DegreeList.length; i++) {
        $("#DegreeSelect").append("<option value='" + i + "'>" + DegreeList[i].year + " - " + DegreeList[i].name + "</option>");
    }
}

function LoadDegreeRequirements() {
    if (CurrentDegree === null || CurrentDegree === undefined) {
        alert("Error loading degree requirements.");
        return;
    }
    
    for (var i = 0; i < CurrentDegree.requirements.length; i++) {
        AddRequirement(CurrentDegree.requirements[i]);
    }
}

$(document).on('click', '.AddCourseRequirement', function () {
    var CourseName = $(this).text();
    
    if (CurrentDegree === null || CurrentDegree === undefined) {
        return;
    }

    if (StringArrayContains(CurrentDegree.requirements, CourseName)) {
        alert("Degree already contains " + CourseName);
        return;
    }
    Modified = true;
    AddRequirement(CourseName);
    CurrentDegree.requirements.push(CourseName);
    $(this).remove();
});

$(document).on('click', '.DeleteCourseRequirement', function () {
    if (CurrentDegree === null || CurrentDegree === undefined) {
        return;
    }

    var CourseName = $(this).text();

    if (!StringArrayContains(CurrentDegree.requirements, CourseName)) {
        return;
    }

    Modified = true;
    CurrentDegree.requirements = RemoveStringFromArray(CurrentDegree.requirements, CourseName);
    $(this).remove();
});

function AddRequirement(CourseName) {
    var CourseBox = $("<span></span>");
    CourseBox.text(CourseName);
    CourseBox.attr("class", "DeleteCourseRequirement");
    $("#DegreeRequirementBox").append(CourseBox);
}