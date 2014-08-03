// NBright JQuery plugin to generate ajax post of input fields - v1.0.1
(function ($) {

	// Usage: var values = $.fn.genxmlajax(selectordiv);
	// selectordiv: The div selector whcih encapsulates the controls for whcih data will be passed tothe server.
    $.fn.genxmlajax = function (selectordiv) {
        return getgenxml(selectordiv);
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

    function getgenxml(selectordiv) {

        // get all the inputs into an array.
        var values = "<root>";

        var $inputs = $(selectordiv + ' :input');
        $inputs.each(function () {
            var strID = $(this).attr("id");
			var nam = strID.split('_');			
			var shortID = nam[nam.length - 1];
			var lp = 1
			while (shortID.length < 3)
			{
				lp++;
				shortID = nam[nam.length - lp];
			}
            if ($(this).attr("type") == 'radio') {
                values += '<f t="rb"  id="' + shortID + '" val="' + $(this).attr("value") + '"><![CDATA[' + $(this).is(':checked') + ']]></f>';
            }
            else if ($(this).attr("type") == 'checkbox') {
				values += '<f t="cb"  id="' + shortID + '" for="' + $('label[for=' + strID + ']').text() + '" val="' + $(this).attr("value") + '">' + $(this).is(':checked') + '</f>';
            }
            else if ($(this).attr("type") == 'text') {
				if ($(this).attr("datatype")===undefined){
					values += '<f t="txt"  id="' + shortID + '"><![CDATA[' + $(this).val() + ']]></f>';}
				else{
					values += '<f t="txt"  id="' + shortID + '" dt="' + $(this).attr("datatype") + '"><![CDATA[' + $(this).val() + ']]></f>';}
            }
            else if ($(this).attr("type") == 'hidden') {
					values += '<f t="hid"  id="' + shortID + '"><![CDATA[' + $(this).val() + ']]></f>';
            }
            else {			
                values += '<f id="' + shortID + '"><![CDATA[' + $(this).val() + ']]></f>';
            }
        });


		
		var $selects = $(selectordiv + ' select');
        $selects.each(function () {
            strID = $(this).attr("id");
			nam = strID.split('_');			
			var shortID = nam[nam.length - 1];
			var lp = 1
			while (shortID.length < 3)
			{
				lp++;
				shortID = nam[nam.length - lp];
			}
            values += '<f t="dd"  id="' + shortID + '" val="' + $(this).val() + '"><![CDATA[' + $('#' + strID + ' option:selected').text() + ']]></f>';
        });

        values += '</root>'

        return values;

    };

	
})(jQuery);


