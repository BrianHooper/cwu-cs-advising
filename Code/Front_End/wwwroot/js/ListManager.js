var CourseHtml = '<div class="Flex CenterFlex Row">' + 
    '<div class="Element DeleteRow" >[X]</div >' +
    '<div class="Element CourseElement"><input type="text" value="" /></div>' +
    '<div class="Element CourseElement">' +
    '<select>' +
    '<option>------</option>' +
    '<option>Computer Science</option>' +
    '<option>Mathematics</option>' +
    '</select>' +
    '</div>' +
    '<div class="Element CourseElement CourseNumberInput"><input type="text" value=""></div>' +
    '<div class="Element CourseElement CourseNumberInput"><input type="text" value="0"></div>' +
    '<div class="Element ToggleElement">&nbsp;</div>' +
    '<div class="Element ToggleElement">&nbsp;</div>' +
    '<div class="Element ToggleElement">&nbsp;</div>' +
    '<div class="Element ToggleElement">&nbsp;</div>' +
    '<div class="Element WideFlex">' +
    '<a href="#">[ CS 361 ]</a>' +
    '<a href="#">[ CS 312 ]</a>' +
    '<a href="#">[+]</a>' +
    '</div>' +
    '</div>';

var RequirementHtml = '<div class="Flex CenterFlex Row">' +
    '<div class="Element DeleteRow">[X]</div>' +
    '<div class="Element CourseElement"><input type="text" value="" /></div>' +
    '<div class="Element WideFlex">' +
    '<a href="#">[+]</a>' +
    '</div>' +
    '</div>';

$(document).ready(function () {
    $('#CourseContainer').on('click', '.ToggleElement', function () {
        var x = "X";
        var cur = $(this).html();

        if ($(this).html() === "X") {
            $(this).html("&nbsp;");
        } else {
            $(this).html("X");
        }
    });
    $("#AddNewCourseButton").click(function () {
        var CourseContainerHtml = $("#CourseContainer").html();
        $("#CourseContainer").html(CourseContainerHtml + CourseHtml);
    });

    $('#CourseContainer').on('click', '.DeleteRow', function () {
        $(this).parent().remove();
    });

    $('#RequirementContainer').on('click', '.DeleteRow', function () {
        $(this).parent().remove();
    });

    $("#AddNewRequirementButton").click(function () {
        var ReqContainerHtml = $("#RequirementContainer").html();
        $("#RequirementContainer").html(ReqContainerHtml + RequirementHtml);
    });
});

