// NBright JQuery plugin to generate ajax post of input fields - v1.0.1
(function ($) {

    // Usage: var values = $.fn.genxmlajax(selectordiv);
    // selectordiv: The div selector whcih encapsulates the controls for whcih data will be passed tothe server.
    $.fn.genxmlajax = function (selectordiv) {
        return getgenxml(selectordiv);
    };

    $.fn.genxmlajaxitems = function (selectordiv, selectoritemdiv) {
        return getgenxmlitems(selectordiv, selectoritemdiv);
    };

    $.fn.popupformlist = function (selformdiv, sellistdiv, selpopupbutton, ajaxbutton, cmdupdate, width) {

        $(selformdiv).dialog({
            autoOpen: false,
            width: width,
            buttons: {
                "Ok": function () {

                    $(selformdiv).trigger('beforeupdate');

                    // get all the inputs into an array.
                    var values = getgenxml(selformdiv);
                    var request = $.ajax({ type: "POST",
                        url: cmdupdate,
                        data: { inputxml: escape(values) }
                    });

                    $(this).dialog("close");

                    request.done(function (data) {
                        $(selformdiv).trigger('afterupdate');
                        displayList(data, ajaxbutton, sellistdiv);
                    });

                    request.fail(function (jqXHR, textStatus) {
                        alert("Request failed: " + textStatus);
                    });

                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            }
        });

        // Dialog Link	
        $(selpopupbutton).click(function () {
            $(selformdiv).dialog('open');
            return false;
        });

    };

    $.fn.initlist = function (sellistdiv, ajaxbutton, cmdget) {
        $.ajaxSetup({ cache: false });
        $.get(cmdget, function (data) {
            displayList(data, ajaxbutton, sellistdiv);
        });
    };

    function displayList(data, ajaxbutton, sellistdiv) {
        $(sellistdiv).html(data);
        $(ajaxbutton).click(function () {
            var cmd = $(this).attr("cmd");
            $.get(cmd, function (data) {
                displayList(data, ajaxbutton, sellistdiv);
            });
        });
    };

    function getgenxmlitems(selectordiv, selectoritemdiv) {
        // get each item div into xml format.
        var values = "<root>";

        var $inputs = $(selectordiv).children(':input');
        $inputs.each(function () {
            values += getctrlxml($(this));
        });

        var $selects = $(selectordiv).children(' select');
        $selects.each(function () {
            strID = $(this).attr("id");
            nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1
            while (shortID.length < 3) {
                lp++;
                shortID = nam[nam.length - lp];
            }
            values += '<f t="dd"  id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
        });

        $(selectordiv).children(selectoritemdiv).each(function () {
            values += '<root>';
            var $iteminputs = $(this).find(':input');
            $iteminputs.each(function () {
                values += getctrlxml($(this));
            });

            var $itemselects = $(this).find(' select');
            $itemselects.each(function () {
                strID = $(this).attr("id");
                nam = strID.split('_');
                var shortID = nam[nam.length - 1];
                var lp = 1
                while (shortID.length < 3) {
                    lp++;
                    shortID = nam[nam.length - lp];
                }
                values += '<f t="dd"  id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
            });

            values += '</root>';
        });

        values += '</root>';
        return values;
    };

    function getgenxml(selectordiv) {

        // get all the inputs into an array.
        var values = "<root>";

        var $inputs = $(selectordiv + ' :input');
        $inputs.each(function () {
            values += getctrlxml($(this));
        });

        var $selects = $(selectordiv + ' select');
        $selects.each(function () {
            strID = $(this).attr("id");
            nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1
            while (shortID.length < 3) {
                lp++;
                shortID = nam[nam.length - lp];
            }
            values += '<f t="dd"  id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
        });

        values += '</root>';

        return values;

    };


    function getctrlxml(element) {

        var values = "";
        var strID = element.attr("id");
        if (strID != undefined) {

            var nam = strID.split('_');
            var shortID = nam[nam.length - 1];
            var lp = 1
            while (shortID.length < 3) {
                lp++;
                shortID = nam[nam.length - lp];
            }
            if (element.attr("type") == 'radio') {
                values += '<f t="rb"  id="' + shortID + '" val="' + element.attr("value") + '"><![CDATA[' + element.is(':checked') + ']]></f>';
            } else if (element.attr("type") == 'checkbox') {
                values += '<f t="cb"  id="' + shortID + '" for="' + $('label[for=' + strID + ']').text() + '" val="' + element.attr("value") + '">' + element.is(':checked') + '</f>';
            } else if (element.attr("type") == 'text' || element.attr("type") == 'date' || element.attr("type") == 'email' || element.attr("type") == 'url') {
                if (element.attr("datatype") === undefined) {
                    values += '<f t="txt"  id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
                } else {
                    values += '<f t="txt"  id="' + shortID + '" dt="' + element.attr("datatype") + '"><![CDATA[' + element.val() + ']]></f>';
                }
            } else if (element.attr("type") == 'hidden') {
                values += '<f t="hid"  id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
            } else {
                values += '<f id="' + shortID + '"><![CDATA[' + element.val() + ']]></f>';
            }
        }

        return values;

    };



})(jQuery);


