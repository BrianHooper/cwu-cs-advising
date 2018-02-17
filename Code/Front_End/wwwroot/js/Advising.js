var incomplete = "Incomplete Graduation Requirements:";
var complete = "All requirements completed!";
var unmetRequirements = [];
var PreviousCourse = { "Title": "", "Credits": "0", "Offered": "0" };



var StartingJSON = {
    "Quarters":
    [{ "Title": "Fall 17", "Courses": ["UNIV 101", "MATH 153", "ENG 101", "CS 112"] },
        { "Title": "Winter 18", "Courses": ["MATH 154", "CS 110", "ENG 102", "COMPUTING"] },
        { "Title": "Spring 18", "Courses": ["MATH 172", "CS 111", "BREADTH 1"] },
        { "Title": "Fall 18", "Courses": ["CS 301", "CS 311", "BREADTH 2", "BREADTH 3"] },
        { "Title": "Winter 19", "Courses": ["CS 302", "CS 312", "CS 325", "BREADTH 4"] },
        { "Title": "Spring 19", "Courses": ["MATH 260", "CS 446", "BREADTH 5"] },
        { "Title": "Fall 19", "Courses": ["CS 361", "MATH 330", "BREADTH 6"] },
        { "Title": "Winter 20", "Courses": ["CS 362", "CS 470", "CS ELECTIVE 1"] },
        { "Title": "Spring 20", "Courses": ["CS 380", "CS 420", "CS ELECTIVE 2", "BREADTH 7"] },
        { "Title": "Fall 20", "Courses": ["CS 480", "UNIVERSITY ELECTIVE 1", "CS 392", "CS 427"] },
        { "Title": "Winter 21", "Courses": ["CS 481", "BREADTH 8", "CS ELECTIVE 3", "UNIVERSITY ELECTIVE 2"] },
        { "Title": "Spring 21", "Courses": ["CS 489", "CS 492", "BREADTH 9"] }], 
    "UnmetRequirements": ["CS ELECTIVE 4", "CS ELECTIVE 5"]
};

$(document).ready(function () {
    ModifyRequirements();
    var select = $("#QuarterContainer").children().eq(0).children().eq(1).children().eq(0);
    AddCredits();
});

function GetCourse(SelectObject) {
    var CourseObject = { "Title": "", "Credits": "0", "Offered": "0" };
    CourseObject.Title = $('option:selected', SelectObject).attr('value');
    CourseObject.Credits = $('option:selected', SelectObject).attr('credits');
    CourseObject.Offered = $('option:selected', SelectObject).attr('offered');
    return CourseObject;
}

// Generate button click
$(document).on("click", "#GenerateButton", function () {
    // Create a list of quarters
    var Schedule = { Quarters: [] };

    // For each quarter in the schedule
    $(".Quarter").each(function () {
        // Get the title
        var Quarter = { Title: "", Courses: [] };
        Quarter.Title = $(this).children().eq(0).html();

        // Get each course
        var children = $(this).children();
        children.each(function () {
            var element = $(this).children().eq(0);
            if (element.hasClass("CourseSelection")) {
                Quarter.Courses.push(element.val());
            }
        });

        // Add the quarter to the list of schedules
        Schedule.Quarters.push(Quarter);
    });

    // Convert to JSON format
    var ScheduleJSON = JSON.stringify(Schedule);
    
    console.log(ScheduleJSON);
    return false; // ignore href
});

$(document).on("click", ".DeleteCourse", function () {
    RemoveCredits();
    // Get the current selected value of the course that is to be deleted
    var selectObject = $(this).parent().children().eq(0);
    var selectedCourse = GetCourse(selectObject);

    // Remove the div element containg the select
    $(this).parent().remove();

    // Update all select elements to contain the newly removed course

    if (selectedCourse.Title != undefined && selectedCourse.Title.length > 0) {
        AddToUnmetRequirements(selectedCourse);
    }

    UpdateSelectWithAllCourses();

    // ignore "href ='#'"
    AddCredits();
    return false;
});

$(document).on("click", ".AddCourse", function () {
    RemoveCredits();
    // Create the div container and set its class
    var CourseDiv = $("<div></div>");
    CourseDiv.attr("class", "Flex MiddleFlex");

    // Create the Select attribute and get its class
    var CourseSelect = $("<select></select>");
    CourseSelect.attr("class", "CourseSelection WideFlex");
    CourseDiv.append(CourseSelect);

    // Create a blank value
    CourseSelect.append($("<option></option>"));

    // Add in the delete button
    CourseDiv.append(DeleteButton());

    // Remove the old add button
    var Parent = $(this).parent().parent();
    $(this).parent().remove();

    // Add the new course element
    Parent.append(CourseDiv);

    // Add in a new add button
    Parent.append(AddButton());

    // Update the course element with any unmet requirements
    UpdateSelectWithAllCourses();
    AddCredits();
});

$(document).on("focus", ".CourseSelection", function () {
    // Take the old selection and add it to the unmet requirements
    PreviousCourse = GetCourse($(this));
});

$(document).on("change", ".CourseSelection", function () {
    RemoveCredits();
    if (PreviousCourse.Title != undefined && PreviousCourse.Title.length > 0) {
        AddToUnmetRequirements(PreviousCourse);
    }

    var course = GetCourse($(this));
    // Get the new selection
    var courseName = $(this).val();

    // Remove all occurences of the course, except from this quarter
    RemoveCourseFromAllSelects(course);
    // Remove the new course from the list of unmet requirements
    RemoveFromUnmetRequriements(course);

    // Add the new course back into the list
    $(this).append(CreateCourseOption(course));

    // Set the selected value to the new course
    $(this).val(course.Title);

    // Clear any empty values in this select
    $(this).children().each(function () {
        if ($(this).val().length == 0) {
            $(this).remove();
        }
    });

    UpdateSelectWithAllCourses();
    $(this).blur(); // Deselect so focus can be obtained again
    AddCredits();
});

function AddCredits() {
    $(".Quarter").each(function () {
        var credits = 0;
        $(this).children().each(function () {
            $(this).children().eq(0).each(function () {
                if ($(this).hasClass("CourseSelection")) {
                    var courseCredits = $('option:selected', this).attr('credits');
                    if (courseCredits != undefined) {
                        credits = credits + parseInt(courseCredits);
                    }                     
                }
            });
        });
        $(this).append('<span class="TotalCredits">Total Credits: ' + credits + '</span>');
    });
}

function RemoveCredits() {
    $(".Quarter").children().each(function () {
        if ($(this).hasClass("TotalCredits")) {
            $(this).remove();
        }
    });
}

function ModifyRequirements() {
    // Clear out any old entries in html list
    $("#RemainingRequirements").empty();
    $("#RemainingRequirementsList").empty();
    

    // Update the list
    if (unmetRequirements.length > 0) {
        $("#RemainingRequirements").html(incomplete);
        for (i = 0; i < unmetRequirements.length; i++) {
            $("#RemainingRequirementsList").append("<li>" + unmetRequirements[i].Title + "</li>");
        }
    } else {
        $("#RemainingRequirements").html(complete);
    }
}

function CreateCourseOption(course) {
    var option = $("<option>" + course.Title + "</option>");
    option.attr("value", course.Title);
    option.attr("credits", course.Credits);
    option.attr("offered", course.Offered);
    return option;
}

function UpdateSelectWithNewlyRemovedCourse(course) {
    // Iterate over each select object
    $(".CourseSelection").each(function () {
        // Get a list of the select objects 'option' children
        var OptionList = $(this).children();

        // If the list of options does not contain the course, add it
        if (!SelectListContains(OptionList, course.Title)) {
            $(this).append(CreateCourseOption(course));
        }
    });
}

function UpdateSelectWithAllCourses() {
    for (var i = 0, len = unmetRequirements.length; i < len; i++) {
        UpdateSelectWithNewlyRemovedCourse(unmetRequirements[i]);
    }
}

/* Returns true of a given select list contains a course */
function SelectListContains(OptionList, courseName) {
    for (i = 0; i < OptionList.length; i++) {
        if (OptionList.get(i).value.indexOf(courseName) >= 0) {
            return true;
        }
    }
    return false;
}

function RemoveCourseFromAllSelects(course) {
    if (course.Title == undefined) {
        return;
    } 
    // Iterate over each select element
    $(".CourseSelection").each(function () {
        $(this).children().each(function () {
            if ($(this).val().indexOf(course.Title) >= 0) {
                $(this).remove();
            }
        });
    });
}

function RemoveCourseFromQuarter(courseName) {  
    $(".Quarter").each(function () {
        if ($(this).html().indexOf(courseName) < 0) {
            var QuarterChildren = $(this).children();
            for (j = 1; j < QuarterChildren.length; j++) {
                var course = document.createElement("option");
                course.text = courseName;
                //QuarterChildren[j].childNodes[0].add(course);
            }
        }
    });
}

function AddCourseToQuarter(courseName) {
    $(".Quarter").each(function () {
        if ($(this).html().indexOf(courseName) >= 0) {
            var QuarterChildren = $(this).children();
            for (j = 1; j < QuarterChildren.length; j++) {
                var SelectList = QuarterChildren[j].childNodes;
                for (k = 0; k < SelectList.length; k++) {
                    var OptionList = SelectList[k].childNodes;
                    for (l = 0; l < OptionList.length; l++) {
                        if(OptionList[l].value != undefined && OptionList[l].value.indexOf(courseName) >= 0) {
                            SelectList[k].removeChild(OptionList[l]);
                        }
                    }
                }
            }
        }
    });

    $(".CourseSelection").each(function () {
        if ($(this).children().length < 1) {
            $(this).parent().remove();
        }
    });
}

/* Creates an Add Button element */
function AddButton() {
    var CourseDiv = $("<div></div>");
    CourseDiv.attr("class", "Flex MiddleFlex");
    var Button = $("<span>+</span>");
    Button.attr("class", "AddCourse WideFlex");
    CourseDiv.append(Button);
    return CourseDiv;
}

/* Creates a delete button element */
function DeleteButton() {
    var Button = $("<span>X</span>");
    Button.attr("class", "DeleteCourse WideFlex");
    return Button;
}

/* Adds a course to the list of unmet requirements */
function AddToUnmetRequirements(course) {
    for (var i = 0; i < unmetRequirements.length; i++) {
        if (unmetRequirements[i].Title.indexOf(course.Title) > 0) {
            return;
        }
    }


    unmetRequirements.push(course);

    ModifyRequirements();
}

/* Removes a course from the list of unmet requirements */
function RemoveFromUnmetRequriements(course) {
    var index = unmetRequirements.length;
    var newRequirements = unmetRequirements.slice();
    var match = course.Title;
    for (var i = 0; i < unmetRequirements.length; i++) {
        var element = unmetRequirements[i].Title;
        if (match.indexOf(element) == 0) {
            unmetRequirements.splice(i, 1);
        }
    }
    ModifyRequirements();
}

function CreateQuarter(QuarterTitle, index) {
    var QuarterDiv = $("<div>Test</div>");
    QuarterDiv.attr("class", "Quarter Border");
    QuarterDiv.attr("quarterId", index);
    QuarterDiv.append("<div>" + QuarterTitle + "</div>");
    QuarterDiv.append(AddButton());
    return QuarterDiv;
}