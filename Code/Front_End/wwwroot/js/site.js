function StringArrayContains(Array, Str) {
    for (var i = 0; i < Array.length; i++) {
        if (Array[i].indexOf(Str) === 0) {
            return true;
        }
    }
    return false;
}

function StringMatch(strA, strB) {
    return strA.indexOf(strB) >= 0;
}

function HasAttribute(Object, Attribute) {
    return Object.attr(Attribute) != undefined;
}

function HasAttrValue(Object, Attribute, Value) {
    if (!HasAttribute(Object, Attribute)) {
        return false;
    }

    return StringMatch(Object.attr(Attribute), Value);
}