var incomplete = "Incomplete Graduation Requirements:";
var complete = "All requirements completed!";
var unmetRequirements = [];
var PreviousCourse = { "Title": "", "Credits": "0", "Offered": "0" };
var lockIcon = "&#128274;";
var deleteIcon = "&#10060;";




// Document Load
$(document).ready(function () {
    if (StudentSchedule.length > 0) {
        Schedule = JSON.parse(StudentSchedule);
        LoadSchedule(Schedule);
    }
});

/*  Given a parsed JSON objects, loads the schedule onto the QuarterContainer   */
function LoadSchedule(Schedule) {
    $("#QuarterContainer").html("");
    unmetRequirements = Schedule.UnmetRequirements;
    for (var i = 0; i < Schedule.Quarters.length; i++) {
        var Quarter = CreateQuarter(Schedule.Quarters[i].Title, QuarterNameToIndex(Schedule.Quarters[i].Title), Schedule.Quarters[i].Locked);
        for (var j = 0; j < Schedule.Quarters[i].Courses.length; j++) {
            AssignCourse(Quarter, Schedule.Quarters[i].Courses[j]);
        }
        $("#QuarterContainer").append(Quarter);
    }

    // Update remaining requirements list
    ModifyRequirements();

    // Update each select to reflect remaining requirements
    UpdateSelectWithAllCourses();

    // Calculate the number of credits for each quarter
    AddCredits();
}


// Returns true if the quarter is locked
function locked(Quarter) {
    return Quarter.attr("locked").indexOf("true") === 0;
}

// Returns true if the quarter is locked
function SetLocked(Quarter, lock) {
    Quarter.attr("locked", lock.toString());
}

// Converts an integer quarter index to a string quarter name
function IndexToQuarterName(strIndex) {
    if (isNaN(strIndex)) {
        return "Error";
    } else {
        var index = parseInt(strIndex);
        switch (index) {
            case 1: return "Winter";
            case 2: return "Spring"; 
            case 3: return "Summer"; 
            case 4: return "Fall"; 
            default: return "Error";
        }
    }
}

// Converts a string quarter name to an integer quarter index
function QuarterNameToIndex(strIndex) {
    if (strIndex.indexOf("Winter") === 0) {
        return 1;
    } else if (strIndex.indexOf("Spring") === 0) {
        return 2;
    } else if (strIndex.indexOf("Summer") === 0) {
        return 3;
    } else if (strIndex.indexOf("Fall") === 0) {
        return 4;
    } else {
        return -1;
    }
}

/*  Returns the next quarter after the last quarter, in "Season Year" format */
function GetNextQuarter(lastQuarter) {
    var split = lastQuarter.split(" ");
    if (split[0].indexOf("Fall") === 0) {
        return "Winter " + (parseInt(split[1]) + 1);
    } else if (split[0].indexOf("Winter") === 0) {
        return "Spring " + split[1];
    } else if (split[0].indexOf("Spring") === 0) {
        return "Summer " + split[1];
    } else {
        return "Fall " + split[1];
    }
}

/*  Toggles the quarter lock    */
$(document).on("click", ".LockQuarter", function () {
    var Quarter = $(this).parent().parent();
    ToggleCover(Quarter);
    if (locked(Quarter)) {
        SetLocked(Quarter, false);
    } else {
        SetLocked(Quarter, true);
    }
});

/*  Toggles opacity for locked quarters */
function ToggleCover(Quarter) {
    if (locked(Quarter)) {
        Quarter.attr("style", "");
        Quarter.children().each(function () {
            if (!$(this).children().eq(1).hasClass("Title")) {
                $(this).attr("style", "");
            }
        });
    } else {
        Quarter.attr("style", "background-color: #DCDCDC;");
        Quarter.children().each(function () {
            
            if (!$(this).children().eq(1).hasClass("Title")) {
                $(this).attr("style", "opacity: 0.3;");
            }
        });
    }
}

//  Add a new quarter to the schedule after the last quarter
$(document).on("click", "#AddQuarter", function () {
    if ($("#QuarterContainer").children().length === 0) {
        return false;
    }
    $("#QuarterContainer").append(CreateNextQuarter());
    return false;
});


/*  Deletes a quarter from the schedule, and adds all of its selected
    courses to the list of unmet requirements.
    If the quarter is not the last quarter in the schedule, the empty quarter is
    not removed. */
$(document).on("click", ".DeleteQuarter", function () {
    if ($("#QuarterContainer").children().length < 2) {
        return;
    }

    if ($(this).parent().parent().children().length < 3) {
        $(this).parent().parent().remove();
        return;
    }

    // Get the name of this quarter
    var thisQuarterName = $(this).parent().children().eq(1).html();

    // Confirm that the user wants to delete this quarter
    if (!confirm("Are you sure you want to delete " + thisQuarterName + "?")) {
        return;
    }

    
    // Temporarily remove the calculated number of credits
    RemoveCredits();

    // Check whether or not this quarter is the last quarter
    var lastQuarterName = $("#QuarterContainer").children().last().children().eq(0).children().eq(1).html();
    var isLastQuarter = thisQuarterName.indexOf(lastQuarterName) === 0;

    // For each CourseSelection object in the quarter
    // If there is a value selected, add it to the unmet requirements
    $(this).parent().parent().children().each(function () {
        var select = $(this).children().first();
        if (select.hasClass("CourseSelection")) {
            if (select.val().length > 0) {
                AddToUnmetRequirements(GetCourse(select));
            } 
            select.parent().remove();
        }
    });

    // Remove the quarter object if it is the final quarter
    if (isLastQuarter) {
        $(this).parent().parent().remove();
    }

    // Update all other quarters
    UpdateSelectWithAllCourses();
    AddCredits();
});

// Returns a Course object containing the title, number of credits, and quarters offered
function GetCourse(SelectObject) {
    var CourseObject = { "Title": "", "Credits": "0", "Offered": "0" };
    CourseObject.Title = $('option:selected', SelectObject).attr('value');
    CourseObject.Credits = $('option:selected', SelectObject).attr('credits');
    CourseObject.Offered = $('option:selected', SelectObject).attr('offered');
    return CourseObject;
}

function LoadingScreen() {
    $("#AdvisingContainer").append($("<div class='PrereqPopup'>&nbsp;</div>"));
    var loadingBox = $("<div></div>");
    loadingBox.attr("id", "LoadingBox");
    loadingBox.append("<div class='Title'>Generating base case</div>");
    loadingBox.append("<div>Please wait while the schedule is generated. This box will close automatically when the operation is complete.");
    $("#AdvisingContainer").append(loadingBox);
}

// Generate button click
$(document).on("click", "#GenerateButton", function () {
    
    $("#Loading").show();
    var ScheduleJson = StringifySchedule();
    // Pass schedule to the server
    $.ajax({
        type: "POST",
        url: "/Advising?handler=RecieveScheduleForAlgorithm",
        data: ScheduleJson,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            LoadSchedule(JSON.parse(response));
            $("#Loading").hide();
        }, 
        error: function (one, two, three) {
            console.log(three);
        }
    });
});

function StringifySchedule() {
    if ($("#QuarterContainer").children().length === 0) {
        return false;
    }
    // Create a list of quarters
    var Schedule = {
        Name : "", AcademicYear : "", Quarters: [], UnmetRequirements: [], Constraints: { MinCredits: 0, MaxCredits: 18, TakingSummer: false }
    };

    Schedule.Name = $("#StudentDegree").text();
    Schedule.AcademicYear = $("#StudentCatalogYear").text();

    // For each quarter in the schedule
    $(".Quarter").each(function () {
        // Get the title
        var Quarter = { Title: "", Locked: true, Courses: [] };
        Quarter.Title = $(this).children().eq(0).children().eq(1).html();
        Quarter.Locked = locked($(this));
        // Get each course
        var children = $(this).children();
        children.each(function () {
            var element = $(this).children().eq(0);
            if (element.hasClass("CourseSelection")) {
                Quarter.Courses.push(GetCourse(element));
            }
        });

        // Add the quarter to the list of schedules
        Schedule.Quarters.push(Quarter);
    });

    Schedule.UnmetRequirements = unmetRequirements;

    Schedule.Constraints.MinCredits = $("#MinCredits").val();
    Schedule.Constraints.MaxCredits = $("#MaxCredits").val();
    Schedule.Constraints.TakingSummer = $("#TakingSummerCourses").is(":checked");


    console.log(Schedule);
    return JSON.stringify(Schedule);
}

/*  Removes a course from a quarter and adds it back into the unmet requirements    */
$(document).on("click", ".DeleteCourse", function () {
    RemoveCredits();
    // Get the current selected value of the course that is to be deleted
    var selectObject = $(this).parent().children().eq(0);
    var selectedCourse = GetCourse(selectObject);

    // Remove the div element containg the select
    $(this).parent().remove();

    // Update all select elements to contain the newly removed course
    if (selectedCourse.Title !== undefined && selectedCourse.Title.length > 0) {
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

/*  Retreives the last selected value when a select object is changed   */
$(document).on("focus", ".CourseSelection", function () {
    PreviousCourse = GetCourse($(this));
});

/*  For a select object, the previously selected value is added to the
    unmet requirements, and the newly selected course is removed
    from unmet requirements
*/
$(document).on("change", ".CourseSelection", function () {
    RemoveCredits();
    if (PreviousCourse.Title !== undefined && PreviousCourse.Title.length > 0) {
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
        if ($(this).val().length === 0) {
            $(this).remove();
        }
    });

    UpdateSelectWithAllCourses();
    $(this).blur(); // Deselect so focus can be obtained again
    AddCredits();
});

/*
    Calculates the total number of credits in a each quarter
    and appends it to the bottom
*/
function AddCredits() {
    $(".Quarter").each(function () {
        var credits = 0;
        $(this).children().each(function () {
            $(this).children().eq(0).each(function () {
                if ($(this).hasClass("CourseSelection")) {
                    var courseCredits = $('option:selected', this).attr('credits');
                    if (courseCredits !== undefined) {
                        credits = credits + parseInt(courseCredits);
                    }                     
                }
            });
        });
        if (credits > 0) {
            var CreditsDiv = $("<div></div>");
            CreditsDiv.text(credits);
            CreditsDiv.attr("class", "TotalCredits CenterFlex");
            if (credits > parseInt($("#MaxCredits").val())) {
                CreditsDiv.attr("style", "font-weight: bold; background-color: #ff7070;");
            } 
            $(this).append(CreditsDiv);
        }
    });
}

/*
    Removes the calculated credit total
*/
function RemoveCredits() {
    $(".Quarter").children().each(function () {
        if ($(this).hasClass("TotalCredits")) {
            $(this).remove();
        }
    });
}

/*  Update the display showing the list of unmet requirements   */
function ModifyRequirements() {
    // Clear out any old entries in html list
    $("#RemainingRequirements").empty();
    $("#RemainingRequirementsList").empty();
    

    // Update the list
    if (unmetRequirements.length > 0) {
        $("#RemainingRequirements").html(incomplete);
        for (i = 0; i < unmetRequirements.length; i++) {
            var listElement = unmetRequirements[i].Title;
            listElement += " (" + unmetRequirements[i].Credits + ") [ ";
            var QuartersOffered = unmetRequirements[i].Offered.split("");
            for (var j = 0; j < QuartersOffered.length; j++) {
                listElement += IndexToQuarterName(QuartersOffered[j]) + " ";
            }
            listElement += "]";
            
            var ListElement = $("<li></li>");
            ListElement.append($("<div class='ReqListElement ReqTitle'>" + unmetRequirements[i].Title + "</div>"));
            ListElement.append($("<div class='ReqListElement ReqCredits'>" + unmetRequirements[i].Credits + " Cr</div>"));
            var QuartersString = "";
            QuartersOffered = unmetRequirements[i].Offered.split("");
            for (j = 0; j < QuartersOffered.length; j++) {
                QuartersString += IndexToQuarterName(QuartersOffered[j]) + " ";
            }
            ListElement.append($("<div class='ReqListElement ReqQuarters'>" + QuartersString + "</div>"));
            $("#RemainingRequirementsList").append(ListElement);
        }
    } else {
        $("#RemainingRequirements").html(complete);
    }
}

/*  Returns an <option> element based on a course object    */
function CreateCourseOption(course) {
    var option = $("<option>" + course.Title + "</option>");
    option.attr("value", course.Title);
    option.attr("credits", course.Credits);
    option.attr("offered", course.Offered);
    return option;
}

/*  Updates each select value with a course, if it is available */
function UpdateSelectWithNewlyRemovedCourse(course) {
    // Iterate over each select object
    $(".CourseSelection").each(function () {

        var QuarterId = $(this).parent().parent().attr("quarterId");
        if (course.Offered.indexOf(QuarterId) >= 0) {
            // Get a list of the select objects 'option' children
            var OptionList = $(this).children();

            // If the list of options does not contain the course, add it
            if (!SelectListContains(OptionList, course.Title)) {
                $(this).append(CreateCourseOption(course));
            }
        }
    });
}

/*  Updates all select values with the unmet requirements   */
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

/*  Removes a course from all select objects    */
function RemoveCourseFromAllSelects(course) {
    if (course.Title === undefined) {
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
        if (match.indexOf(element) === 0) {
            unmetRequirements.splice(i, 1);
        }
    }
    ModifyRequirements();
}

// Returns a Quarter element
function CreateQuarter(Title, Index, Locked) {
    var QuarterDiv = $("<div></div>");
    QuarterDiv.attr("class", "Quarter Border");
    QuarterDiv.attr("quarterId", Index.toString());
    QuarterDiv.attr("locked", false);
    var QuarterTitle = $("<div></div>");
    QuarterTitle.attr("class", "Flex MiddleFlex");
    QuarterTitle.append($("<div class='LockQuarter'>" + lockIcon + "</div>"));
    QuarterTitle.append($("<div class='Title WideFlex'>" + Title + "</div>"));
    QuarterTitle.append($("<div class='DeleteQuarter'>" + deleteIcon + "</div>"));
    QuarterDiv.append(QuarterTitle);
    QuarterDiv.append(AddButton());
    if (Locked) {
        
        ToggleCover(QuarterDiv);
        SetLocked(QuarterDiv, true);
    }
    return QuarterDiv;
}

// Returns a quarter element with the incremented attributes from the last quarter in a schedule
function CreateNextQuarter() {
    var Title = GetNextQuarter($("#QuarterContainer").children().last().children().eq(0).children().eq(1).html());
    var Index = QuarterNameToIndex(Title);
    return CreateQuarter(Title, Index, false);
}

// Assigns a course to a specific quarter
function AssignCourse(Quarter, Course) {
    //Get rid of the add course button
    Quarter.children().last().remove();

    // Create the div container and set its class
    var CourseDiv = $("<div></div>");
    CourseDiv.attr("class", "Flex MiddleFlex");

    // Create the Select attribute and get its class
    var CourseSelect = $("<select></select>");
    CourseSelect.attr("class", "CourseSelection WideFlex");
    CourseDiv.append(CourseSelect);

    // Create a blank value
    CourseSelect.append(CreateCourseOption(Course));

    // Add in the delete button
    CourseDiv.append(DeleteButton());

    // Add the new course element
    Quarter.append(CourseDiv);

    // Add in a new add button
    Quarter.append(AddButton());
}


$(document).on("click", "#LoadBaseCaseButton", function () {
    if (confirm("Are you sure you want to load the base case? This will overwrite any unsaved changes to the student schedule.")) {
        LoadSchedule(JSON.parse(BaseCase));
    }    
});

// Generate button click
$(document).on("click", "#SaveButton", function () {
    SaveSchedule(true);
});

function SaveSchedule(feedback) {
    var ScheduleJson = StringifySchedule();
    // Pass schedule to the server
    $.ajax({
        type: "POST",
        url: "/Advising?handler=RecieveScheduleForSavingStudent",
        data: ScheduleJson,

        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (feedback) {
                if (response) {
                    alert("Schedule saved!");
                } else {
                    alert("Saving schedule failed, check database connection.");
                }
            }
        },
        error: function (one, two, three) {
            return false;
        }
    });
}

// Generate button click
$(document).on("click", "#SaveBaseCaseButton", function () {
    var ScheduleJson = StringifySchedule();

    // Pass schedule to the server
    $.ajax({
        type: "POST",
        url: "/Advising?handler=RecieveScheduleForSavingBaseCase",
        data: ScheduleJson,

        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            return false;
        },
        error: function (one, two, three) {
            console.log(three);
        }
    });
});

// Generate button click
$(document).on("click", "#PrintButton", function () {
    var ScheduleJson = StringifySchedule();

    // Pass schedule to the server
    $.ajax({
        type: "POST",
        url: "/Advising?handler=RecieveScheduleForSavingBaseCase",
        data: ScheduleJson,

        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            return false;
        },
        error: function (one, two, three) {
            console.log(three);
        }
    });
});

$(document).on("click", "#PrintButton", function () {
    SaveSchedule(false);
    window.open("Print");
});

$(document).on("change", "#MaxCredits", function () {
    RemoveCredits();
    AddCredits();
});