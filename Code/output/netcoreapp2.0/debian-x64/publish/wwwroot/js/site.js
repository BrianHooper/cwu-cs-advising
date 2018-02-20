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