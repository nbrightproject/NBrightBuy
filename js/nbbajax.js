
var nbxrtn = 'input[id*="nbxrtn"]';
var nbxaction = 'input[id*="nbxaction"]';
var nbxprocessing = 'input[id*="nbxprocessing"]';

function nbxonclick(selector,cmd,selformdiv,action)
{
	$(selector).click(function () {
		nbxget(selector,cmd,selformdiv,action);
	});
}
function nbxonchange(selector,cmd,selformdiv,action)
{		 
	$(selector).change(function () {
		nbxget(selector,cmd,selformdiv,action);
	});
}

function nbxget(selector,cmd,selformdiv,action)
{		 
    $.ajaxSetup({ cache: false });
	$(nbxaction).val($(this).id);
	$(nbxprocessing).val("BEFORE").trigger('change');
	
	// set the nbxaction field to action, so we know which ajax action is processing.
	$('input[id*="nbxaction"]').val(action)
	
	var cmdupdate = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=' + cmd;
	var values = $.fn.genxmlajax(selformdiv);
	var request = $.ajax({ type: "POST",
		url: cmdupdate,
		cache: false,
		data: { inputxml: encodeURI(values) }		
	});
	
	request.done(function (data) {
		$(nbxprocessing).val("AFTER").trigger('change');	 
		if (data != 'noaction') $(nbxrtn).val(data).trigger('change');
	});

	request.fail(function (jqXHR, textStatus) {
		alert("Request failed: " + textStatus);
	});
}

	function nbxcheckifnull(selector,searchtoken)
	{ // check if the value is empty, if so do NOT build the SQL.  Add it to disabledlist token.
		var disabledlist = $("input[id*='disabledsearchtokens']").val();
		if ($(selector).val() == '')
			disabledlist = disabledlist.replace(';' + searchtoken,'') + ';' + searchtoken;
		else
			disabledlist = disabledlist.replace(';' + searchtoken,'');
		$("input[id*='disabledsearchtokens']").val(disabledlist);
	}	