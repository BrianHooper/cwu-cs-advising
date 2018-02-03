var incomplete = "Incomplete Graduation Requirements:";
var complete = "All requirements completed!";
var remainingRequirements = [];


$(document).ready(function () {
    ModifyRequirements();

    $(".DeleteCourse").click(function () {
        var selectedCourse = $(this).parent().children().get(0).value;
        remainingRequirements.push(selectedCourse);
        ModifyRequirements();
        $(this).parent().remove();
        RemoveCourseFromQuarter(selectedCourse);
        return false;
    });

    $(".CourseSelection").change(function () {
        var courseName = $(this).val();
        AddCourseToQuarter(courseName);
        $(this).append($("<option></option>").text(courseName).attr("value", courseName));
        $(this).val(courseName);
    });
});

function ModifyRequirements() {
    $("#RemainingRequirementsList").html("");

    for (i = 0; i < remainingRequirements.length; i++) {
        $("#RemainingRequirementsList").html($("#RemainingRequirementsList").html() + "<li>" + remainingRequirements[i] + "</li>");
    }

    if (remainingRequirements.length > 0) {
        $("#RemainingRequirements").html(incomplete);
    } else {
        $("#RemainingRequirements").html(complete);
    }
}

function RemoveCourseFromQuarter(courseName) {  
    $(".Quarter").each(function () {
        if ($(this).html().indexOf(courseName) < 0) {
            var QuarterChildren = $(this).children();
            for (j = 1; j < QuarterChildren.length; j++) {
                var course = document.createElement("option");
                course.text = courseName;
                QuarterChildren[j].childNodes[0].add(course);
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