function GetCourseNumber(Course) {
    return Course.ID.match(/\d+/);
}

function GetCoursePrefix(Course) {
    return Course.ID.match(/[a-zA-Z]/);
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
        if (!MatchesCourseNumber(CourseList[i], Min, Max)) {
            MatchesCourse = false;
        }

        if (MatchesCourse) {
            MatchingCourses.push(CourseList[i]);
        }
    }
    return MatchingCourses;
}