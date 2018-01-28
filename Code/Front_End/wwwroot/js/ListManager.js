

$(document).ready(function () {
    $(".ToggleElement").click(function () {
        var x = "X";
        var cur = $(this).html();

        if ($(this).html() === "X") {
            $(this).html("&nbsp;");
        } else {
            $(this).html("X");
        }
    });

    $(".DeleteRow").click(function () {
        $(this).parent().remove();
    });
});

