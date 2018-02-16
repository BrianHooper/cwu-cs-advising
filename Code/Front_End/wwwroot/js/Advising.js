var incomplete = "Incomplete Graduation Requirements:";
var complete = "All requirements completed!";
var unmetRequirements = [];
var PreviousSelect;


$(document).ready(function () {
    ModifyRequirements();
});

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
    // Get the current selected value of the course that is to be deleted
    var selectedCourse = $(this).parent().children().get(0).value;

    // Remove the div element containg the select
    $(this).parent().remove();

    // Update all select elements to contain the newly removed course

    if (selectedCourse.length > 0) {
        AddToUnmetRequirements(selectedCourse);
    }

    UpdateSelectWithAllCourses();

    // ignore "href ='#'"
    return false;
});

$(document).on("click", ".AddCourse", function () {
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
});

$(document).on("focus", ".CourseSelection", function () {
    // Take the old selection and add it to the unmet requirements
    PreviousSelect = ($(this).val());
});

$(document).on("change", ".CourseSelection", function () {
    if (PreviousSelect.length > 0) {
        AddToUnmetRequirements(PreviousSelect);
        PreviousSelect = "";
    }

    // Get the new selection
    var courseName = $(this).val();

    // Remove all occurences of the course, except from this quarter
    RemoveCourseFromAllSelects(courseName);
    // Remove the new course from the list of unmet requirements
    RemoveFromUnmetRequriements(courseName);

    // Add the new course back into the list
    $(this).append($("<option></option>").text(courseName).attr("value", courseName));

    // Set the selected value to the new course
    $(this).val(courseName);

    // Clear any empty values in this select
    $(this).children().each(function () {
        if ($(this).val().length == 0) {
            $(this).remove();
        }
    });

    UpdateSelectWithAllCourses();
    $(this).blur(); // Deselect so focus can be obtained again
});



function ModifyRequirements() {
    // Clear out any old entries in html list
    $("#RemainingRequirements").empty();
    $("#RemainingRequirementsList").empty();
    

    // Update the list
    if (unmetRequirements.length > 0) {
        $("#RemainingRequirements").html(incomplete);
        for (i = 0; i < unmetRequirements.length; i++) {
            $("#RemainingRequirementsList").append("<li>" + unmetRequirements[i] + "</li>");
        }
    } else {
        $("#RemainingRequirements").html(complete);
    }
}

function UpdateSelectWithNewlyRemovedCourse(courseName) {
    // Iterate over each select object
    $(".CourseSelection").each(function () {
        // Get a list of the select objects 'option' children
        var OptionList = $(this).children();

        // If the list of options does not contain the course, add it
        if (!SelectListContains(OptionList, courseName)) {
            $(this).append("<option>" + courseName + "</option>");
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

function RemoveCourseFromAllSelects(courseName) {
    // Iterate over each select element
    $(".CourseSelection").each(function () {
        $(this).children().each(function () {
            if ($(this).val().indexOf(courseName) >= 0) {
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
function AddToUnmetRequirements(courseName) {
    if (unmetRequirements.indexOf(courseName) < 0) {
        unmetRequirements.push(courseName);
    }
    ModifyRequirements();
}

/* Removes a course from the list of unmet requirements */
function RemoveFromUnmetRequriements(courseName) {
    var index = unmetRequirements.indexOf(courseName);
    if (index >= 0) {
        unmetRequirements.splice(index, 1);
    }
    ModifyRequirements();
}

