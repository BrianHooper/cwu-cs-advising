var incomplete = "Incomplete Graduation Requirements:";
var complete = "All requirements completed!";
var unmetRequirements = [];


$(document).ready(function () {
    ModifyRequirements();
    var PreviousSelect;

    $(".DeleteCourse").click(function () {
        // Get the current selected value of the course that is to be deleted
        var selectedCourse = $(this).parent().children().get(0).value;

        // Add the course to the list of unmet requirements
        AddToUnmetRequirements(selectedCourse);

        // Remove the div element containg the select
        $(this).parent().remove();

        // Update all select elements to contain the newly removed course
        //RemoveCourseFromQuarter(selectedCourse);
        UpdateSelectWithNewlyRemovedCourse(selectedCourse);

        // ignore "href ='#'"
        return false;
    });

    $(".CourseSelection").on("focus", function () {
        // Take the old selection and add it to the unmet requirements
        PreviousSelect = ($(this).val());
    }).change(function () {
        // Take the old selection and add it to the unmet requirements
        AddToUnmetRequirements(PreviousSelect);
        PreviousSelect = "";

        // Get the new selection
        var courseName = $(this).val();
        
        // Remove all occurences of the course, except from this quarter
        RemoveCourseFromAllSelects(courseName, $(this).parent().parent().children().eq(0).html());

        // Remove the new course from the list of unmet requirements
        RemoveFromUnmetRequriements(courseName);

        // Add the new course back into the list
        $(this).append($("<option></option>").text(courseName).attr("value", courseName));

        // Set the selected value to the new course
        $(this).val(courseName);
        });

    $(".AddCourse").click(function () {
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
    // Iterate over the list of unmet requirements and add them to each select
    for (i = 0; i < unmetRequirements.length; i++) {
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

function RemoveCourseFromAllSelects(courseName, skip) {
    // Iterate over each select element
    $(".CourseSelection").each(function () {
        if (skip.indexOf($(this).parent().parent().children().eq(0).html()) < 0) {


            //alert(skip + ": " + $(this).parent().parent().children().eq(0).html());
            // Get the selected value of the select element
            var selected = $(this).val();
            if (selected.indexOf(courseName) >= 0) {
                $(this).parent().remove();
            }
        }
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
    var CourseDiv = $("<div></div>");
    CourseDiv.attr("class", "Flex MiddleFlex");
    var Button = $("<span>X</span>");
    Button.attr("class", "DeleteCourse WideFlex");
    CourseDiv.append(Button);
    return CourseDiv;
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

