var deleteIcon = "&#10060;";

var RequirementHtml = '<div class="Flex CenterFlex Row">' +
    '<div class="Element DeleteRow">[X]</div>' +
    '<div class="Element CourseElement"><input type="text" value="" /></div>' +
    '<div class="Element WideFlex">' +
    '<a href="#">[+]</a>' +
    '</div>' +
    '</div>';

$(document).ready(function () {
    $('#RequirementContainer').on('click', '.DeleteRow', function () {
        $(this).parent().remove();
    });

    $("#AddNewRequirementButton").click(function () {
        var ReqContainerHtml = $("#RequirementContainer").html();
        $("#RequirementContainer").html(ReqContainerHtml + RequirementHtml);
    });
});

